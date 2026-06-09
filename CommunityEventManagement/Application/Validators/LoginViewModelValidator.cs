using CommunityEventManagement.Models.ViewModels;
using FluentValidation;

namespace CommunityEventManagement.Application.Validators;

/// <summary>
/// LoginViewModelValidator checks the login form details. It is simple — it just makes sure an
/// email and a password were both entered.
/// </summary>
public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
{
    public LoginViewModelValidator()
    {
        RuleFor(l => l.Email)
            .NotEmpty().WithMessage("Please enter your email address.")
            .EmailAddress().WithMessage("Please enter a valid email address.");

        RuleFor(l => l.Password)
            .NotEmpty().WithMessage("Please enter your password.");
    }
}
