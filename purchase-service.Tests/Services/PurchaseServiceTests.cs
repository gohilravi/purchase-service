using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using purchase_service.Data;
using purchase_service.Models;
using purchase_service.Models.DTOs;
using purchase_service.Services;
using purchase_service.Tests.Helpers;
using Xunit;

namespace purchase_service.Tests.Services;

public class PurchaseServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ILogger<PurchaseService>> _loggerMock;
    private readonly PurchaseService _service;

    public PurchaseServiceTests()
    {
        _context = TestDbContextHelper.CreateInMemoryDbContext();
        _loggerMock = new Mock<ILogger<PurchaseService>>();
        _service = new PurchaseService(_context, _loggerMock.Object);
    }

    [Fact]
    public async Task CreatePurchaseAsync_ValidRequest_ReturnsPurchaseResponse()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 1
        };

        // Act
        var result = await _service.CreatePurchaseAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.PurchaseId.Should().BeGreaterThan(0);

        var purchase = await _context.Purchases.FindAsync(result.PurchaseId);
        purchase.Should().NotBeNull();
        purchase!.OfferId.Should().Be(request.OfferId);
        purchase.BuyerId.Should().Be(request.BuyerId);
        purchase.StatusTypeId.Should().Be(1); // Assigned status
        purchase.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        purchase.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        // Verify logger was called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Purchase created with ID") && v.ToString()!.Contains(request.BuyerId.ToString())),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePurchaseAsync_BuyerDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 999 // Non-existent buyer
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePurchaseAsync(request));
    }

    [Fact]
    public async Task CreatePurchaseAsync_AssignedStatusNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        // Remove Assigned status
        var assignedStatus = await _context.StatusTypes.FirstOrDefaultAsync(s => s.Status == "Assigned");
        if (assignedStatus != null)
        {
            _context.StatusTypes.Remove(assignedStatus);
            await _context.SaveChangesAsync();
        }

        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 1
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreatePurchaseAsync(request));
    }

    [Fact]
    public async Task UpdatePurchaseStatusAsync_ValidRequest_UpdatesPurchaseStatus()
    {
        // Arrange
        var purchase = new Purchase
        {
            OfferId = 100,
            BuyerId = 1,
            StatusTypeId = 1, // Assigned
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        var request = new UpdatePurchaseStatusRequest
        {
            Status = "Completed"
        };

        var originalLastModified = purchase.LastModifiedAt;

        // Act
        await Task.Delay(100); // Small delay to ensure timestamp difference
        await _service.UpdatePurchaseStatusAsync(purchase.Id, request);

        // Assert
        var updatedPurchase = await _context.Purchases.FindAsync(purchase.Id);
        updatedPurchase.Should().NotBeNull();
        updatedPurchase!.StatusTypeId.Should().Be(3); // Completed status
        updatedPurchase.LastModifiedAt.Should().BeAfter(originalLastModified);

        // Verify logger was called
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Purchase") && v.ToString()!.Contains(purchase.Id.ToString()) && v.ToString()!.Contains("status updated to") && v.ToString()!.Contains(request.Status)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePurchaseStatusAsync_PurchaseDoesNotExist_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "Completed"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdatePurchaseStatusAsync(999, request));
    }

    [Fact]
    public async Task UpdatePurchaseStatusAsync_StatusNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var purchase = new Purchase
        {
            OfferId = 100,
            BuyerId = 1,
            StatusTypeId = 1,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        var request = new UpdatePurchaseStatusRequest
        {
            Status = "InvalidStatus"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdatePurchaseStatusAsync(purchase.Id, request));
    }

    [Fact]
    public async Task UpdatePurchaseStatusAsync_CaseInsensitiveStatus_UpdatesSuccessfully()
    {
        // Arrange
        var purchase = new Purchase
        {
            OfferId = 100,
            BuyerId = 1,
            StatusTypeId = 1,
            CreatedAt = DateTime.UtcNow,
            LastModifiedAt = DateTime.UtcNow
        };
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        var request = new UpdatePurchaseStatusRequest
        {
            Status = "completed" // lowercase
        };

        // Act
        await _service.UpdatePurchaseStatusAsync(purchase.Id, request);

        // Assert
        var updatedPurchase = await _context.Purchases.FindAsync(purchase.Id);
        updatedPurchase.Should().NotBeNull();
        updatedPurchase!.StatusTypeId.Should().Be(3); // Completed status
    }

    [Fact]
    public async Task UpdatePurchaseStatusAsync_OnlyUpdatesStatusAndLastModified()
    {
        // Arrange
        var purchase = new Purchase
        {
            OfferId = 100,
            BuyerId = 1,
            StatusTypeId = 1,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            LastModifiedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        var originalOfferId = purchase.OfferId;
        var originalBuyerId = purchase.BuyerId;
        var originalCreatedAt = purchase.CreatedAt;

        var request = new UpdatePurchaseStatusRequest
        {
            Status = "Canceled"
        };

        // Act
        await _service.UpdatePurchaseStatusAsync(purchase.Id, request);

        // Assert
        var updatedPurchase = await _context.Purchases.FindAsync(purchase.Id);
        updatedPurchase.Should().NotBeNull();
        updatedPurchase!.OfferId.Should().Be(originalOfferId);
        updatedPurchase.BuyerId.Should().Be(originalBuyerId);
        updatedPurchase.CreatedAt.Should().Be(originalCreatedAt);
        updatedPurchase.StatusTypeId.Should().Be(2); // Canceled
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

