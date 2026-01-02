using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using purchase_service.Contracts;
using purchase_service.Data;
using purchase_service.Models;
using purchase_service.Models.DTOs;
using purchase.Models;

namespace purchase_service.Services;

public class PurchaseService : IPurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PurchaseService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    public PurchaseService(
        ApplicationDbContext context,
        ILogger<PurchaseService> logger,
        IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<PurchaseResponse> CreatePurchaseAsync(CreatePurchaseRequest request)
    {
        // Check if buyer exists
       

        // Create new purchase
        var purchase = new Purchase
        {
            OfferId = request.OfferId,
            BuyerId = request.BuyerId,
            Status = "Assigned",
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };

        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Purchase created with ID {PurchaseId} for Buyer {BuyerId}", purchase.Id, request.BuyerId);

        // Publish command to sync record in ElasticSearch
        await _publishEndpoint.Publish(new Contracts.SyncRecordInElasticSearch
        {
            ElasticSearchId = request.OfferId.ToString(),
            ObjectType = "Purchase",
            Operation = "Create",
            Payload = JsonSerializer.Serialize(purchase)
        });

        _logger.LogInformation("Published SyncRecordInElasticSearch command for Purchase {PurchaseId}", purchase.Id);

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

        // Publish command to sync record in ElasticSearch
        await _publishEndpoint.Publish(new Contracts.SyncRecordInElasticSearch
        {
            ElasticSearchId = purchase.OfferId.ToString(),
            ObjectType = "Purchase",
            Operation = "Update",
            Payload = JsonSerializer.Serialize(purchase)
        });

        _logger.LogInformation("Published SyncRecordInElasticSearch command for Purchase {PurchaseId}", purchase.Id);
    }
}
