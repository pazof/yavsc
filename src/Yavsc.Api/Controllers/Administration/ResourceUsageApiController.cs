using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Yavsc.Abstract.Resources;
using Yavsc.Models;
using Yavsc.Server.Helpers;

namespace Yavsc.ApiControllers.Administration;

[Produces("application/json")]
[Authorize]
[Route(Constants.APIPrefix + "/" + Constants.ResourceUsagePath)]
public sealed class ResourceUsageApiController : Controller
{
    private readonly ApplicationDbContext _dbContext;

    public ResourceUsageApiController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static ResourceUsageSummary ToSummary(ResourceUsageRecord record)
        => new()
        {
            UserId = record.UserId,
            UserName = record.UserName,
            RecordedAtUtc = new DateTimeOffset(record.RecordedAtUtc, TimeSpan.Zero),
            ApiCalls = record.ApiCalls,
            CpuSeconds = record.CpuSeconds,
            BandwidthMb = record.BandwidthMb,
            StorageMb = record.StorageMb,
            EstimatedCompensation = record.EstimatedCompensation,
            Currency = record.Currency
        };

    [HttpGet(Constants.ResourceUsageCurrentUserRoute)]
    public async Task<ActionResult<ResourceUsageSummary>> GetCurrentUserUsage()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        var userName = User.Identity?.Name ?? Constants.ResourceUsageCurrentUserDisplayName;
        var record = await _dbContext.ResourceUsageRecords
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.RecordedAtUtc)
            .FirstOrDefaultAsync();

        if (record is null)
        {
            var zeroUsage = new ResourceUsageRecord
            {
                UserId = userId,
                UserName = userName,
                RecordedAtUtc = DateTime.UtcNow,
                ApiCalls = 0m,
                CpuSeconds = 0m,
                BandwidthMb = 0m,
                StorageMb = 0m,
                EstimatedCompensation = 0m,
                Currency = Constants.ResourceUsageDefaultCurrency,
                UserCreated = userId,
                UserModified = userId,
                DateCreated = DateTime.UtcNow,
                DateModified = DateTime.UtcNow,
            };

            _dbContext.ResourceUsageRecords.Add(zeroUsage);
            await _dbContext.SaveChangesAsync(userId);
            return Ok(ToSummary(zeroUsage));
        }

        return Ok(ToSummary(record));
    }

    [HttpGet(Constants.ResourceUsageAdminRoute)]
    public async Task<ActionResult<List<ResourceUsageSummary>>> GetAdminOverview()
    {
        if (!User.IsInRole(Constants.AdminGroupName))
        {
            return Forbid();
        }

        var latestByUser = (await _dbContext.ResourceUsageRecords
                .OrderByDescending(r => r.RecordedAtUtc)
                .ToListAsync())
            .GroupBy(r => r.UserId)
            .Select(g => g.First())
            .OrderByDescending(r => r.RecordedAtUtc)
            .ToList();

        return Ok(latestByUser.Select(ToSummary).ToList());
    }
}
