using Microsoft.EntityFrameworkCore;
using purchase_service.Data;
using purchase_service.Models;
using purchase_service.Models.DTOs;

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
        // Validate buyer exists
        var buyerExists = await _context.Buyers.AnyAsync(b => b.Id == request.BuyerId);
        if (!buyerExists)
        {
            throw new InvalidOperationException($"Buyer with ID {request.BuyerId} does not exist.");
        }

        // Get the "Assigned" status type
        var assignedStatus = await _context.StatusTypes
            .FirstOrDefaultAsync(s => s.Status.Equals("Assigned", StringComparison.OrdinalIgnoreCase));

        if (assignedStatus == null)
        {
            throw new InvalidOperationException("StatusType 'Assigned' not found in database.");
        }

        // Create new purchase
        var purchase = new Purchase
        {
            OfferId = request.OfferId,
            BuyerId = request.BuyerId,
            StatusTypeId = assignedStatus.Id,
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

        // Get the status type by status name
        var statusType = await _context.StatusTypes
            .FirstOrDefaultAsync(s => s.Status.Equals(request.Status, StringComparison.OrdinalIgnoreCase));

        if (statusType == null)
        {
            throw new InvalidOperationException($"StatusType '{request.Status}' not found in database.");
        }

        // Update only StatusTypeId and LastModifiedAt
        purchase.StatusTypeId = statusType.Id;
        purchase.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Purchase {PurchaseId} status updated to {Status}", purchaseId, request.Status);
    }
}
