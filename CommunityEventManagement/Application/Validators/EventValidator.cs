using CommunityEventManagement.Models.ViewModels;
using FluentValidation;

namespace CommunityEventManagement.Application.Validators;

/// <summary>
/// EventValidator checks an EventViewModel before it is saved. I chose FluentValidation instead of
/// data annotations because it lets me write cross-property rules (rules that compare two fields),
/// which data annotations cannot do easily. The clearest example here is the rule that the end
/// time must be after the start time.
/// </summary>
public class EventValidator : AbstractValidator<EventViewModel>
{
    public EventValidator()
    {
        RuleFor(e => e.Name)
            .NotEmpty().WithMessage("Please enter an event name.")
            .MaximumLength(200).WithMessage("The event name cannot be longer than 200 characters.");

        RuleFor(e => e.Date)
            .GreaterThanOrEqualTo(DateTime.Today).WithMessage("The event date must be today or in the future.");

        // This is the cross-property rule: the end time is compared against the start time.
        RuleFor(e => e.EndTime)
            .GreaterThan(e => e.StartTime).WithMessage("The end time must be after the start time.");

        RuleFor(e => e.Description)
            .NotEmpty().WithMessage("Please enter a short description.")
            .MaximumLength(2000).WithMessage("The description cannot be longer than 2000 characters.");

        RuleFor(e => e.MaxCapacity)
            .GreaterThan(0).WithMessage("The capacity must be at least 1.")
            .LessThanOrEqualTo(10000).WithMessage("The capacity cannot be more than 10,000.");
    }
}
