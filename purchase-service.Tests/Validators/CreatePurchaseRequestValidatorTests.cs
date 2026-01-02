using FluentAssertions;
using purchase_service.Models.DTOs;
using purchase_service.Models.DTOs.Validators;
using Xunit;

namespace purchase_service.Tests.Validators;

public class CreatePurchaseRequestValidatorTests
{
    private readonly CreatePurchaseRequestValidator _validator;

    public CreatePurchaseRequestValidatorTests()
    {
        _validator = new CreatePurchaseRequestValidator();
    }

    [Fact]
    public void Validate_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_OfferIdZero_ReturnsFailure()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 0,
            BuyerId = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OfferId");
        result.Errors.Should().Contain(e => e.ErrorMessage == "OfferId is required and must be greater than 0");
    }

    [Fact]
    public void Validate_OfferIdNegative_ReturnsFailure()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = -1,
            BuyerId = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "OfferId");
    }

    [Fact]
    public void Validate_BuyerIdZero_ReturnsFailure()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 0
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BuyerId");
        result.Errors.Should().Contain(e => e.ErrorMessage == "BuyerId is required and must be greater than 0");
    }

    [Fact]
    public void Validate_BuyerIdNegative_ReturnsFailure()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = -1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "BuyerId");
    }

    [Fact]
    public void Validate_BothIdsInvalid_ReturnsMultipleFailures()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 0,
            BuyerId = 0
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors.Should().Contain(e => e.PropertyName == "OfferId");
        result.Errors.Should().Contain(e => e.PropertyName == "BuyerId");
    }

    [Fact]
    public void Validate_OfferIdPositive_ReturnsSuccess()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 1,
            BuyerId = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_BuyerIdPositive_ReturnsSuccess()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = 100,
            BuyerId = 1
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_LargePositiveIds_ReturnsSuccess()
    {
        // Arrange
        var request = new CreatePurchaseRequest
        {
            OfferId = int.MaxValue,
            BuyerId = int.MaxValue
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}

