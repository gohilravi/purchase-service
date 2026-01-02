using FluentValidation;
using purchase_service.Models.DTOs;

namespace purchase_service.Models.DTOs.Validators;

public class CreatePurchaseRequestValidator : AbstractValidator<CreatePurchaseRequest>
{
    public CreatePurchaseRequestValidator()
    {
        RuleFor(x => x.OfferId)
            .GreaterThan(0)
            .WithMessage("OfferId is required and must be greater than 0");

        RuleFor(x => x.BuyerId)
            .GreaterThan(0)
            .WithMessage("BuyerId is required and must be greater than 0");
    }
}

