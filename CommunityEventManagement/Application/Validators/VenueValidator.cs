using CommunityEventManagement.Models.ViewModels;
using FluentValidation;

namespace CommunityEventManagement.Application.Validators;

/// <summary>
/// VenueValidator checks a VenueViewModel before it is saved.
/// </summary>
public class VenueValidator : AbstractValidator<VenueViewModel>
{
    public VenueValidator()
    {
        RuleFor(v => v.Name)
            .NotEmpty().WithMessage("Please enter a venue name.")
            .MaximumLength(200);

        RuleFor(v => v.Address)
            .NotEmpty().WithMessage("Please enter an address.")
            .MaximumLength(300);

        RuleFor(v => v.Capacity)
            .GreaterThan(0).WithMessage("The capacity must be at least 1.")
            .LessThanOrEqualTo(100000).WithMessage("The capacity looks too large, please check it.");
    }
}
