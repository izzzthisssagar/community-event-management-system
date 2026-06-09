using CommunityEventManagement.Models.ViewModels;
using FluentValidation;

namespace CommunityEventManagement.Application.Validators;

/// <summary>
/// SignUpViewModelValidator checks the sign-up form. As well as the usual required/email rules, it
/// has a cross-property rule that the two passwords must match.
/// </summary>
public class SignUpViewModelValidator : AbstractValidator<SignUpViewModel>
{
    public SignUpViewModelValidator()
    {
        RuleFor(s => s.FirstName)
            .NotEmpty().WithMessage("Please enter your first name.")
            .MaximumLength(100);

        RuleFor(s => s.LastName)
            .NotEmpty().WithMessage("Please enter your last name.")
            .MaximumLength(100);

        RuleFor(s => s.Email)
            .NotEmpty().WithMessage("Please enter an email address.")
            .EmailAddress().WithMessage("Please enter a valid email address.")
            .MaximumLength(200);

        RuleFor(s => s.PhoneNumber)
            .NotEmpty().WithMessage("Please enter a phone number.")
            .MaximumLength(30);

        RuleFor(s => s.Password)
            .NotEmpty().WithMessage("Please choose a password.")
            .MinimumLength(6).WithMessage("Your password must be at least 6 characters.")
            .MaximumLength(72).WithMessage("Your password cannot be longer than 72 characters.");

        // Cross-property rule: the confirmation must match the password.
        RuleFor(s => s.ConfirmPassword)
            .Equal(s => s.Password).WithMessage("The two passwords do not match.");
    }
}
