using CommunityEventManagement.Models.ViewModels;
using FluentValidation;

namespace CommunityEventManagement.Application.Validators;

/// <summary>
/// ParticipantValidator checks a ParticipantViewModel before it is saved. It makes sure the names
/// are filled in, the email looks like a real email address, and the phone number is sensible.
/// </summary>
public class ParticipantValidator : AbstractValidator<ParticipantViewModel>
{
    public ParticipantValidator()
    {
        RuleFor(p => p.FirstName)
            .NotEmpty().WithMessage("Please enter a first name.")
            .MaximumLength(100);

        RuleFor(p => p.LastName)
            .NotEmpty().WithMessage("Please enter a last name.")
            .MaximumLength(100);

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("Please enter an email address.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(200);

        RuleFor(p => p.PhoneNumber)
            .NotEmpty().WithMessage("Please enter a phone number.")
            .MaximumLength(30)
            .Matches(@"^[0-9+()\-\s]+$").WithMessage("The phone number can only contain digits, spaces and the characters + ( ) -.");
    }
}
