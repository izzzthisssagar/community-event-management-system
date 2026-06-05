using CommunityEventManagement.Models.ViewModels;
using FluentValidation;

namespace CommunityEventManagement.Application.Validators;

/// <summary>
/// ActivityValidator checks an ActivityViewModel before it is saved. It validates the shared
/// fields that every activity has, and then uses conditional When(...) rules so that the
/// subclass-specific fields are only required for the matching activity type (for example the
/// instructor name is only required for a Workshop).
/// </summary>
public class ActivityValidator : AbstractValidator<ActivityViewModel>
{
    public ActivityValidator()
    {
        RuleFor(a => a.Title)
            .NotEmpty().WithMessage("Please enter a title.")
            .MaximumLength(200);

        RuleFor(a => a.DurationMinutes)
            .GreaterThan(0).WithMessage("The duration must be at least 1 minute.")
            .LessThanOrEqualTo(1440).WithMessage("The duration cannot be longer than a day (1440 minutes).");

        // Workshop-only rules.
        When(a => a.ActivityType == "Workshop", () =>
        {
            RuleFor(a => a.InstructorName)
                .NotEmpty().WithMessage("Please enter the instructor's name for a workshop.");
            RuleFor(a => a.MaterialsRequired)
                .NotEmpty().WithMessage("Please say what materials are required for a workshop.");
        });

        // Game-only rules.
        When(a => a.ActivityType == "Game", () =>
        {
            RuleFor(a => a.MinimumAge)
                .GreaterThanOrEqualTo(0).WithMessage("The minimum age cannot be negative.")
                .LessThanOrEqualTo(120).WithMessage("Please enter a sensible minimum age.");
        });

        // Talk-only rules.
        When(a => a.ActivityType == "Talk", () =>
        {
            RuleFor(a => a.SpeakerName)
                .NotEmpty().WithMessage("Please enter the speaker's name for a talk.");
            RuleFor(a => a.Topic)
                .NotEmpty().WithMessage("Please enter the topic for a talk.");
        });
    }
}
