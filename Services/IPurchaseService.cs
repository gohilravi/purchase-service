using purchase_service.Models.DTOs;

namespace purchase_service.Services;

public interface IPurchaseService
{
    Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request);
    Task UpdatePurchaseStatusAsync(int purchaseId, UpdatePurchaseStatusRequest request);
}
