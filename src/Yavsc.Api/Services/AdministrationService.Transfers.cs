using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Administration;
using Yavsc.Models;
using Yavsc.Models.Access;
using Yavsc.Models.Blog;
using Yavsc.Models.Relationship;
using Yavsc.Models.Workflow;

namespace Yavsc.Api.Services;

public sealed partial class AdministrationService
{
    public async Task<AdministrationTransfer> ExportAsync(CancellationToken ct)
    {
        var userData = await db.Users.AsNoTracking().OrderBy(user => user.Id)
            .Select(user => new UserBusinessData
            {
                Id = user.Id, UserName = user.UserName!, Email = user.Email!, FullName = user.FullName,
                Avatar = user.Avatar, DedicatedGoogleCalendar = user.DedicatedGoogleCalendar,
                AllowMonthlyEmail = user.AllowMonthlyEmail, DiskQuota = user.DiskQuota, MaxFileSize = user.MaxFileSize
            }).ToArrayAsync(ct);
        var blogs = await db.BlogSpot.AsNoTracking().Include(post => post.ACL).Include(post => post.Publication)
            .Include(post => post.Tags).ThenInclude(tag => tag.Tag).OrderBy(post => post.Id).AsSplitQuery().ToListAsync(ct);
        var activities = await db.Activities.AsNoTracking().OrderBy(activity => activity.Code)
            .Select(activity => new ActivityBusinessData
            {
                Code = activity.Code, Name = activity.Name, ParentCode = activity.ParentCode,
                Description = activity.Description, Photo = activity.Photo, Moderated = activity.Moderated,
                ModeratorGroupName = activity.ModeratorGroupName, Rate = activity.Rate,
                SettingsClassName = activity.SettingsClassName, Hidden = activity.Hidden
            }).ToArrayAsync(ct);
        return new AdministrationTransfer
        {
            Users = userData, Activities = activities,
            Blogs = blogs.Select(post => new BlogBusinessData
            {
                Id = post.Id, AuthorId = post.AuthorId!, Title = post.Title, Article = post.Article, Photo = post.Photo,
                DateCreated = Utc(post.DateCreated), Published = post.Publication is not null,
                CircleIds = post.ACL?.Select(acl => acl.CircleId).ToArray() ?? [],
                Tags = post.Tags.Select(tag => tag.Tag.Name).ToArray()
            }).ToArray()
        };
    }

    public async Task<ImportResult> ImportAsync(AdministrationTransfer transfer, CancellationToken ct)
    {
        Validate(transfer);
        foreach (var item in transfer.Users) Validate(item);
        foreach (var item in transfer.Blogs)
        {
            Validate(item);
            if (item.CircleIds is null || item.Tags is null || item.Tags.Any(string.IsNullOrWhiteSpace))
                throw Invalid("Les cercles et étiquettes d’un billet doivent être valides.");
        }
        foreach (var item in transfer.Activities) Validate(item);
        Unique(transfer.Users.Select(user => user.Id));
        Unique(transfer.Users.Select(user => user.UserName.ToUpperInvariant()));
        Unique(transfer.Blogs.Select(post => post.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)));
        Unique(transfer.Activities.Select(activity => activity.Code));

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (transfer.Blogs.Length > 0 && db.Database.IsNpgsql())
            await db.Database.ExecuteSqlRawAsync("""LOCK TABLE "BlogSpot" IN SHARE ROW EXCLUSIVE MODE""", ct);
        foreach (var item in transfer.Users)
        {
            ct.ThrowIfCancellationRequested();
            var user = await users.FindByIdAsync(item.Id);
            var isNew = user is null;
            if (user is not null && user.Email != item.Email)
                throw Conflict("L’adresse e-mail d’un compte existant est une clé métier ; elle ne peut pas changer par import.");
            user ??= new ApplicationUser { Id = item.Id };
            if (user.Email != item.Email)
                user.EmailConfirmed = false;
            user.UserName = item.UserName;
            user.Email = item.Email;
            user.FullName = item.FullName;
            user.Avatar = item.Avatar;
            user.DedicatedGoogleCalendar = item.DedicatedGoogleCalendar;
            user.AllowMonthlyEmail = item.AllowMonthlyEmail;
            user.DiskQuota = item.DiskQuota;
            user.MaxFileSize = item.MaxFileSize;
            CheckIdentity(isNew ? await users.CreateAsync(user) : await users.UpdateAsync(user));
        }

        var activities = await db.Activities.ToDictionaryAsync(activity => activity.Code, ct);
        foreach (var item in transfer.Activities)
        {
            if (!activities.TryGetValue(item.Code, out var activity))
            {
                activity = new Activity { Code = item.Code };
                activities.Add(item.Code, activity);
                db.Activities.Add(activity);
            }
            activity.Name = item.Name;
            activity.ParentCode = item.ParentCode;
            activity.Description = item.Description;
            activity.Photo = item.Photo;
            activity.Moderated = item.Moderated;
            activity.ModeratorGroupName = item.ModeratorGroupName;
            activity.Rate = item.Rate;
            activity.SettingsClassName = item.SettingsClassName;
            activity.Hidden = item.Hidden;
        }
        foreach (var item in transfer.Activities)
        {
            var visited = new HashSet<string> { item.Code };
            var parent = item.ParentCode;
            while (parent is not null)
            {
                if (!visited.Add(parent)) throw Invalid("Le graphe des activités contient un cycle.");
                if (!activities.TryGetValue(parent, out var ancestor)) throw Invalid($"Activité parent inconnue : {parent}.");
                parent = ancestor.ParentCode;
            }
        }
        await db.SaveChangesAsync(Actor, ct);

        var existingTags = await db.Tags.OrderBy(tag => tag.Id).ToListAsync(ct);
        var tags = existingTags.GroupBy(tag => tag.Name).ToDictionary(group => group.Key, group => group.First());
        foreach (var item in transfer.Blogs)
        {
            if (!await db.Users.AnyAsync(user => user.Id == item.AuthorId, ct))
                throw Invalid($"Auteur inconnu : {item.AuthorId}.");
            foreach (var circleId in item.CircleIds.Distinct())
            {
                if (!await db.Circle.AnyAsync(circle => circle.Id == circleId, ct))
                    throw Invalid($"Cercle inconnu : {circleId}. Les droits du billet ne peuvent pas être élargis.");
            }
            var post = await db.BlogSpot.Include(post => post.ACL).Include(post => post.Tags)
                .Include(post => post.Publication).SingleOrDefaultAsync(post => post.Id == item.Id, ct);
            if (post is null)
            {
                post = new BlogPost { Id = item.Id, ACL = [], Tags = [] };
                db.BlogSpot.Add(post);
            }
            else if (post.AuthorId != item.AuthorId)
                throw Conflict("Un import ne peut pas changer l’auteur d’un billet existant.");
            post.AuthorId = item.AuthorId;
            post.Title = item.Title;
            post.Article = item.Article;
            post.Photo = item.Photo;
            post.DateCreated = item.DateCreated.UtcDateTime;
            post.DateModified = DateTime.UtcNow;
            post.UserModified = Actor;
            post.UserCreated ??= item.AuthorId;
            post.ACL ??= [];
            post.Tags ??= [];
            db.RemoveRange(post.ACL);
            post.ACL = item.CircleIds.Distinct().Select(id => new CircleAuthorizationToBlogPost { CircleId = id }).ToList();
            db.RemoveRange(post.Tags);
            post.Tags = item.Tags.Distinct().Select(name =>
            {
                if (!tags.TryGetValue(name, out var tag))
                {
                    tag = new Tag { Name = name };
                    tags.Add(name, tag);
                }
                return new BlogTag { Tag = tag };
            }).ToList();
            if (item.Published && post.Publication is null)
                post.Publication = new BlogSpotPublication();
            else if (!item.Published && post.Publication is not null)
            {
                db.Remove(post.Publication);
                post.Publication = null!;
            }
        }
        await db.SaveChangesAsync(ct);
        if (transfer.Blogs.Length > 0 && db.Database.IsNpgsql())
        {
            // Explicit imported IDs must not collide with subsequently generated IDs.
            await db.Database.ExecuteSqlRawAsync("""
                SELECT setval(pg_get_serial_sequence('"BlogSpot"', 'Id'),
                    GREATEST(COALESCE((SELECT MAX("Id") FROM "BlogSpot"), 1),
                        COALESCE(pg_sequence_last_value(pg_get_serial_sequence('"BlogSpot"', 'Id')::regclass), 1)), true)
                """, ct);
        }
        await transaction.CommitAsync(ct);
        return new(transfer.Users.Length, transfer.Blogs.Length, transfer.Activities.Length);
    }

    private static void Validate(object? value)
    {
        if (value is null) throw Invalid("Une ligne de transfert ne peut pas être nulle.");
        var errors = new List<ValidationResult>();
        if (!Validator.TryValidateObject(value, new ValidationContext(value), errors, true))
            throw Invalid(string.Join("; ", errors.Select(error => error.ErrorMessage)));
    }

    private static void Unique(IEnumerable<string> keys)
    {
        var values = keys.ToArray();
        if (values.Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw Invalid("Le transfert contient des identifiants en double.");
    }
}
