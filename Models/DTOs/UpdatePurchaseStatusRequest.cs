using System.Diagnostics.CodeAnalysis;

namespace purchase_service.Models.DTOs;

[ExcludeFromCodeCoverage]
public class UpdatePurchaseStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

