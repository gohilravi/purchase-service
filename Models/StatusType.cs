using System.Diagnostics.CodeAnalysis;

namespace purchase_service.Models;

[ExcludeFromCodeCoverage]
public class StatusType
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModifiedAt { get; set; }
}

