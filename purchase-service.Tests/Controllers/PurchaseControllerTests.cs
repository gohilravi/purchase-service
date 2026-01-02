using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using purchase_service.Controllers;
using purchase_service.Models.DTOs;
using purchase_service.Services;
using purchase_service.Tests.Helpers;
using Xunit;

namespace purchase_service.Tests.Controllers;

public class PurchaseControllerTests
{
    private readonly Mock<IPurchaseService> _purchaseServiceMock;
    private readonly Mock<ILogger<PurchaseController>> _loggerMock;
    private readonly Mock<IValidator<CreatePurchaseRequest>> _createValidatorMock;
    private readonly Mock<IValidator<UpdatePurchaseStatusRequest>> _updateValidatorMock;
    private readonly PurchaseController _controller;

    public PurchaseControllerTests()
    {
        _purchaseServiceMock = new Mock<IPurchaseService>();
        _loggerMock = new Mock<ILogger<PurchaseController>>();
        _createValidatorMock = new Mock<IValidator<CreatePurchaseRequest>>();
        _updateValidatorMock = new Mock<IValidator<UpdatePurchaseStatusRequest>>();

        _controller = new PurchaseController(
            _purchaseServiceMock.Object,
            _loggerMock.Object,
            _createValidatorMock.Object,
            _updateValidatorMock.Object);
    }

    #region CreatePurchase Tests

    [Fact]
    public async Task CreatePurchase_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 1
        };

        var response = new PurchaseResponse { PurchaseId = 1 };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreatePurchaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _purchaseServiceMock
            .Setup(s => s.CreatePurchaseAsync(It.IsAny<CreatePurchaseRequest>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreatePurchase(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = result as CreatedAtActionResult;
        createdAtResult!.StatusCode.Should().Be(201);
        createdAtResult.Value.Should().BeOfType<ApiResponse<PurchaseResponse>>();

        var apiResponse = createdAtResult.Value as ApiResponse<PurchaseResponse>;
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Data!.PurchaseId.Should().Be(1);
        apiResponse.Message.Should().Be("Purchase created successfully");

        // Verify service was called
        _purchaseServiceMock.Verify(
            x => x.CreatePurchaseAsync(It.Is<CreatePurchaseRequest>(r => r.OfferId == request.OfferId && r.BuyerId == request.BuyerId)),
            Times.Once);

        // Verify validator was called
        _createValidatorMock.Verify(
            x => x.ValidateAsync(It.Is<CreatePurchaseRequest>(r => r.OfferId == request.OfferId && r.BuyerId == request.BuyerId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePurchase_ValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 0,
            BuyerId = 0
        };

        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("OfferId", "OfferId is required and must be greater than 0"),
            new ValidationFailure("BuyerId", "BuyerId is required and must be greater than 0")
        };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreatePurchaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationErrors));

        // Act
        var result = await _controller.CreatePurchase(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.StatusCode.Should().Be(400);

        var apiResponse = badRequestResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Validation failed");
        apiResponse.Errors.Should().HaveCount(2);

        // Verify validator was called
        _createValidatorMock.Verify(
            x => x.ValidateAsync(It.IsAny<CreatePurchaseRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify service was NOT called when validation fails
        _purchaseServiceMock.Verify(
            x => x.CreatePurchaseAsync(It.IsAny<CreatePurchaseRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task CreatePurchase_BuyerDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 999
        };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreatePurchaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _purchaseServiceMock
            .Setup(s => s.CreatePurchaseAsync(It.IsAny<CreatePurchaseRequest>()))
            .ThrowsAsync(new InvalidOperationException("Buyer with ID 999 does not exist."));

        // Act
        var result = await _controller.CreatePurchase(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.StatusCode.Should().Be(400);

        var apiResponse = badRequestResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Buyer with ID 999 does not exist.");

        // Verify service was called
        _purchaseServiceMock.Verify(
            x => x.CreatePurchaseAsync(It.Is<CreatePurchaseRequest>(r => r.BuyerId == 999)),
            Times.Once);

        // Verify logger was called for warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid operation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreatePurchase_GenericException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 1
        };

        _createValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<CreatePurchaseRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _purchaseServiceMock
            .Setup(s => s.CreatePurchaseAsync(It.IsAny<CreatePurchaseRequest>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.CreatePurchase(request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);

        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("An error occurred while creating the purchase");

        // Verify service was called
        _purchaseServiceMock.Verify(
            x => x.CreatePurchaseAsync(It.IsAny<CreatePurchaseRequest>()),
            Times.Once);

        // Verify logger was called for error
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error creating purchase")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    #region UpdatePurchaseStatus Tests

    [Fact]
    public async Task UpdatePurchaseStatus_ValidRequest_ReturnsOk()
    {
        // Arrange
        var purchaseId = 1;
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "Completed"
        };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdatePurchaseStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _purchaseServiceMock
            .Setup(s => s.UpdatePurchaseStatusAsync(It.IsAny<int>(), It.IsAny<UpdatePurchaseStatusRequest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.UpdatePurchaseStatus(purchaseId, request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var apiResponse = okResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeTrue();
        apiResponse.Message.Should().Be("Purchase status updated successfully");

        // Verify service was called
        _purchaseServiceMock.Verify(
            x => x.UpdatePurchaseStatusAsync(
                It.Is<int>(id => id == purchaseId),
                It.Is<UpdatePurchaseStatusRequest>(r => r.Status == request.Status)),
            Times.Once);

        // Verify validator was called
        _updateValidatorMock.Verify(
            x => x.ValidateAsync(It.Is<UpdatePurchaseStatusRequest>(r => r.Status == request.Status), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePurchaseStatus_ValidationFails_ReturnsBadRequest()
    {
        // Arrange
        var purchaseId = 1;
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "InvalidStatus"
        };

        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Status", "Status must be one of: Assigned, Canceled, Completed")
        };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdatePurchaseStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationErrors));

        // Act
        var result = await _controller.UpdatePurchaseStatus(purchaseId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.StatusCode.Should().Be(400);

        var apiResponse = badRequestResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Validation failed");
        apiResponse.Errors.Should().HaveCount(1);

        // Verify validator was called
        _updateValidatorMock.Verify(
            x => x.ValidateAsync(It.IsAny<UpdatePurchaseStatusRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);

        // Verify service was NOT called when validation fails
        _purchaseServiceMock.Verify(
            x => x.UpdatePurchaseStatusAsync(It.IsAny<int>(), It.IsAny<UpdatePurchaseStatusRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdatePurchaseStatus_PurchaseDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var purchaseId = 999;
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "Completed"
        };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdatePurchaseStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _purchaseServiceMock
            .Setup(s => s.UpdatePurchaseStatusAsync(It.IsAny<int>(), It.IsAny<UpdatePurchaseStatusRequest>()))
            .ThrowsAsync(new InvalidOperationException("Purchase with ID 999 does not exist."));

        // Act
        var result = await _controller.UpdatePurchaseStatus(purchaseId, request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);

        var apiResponse = notFoundResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("Purchase with ID 999 does not exist.");

        // Verify service was called
        _purchaseServiceMock.Verify(
            x => x.UpdatePurchaseStatusAsync(It.Is<int>(id => id == purchaseId), It.IsAny<UpdatePurchaseStatusRequest>()),
            Times.Once);

        // Verify logger was called for warning
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid operation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdatePurchaseStatus_StatusNotFound_ReturnsBadRequest()
    {
        // Arrange
        var purchaseId = 1;
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "NonExistentStatus"
        };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdatePurchaseStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _purchaseServiceMock
            .Setup(s => s.UpdatePurchaseStatusAsync(It.IsAny<int>(), It.IsAny<UpdatePurchaseStatusRequest>()))
            .ThrowsAsync(new InvalidOperationException("StatusType 'NonExistentStatus' not found in database."));

        // Act
        var result = await _controller.UpdatePurchaseStatus(purchaseId, request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.StatusCode.Should().Be(400);

        var apiResponse = badRequestResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("StatusType 'NonExistentStatus' not found in database.");
    }

    [Fact]
    public async Task UpdatePurchaseStatus_GenericException_ReturnsInternalServerError()
    {
        // Arrange
        var purchaseId = 1;
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "Completed"
        };

        _updateValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<UpdatePurchaseStatusRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _purchaseServiceMock
            .Setup(s => s.UpdatePurchaseStatusAsync(It.IsAny<int>(), It.IsAny<UpdatePurchaseStatusRequest>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.UpdatePurchaseStatus(purchaseId, request);

        // Assert
        result.Should().BeOfType<ObjectResult>();
        var objectResult = result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);

        var apiResponse = objectResult.Value as ApiResponse<object>;
        apiResponse!.Success.Should().BeFalse();
        apiResponse.Message.Should().Be("An error occurred while updating the purchase status");

        // Verify service was called
        _purchaseServiceMock.Verify(
            x => x.UpdatePurchaseStatusAsync(It.IsAny<int>(), It.IsAny<UpdatePurchaseStatusRequest>()),
            Times.Once);

        // Verify logger was called for error
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error updating purchase status")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}

