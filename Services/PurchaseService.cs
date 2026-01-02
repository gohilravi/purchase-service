using Microsoft.EntityFrameworkCore;
using purchase_service.Data;
using purchase_service.Models;
using purchase_service.Models.DTOs;
using purchase.Models;

namespace purchase_service.Services;

public class PurchaseService : IPurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseService(ApplicationDbContext context, ILogger<PurchaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request)
    {
        // Check if buyer exists
        var buyer = await _context.Buyers.FindAsync(request.BuyerId);
        if (buyer == null)
            throw new InvalidOperationException($"Buyer with ID {request.BuyerId} does not exist.");

        // Get the "Assigned" status type
        var assignedStatus = await _context.StatusTypes.FirstOrDefaultAsync(s => s.Status == "Assigned");
        if (assignedStatus == null)
            throw new InvalidOperationException("Assigned status not found.");

        // Create new purchase
        var purchase = new Purchase
        {
            OfferId = request.OfferId,
            BuyerId = request.BuyerId,
            Status = assignedStatus.Status,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Purchase created with ID {PurchaseId} for Buyer {BuyerId}", purchase.Id, request.BuyerId);

        return new PurchaseResponse
        {
            PurchaseId = purchase.Id
        };
    }

    public async Task UpdatePurchaseStatusAsync(int purchaseId, UpdatePurchaseStatusRequest request)
    {
        // Validate purchase exists
        var purchase = await _context.Purchases.FindAsync(purchaseId);
        if (purchase == null)
        {
            throw new InvalidOperationException($"Purchase with ID {purchaseId} does not exist.");
        }

        // Find status type (case-insensitive)
        var statusType = await _context.StatusTypes.FirstOrDefaultAsync(s => s.Status.ToLower() == request.Status.ToLower());
        if (statusType == null)
            throw new InvalidOperationException($"Status '{request.Status}' not found.");

        // Update only Status and LastModifiedAt
        purchase.Status = statusType.Status;
        purchase.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Purchase {PurchaseId} status updated to {Status}", purchaseId, statusType.Status);
    }
}
