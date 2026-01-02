using FluentAssertions;
using purchase_service.Models.DTOs;
using purchase_service.Models.DTOs.Validators;
using Xunit;

namespace purchase_service.Tests.Validators;

public class UpdatePurchaseStatusRequestValidatorTests
{
    private readonly UpdatePurchaseStatusRequestValidator _validator;

    public UpdatePurchaseStatusRequestValidatorTests()
    {
        _validator = new UpdatePurchaseStatusRequestValidator();
    }

    [Theory]
    [InlineData("Assigned")]
    [InlineData("Canceled")]
    [InlineData("Completed")]
    [InlineData("assigned")]
    [InlineData("canceled")]
    [InlineData("completed")]
    [InlineData("ASSIGNED")]
    [InlineData("CANCELED")]
    [InlineData("COMPLETED")]
    [InlineData("AsSiGnEd")]
    public void Validate_ValidStatus_ReturnsSuccess(string status)
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = status
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_EmptyStatus_ReturnsFailure()
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = string.Empty
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
        result.Errors.Should().Contain(e => e.ErrorMessage == "Status is required");
    }

    [Fact]
    public void Validate_NullStatus_ReturnsFailure()
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = null!
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Theory]
    [InlineData("InvalidStatus")]
    [InlineData("Pending")]
    [InlineData("Active")]
    [InlineData("InProgress")]
    [InlineData(" ")]
    [InlineData("Assigned ")]
    [InlineData(" Assigned")]
    public void Validate_InvalidStatus_ReturnsFailure(string status)
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = status
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Status must be one of:"));
    }

    [Fact]
    public void Validate_WhitespaceStatus_ReturnsFailure()
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "   "
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Fact]
    public void Validate_StatusWithExtraCharacters_ReturnsFailure()
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "AssignedExtra"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Status");
    }

    [Fact]
    public void Validate_ErrorMessageContainsAllowedStatuses()
    {
        // Arrange
        var request = new UpdatePurchaseStatusRequest
        {
            Status = "Invalid"
        };

        // Act
        var result = _validator.Validate(request);

        // Assert
        result.IsValid.Should().BeFalse();
        var error = result.Errors.First(e => e.PropertyName == "Status");
        error.ErrorMessage.Should().Contain("Assigned");
        error.ErrorMessage.Should().Contain("Canceled");
        error.ErrorMessage.Should().Contain("Completed");
    }
}

