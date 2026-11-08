#nullable enable
using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models;

public sealed class ResourceStorageRecord
{
    [MaxLength(450)]
    public string UserId { get; set; } = "";
    [MaxLength(1024)]
    public string Path { get; set; } = "";
    public long Bytes { get; set; }
}
