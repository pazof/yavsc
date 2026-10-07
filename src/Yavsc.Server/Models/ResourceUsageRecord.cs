using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Models;

public sealed class ResourceUsageRecord : ITrackedEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public DateTime RecordedAtUtc { get; set; } = DateTime.UtcNow;

    public decimal ApiCalls { get; set; }
    public decimal CpuSeconds { get; set; }
    public decimal BandwidthMb { get; set; }
    public decimal StorageMb { get; set; }
    public decimal EstimatedCompensation { get; set; }

    [Required]
    [MaxLength(8)]
    public string Currency { get; set; } = Constants.ResourceUsageDefaultCurrency;

    public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    public string UserCreated { get; set; } = string.Empty;
    public DateTime DateModified { get; set; } = DateTime.UtcNow;
    public string UserModified { get; set; } = string.Empty;
}
