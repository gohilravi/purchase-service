using FluentValidation;
using purchase_service.Models.DTOs;

namespace purchase_service.Models.DTOs.Validators;

public class UpdatePurchaseStatusRequestValidator : AbstractValidator<UpdatePurchaseStatusRequest>
{
    private static readonly string[] AllowedStatuses = { "Assigned", "Canceled", "Completed" };

    public UpdatePurchaseStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required")
            .Must(status => AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}");
    }
}

