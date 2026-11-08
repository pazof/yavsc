#nullable enable
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Yavsc.Models;
using Yavsc.Server.Helpers;
using Yavsc.Services;

namespace Yavsc.Web.Services;

public interface IAccountWithdrawalService
{
    Task WithdrawAsync(string userId, CancellationToken ct);
}

public sealed class AccountWithdrawalService(ApplicationDbContext db, UserManager<ApplicationUser> users,
    IPayPalSubscriptionGateway paypal, IOptions<SiteSettings> site, IOptions<PodcastOptions> podcasts,
    TimeProvider clock, ILogger<AccountWithdrawalService> logger) : IAccountWithdrawalService
{
    public async Task WithdrawAsync(string userId, CancellationToken ct)
    {
        var user = await users.FindByIdAsync(userId)
            ?? throw new SubscriptionOperationException(404, "Compte introuvable.");
        if (user.WithdrawnAt is not null) return;
        var userName = user.UserName ?? throw new InvalidOperationException("Withdrawal requires a storage owner name.");
        var personalDirectory = Child(site.Value.UserFilesRoot, userName);
        var avatarFiles = new[] { ".png", ".s.png", ".xs.png" }.Select(ext => Child(site.Value.Avatars, userName + ext)).ToArray();
        var started = await ResourceUsageTracker.WithUserLockAsync(db, userId, async () =>
        {
            await db.Entry(user).ReloadAsync(ct);
            if (user.WithdrawnAt is not null) return false;
            user.WithdrawalRequestedAt ??= clock.GetUtcNow();
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            user.SecurityStamp = Guid.NewGuid().ToString();
            await db.SaveChangesAsync(ct);
            return true;
        }, ct);
        if (!started) return;

        var subscriptions = await db.ResourceSubscriptions.Where(s => s.UserId == userId).ToListAsync(ct);
        foreach (var subscription in subscriptions)
        {
            if (subscription.State is "CANCELLED" or "EXPIRED") continue;
            if (subscription.PayPalId is null)
            {
                if (subscription.State == "CREATING")
                    throw new SubscriptionOperationException(409, "Une creation PayPal doit etre rapprochee avant de confirmer le retrait.");
            }
            else
            {
                var remote = await paypal.GetAsync(subscription.PayPalId, ct);
                if (remote.State is not ("CANCELLED" or "EXPIRED"))
                    await paypal.CancelAsync(subscription.PayPalId, ct);
            }
            subscription.State = "CANCELLED";
            subscription.NextBillingAt = null;
            await db.SaveChangesAsync(ct);
        }

        DeleteDirectory(personalDirectory);
        foreach (var path in avatarFiles)
        {
            RejectLink(path);
            if (Directory.Exists(Path.GetDirectoryName(path))) File.Delete(path);
        }
        await DeletePodcastsAsync(userId, ct);

        await ResourceUsageTracker.WithUserLockAsync(db, userId, async () =>
        {
            await db.Entry(user).ReloadAsync(ct);
            if (user.WithdrawnAt is not null) return true;
            db.DeviceDeclaration.RemoveRange(await db.DeviceDeclaration.Where(d => d.DeviceOwnerId == userId).ToListAsync(ct));
            db.UserLogins.RemoveRange(await db.UserLogins.Where(d => d.UserId == userId).ToListAsync(ct));
            db.UserTokens.RemoveRange(await db.UserTokens.Where(d => d.UserId == userId).ToListAsync(ct));
            db.UserClaims.RemoveRange(await db.UserClaims.Where(d => d.UserId == userId).ToListAsync(ct));
            db.UserRoles.RemoveRange(await db.UserRoles.Where(d => d.UserId == userId).ToListAsync(ct));
            db.BankIdentity.RemoveRange(await db.BankIdentity.Where(d => d.UserId == userId).ToListAsync(ct));
            db.Contact.RemoveRange(await db.Contact.Where(d => d.OwnerId == userId || d.UserId == userId).ToListAsync(ct));
            db.ResourceUsageRecords.RemoveRange(await db.ResourceUsageRecords.Where(d => d.UserId == userId).ToListAsync(ct));
            db.ResourceStorageRecords.RemoveRange(await db.ResourceStorageRecords.Where(d => d.UserId == userId).ToListAsync(ct));
            var posts = await db.BlogSpot.Where(p => p.AuthorId == userId).ToListAsync(ct);
            foreach (var post in posts)
            {
                post.Title = "Contenu retire";
                post.Article = null;
                post.Photo = null;
            }
            var comments = await db.Comment.Where(p => p.AuthorId == userId).ToListAsync(ct);
            foreach (var comment in comments) comment.Article = "Contenu retire";
            var performer = await db.Performers.SingleOrDefaultAsync(p => p.PerformerId == userId, ct);
            if (performer is not null)
            {
                performer.Active = false;
                performer.AcceptNotifications = false;
                performer.AcceptPublicContact = false;
                performer.UseGeoLocalizationToReduceDistanceWithClients = false;
                performer.WebSite = "";
                performer.DonationPayPalEmail = null;
            }
            var flows = await db.LiveFlow.Where(f => f.OwnerId == userId).ToListAsync(ct);
            foreach (var flow in flows)
            {
                flow.Title = "Contenu retire";
                flow.Pitch = "";
                flow.PublicAnnouncement = "";
                flow.DifferedFileName = "";
                flow.PodcastDeleted = true;
                flow.ObsKeyHash = null;
                flow.ObsEnabledUntil = null;
                flow.ObsConnectionId = null;
                flow.ObsLastError = null;
            }
            var fileIds = await db.UploadedFiles.Where(f => f.OwnerId == userId
                || db.BlogAttachedFiles.Any(a => a.FileId == f.Id && a.Post.AuthorId == userId))
                .Select(f => f.Id).ToListAsync(ct);
            var files = await db.UploadedFiles.Where(f => fileIds.Contains(f.Id)).ToListAsync(ct);
            foreach (var file in files)
            {
                file.Path = "";
                file.Length = 0;
                file.ContentType = "application/octet-stream";
            }
            user.UserName = "withdrawn-" + Guid.NewGuid().ToString("N");
            user.NormalizedUserName = user.UserName.ToUpperInvariant();
            user.Email = user.UserName + "@invalid.invalid";
            user.NormalizedEmail = user.Email.ToUpperInvariant();
            user.EmailConfirmed = false;
            user.FullName = null;
            user.PhoneNumber = null;
            user.PhoneNumberConfirmed = false;
            user.PasswordHash = null;
            user.TwoFactorEnabled = false;
            user.PostalAddressId = null;
            user.DedicatedGoogleCalendar = null;
            user.Avatar = Constants.DefaultAvatar;
            user.AllowMonthlyEmail = false;
            user.DiskUsage = 0;
            user.PremiumDiskQuotaUntil = null;
            user.DiscoveryDiskQuotaUntil = null;
            user.WithdrawnAt = clock.GetUtcNow();
            await db.SaveChangesAsync(ct);
            return true;
        }, ct);
        logger.LogInformation("Account withdrawal completed, files erased and identity pseudonymized");
    }

    private async Task DeletePodcastsAsync(string userId, CancellationToken ct)
    {
        var root = Path.GetFullPath(podcasts.Value.RootPath);
        if (!Directory.Exists(root)) return;
        RejectLink(root);
        foreach (var directory in Directory.EnumerateDirectories(root))
        {
            if (!Guid.TryParseExact(Path.GetFileName(directory), "N", out _)) continue;
            RejectLink(directory);
            var index = Path.Combine(directory, "index.json");
            RejectLink(index);
            if (!File.Exists(index)) continue;
            await using var stream = File.OpenRead(index);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var podcast = document.RootElement.GetProperty("Manifest").GetProperty("Podcast");
            var owner = podcast.GetProperty("OwnerId").GetString();
            if (owner == userId)
            {
                if (podcast.TryGetProperty("State", out var state) && state.GetString() == "Live"
                    || podcast.TryGetProperty("Replay", out var replay) && replay.ValueKind == JsonValueKind.Object
                        && replay.TryGetProperty("State", out var replayState) && replayState.GetString() == "Processing")
                    throw new SubscriptionOperationException(409, "Un podcast est encore en direct ou en cours de conversion ; attendre son arret avant de reprendre le retrait.");
                await stream.DisposeAsync();
                DeleteDirectory(directory);
            }
        }
    }

    private static string Child(string root, string name)
    {
        if (string.IsNullOrWhiteSpace(root) || string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name)
            || name is "." or ".." || name.Contains('\\'))
            throw new InvalidOperationException("Invalid withdrawal storage configuration or user path.");
        var fullRoot = Path.GetFullPath(root);
        RejectLink(fullRoot);
        var child = Path.GetFullPath(Path.Combine(fullRoot, name));
        if (!child.StartsWith(fullRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            throw new InvalidOperationException("Withdrawal path escapes its storage root.");
        return child;
    }

    private static void RejectLink(string path)
    {
        if ((File.Exists(path) || Directory.Exists(path)) && File.GetAttributes(path).HasFlag(FileAttributes.ReparsePoint))
            throw new IOException("Withdrawal refuses symbolic links in managed storage.");
    }

    private static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        RejectLink(path);
        foreach (var entry in Directory.EnumerateFileSystemEntries(path))
        {
            RejectLink(entry);
            if (Directory.Exists(entry)) DeleteDirectory(entry);
            else File.Delete(entry);
        }
        Directory.Delete(path);
    }
}
