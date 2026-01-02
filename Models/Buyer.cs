using System.Diagnostics.CodeAnalysis;

namespace purchase_service.Models;

[ExcludeFromCodeCoverage]
public class Buyer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

