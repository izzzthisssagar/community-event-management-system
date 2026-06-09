using CommunityEventManagement.Application.Validators;
using CommunityEventManagement.Models.ViewModels;
using FluentValidation.TestHelper;

namespace CommunityEventManagement.Tests.Application;

/// <summary>
/// These tests check my FluentValidation rules, including the boundary conditions and the
/// cross-property and conditional rules that are the main reason I chose FluentValidation over
/// simple data annotations.
/// </summary>
public class ValidatorTests
{
    private readonly EventValidator _eventValidator = new();
    private readonly ActivityValidator _activityValidator = new();
    private readonly ParticipantValidator _participantValidator = new();
    private readonly LoginViewModelValidator _loginValidator = new();

    private static EventViewModel ValidEvent() => new EventViewModel
    {
        Name = "Valid Event",
        Date = DateTime.Today.AddDays(7),
        StartTime = new TimeSpan(10, 0, 0),
        EndTime = new TimeSpan(12, 0, 0),
        Description = "A valid description",
        MaxCapacity = 50
    };

    [Fact]
    public void EventValidator_WhenEndTimeIsBeforeStartTime_HasErrorOnEndTime()
    {
        // This is the key cross-property rule.
        EventViewModel model = ValidEvent();
        model.StartTime = new TimeSpan(14, 0, 0);
        model.EndTime = new TimeSpan(13, 0, 0);

        _eventValidator.TestValidate(model).ShouldHaveValidationErrorFor(e => e.EndTime);
    }

    [Fact]
    public void EventValidator_WhenEverythingIsValid_HasNoErrors()
    {
        _eventValidator.TestValidate(ValidEvent()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void EventValidator_WhenDateIsInThePast_HasErrorOnDate()
    {
        EventViewModel model = ValidEvent();
        model.Date = DateTime.Today.AddDays(-1);

        _eventValidator.TestValidate(model).ShouldHaveValidationErrorFor(e => e.Date);
    }

    [Fact]
    public void EventValidator_WhenCapacityIsZero_HasErrorOnMaxCapacity()
    {
        // Boundary: capacity must be at least 1.
        EventViewModel model = ValidEvent();
        model.MaxCapacity = 0;

        _eventValidator.TestValidate(model).ShouldHaveValidationErrorFor(e => e.MaxCapacity);
    }

    [Fact]
    public void EventValidator_WhenCapacityIsOne_IsValid()
    {
        // Boundary: 1 is the smallest allowed capacity.
        EventViewModel model = ValidEvent();
        model.MaxCapacity = 1;

        _eventValidator.TestValidate(model).ShouldNotHaveValidationErrorFor(e => e.MaxCapacity);
    }

    [Fact]
    public void ActivityValidator_WhenWorkshopHasNoInstructor_HasError()
    {
        // Conditional rule: the instructor is only required for a Workshop.
        ActivityViewModel model = new ActivityViewModel
        {
            ActivityType = "Workshop",
            Title = "Pottery",
            DurationMinutes = 60,
            InstructorName = "",
            MaterialsRequired = "Clay"
        };

        _activityValidator.TestValidate(model).ShouldHaveValidationErrorFor(a => a.InstructorName);
    }

    [Fact]
    public void ActivityValidator_WhenTalkHasNoSpeaker_HasError()
    {
        ActivityViewModel model = new ActivityViewModel
        {
            ActivityType = "Talk",
            Title = "Climate",
            DurationMinutes = 45,
            SpeakerName = "",
            Topic = "Sustainability"
        };

        _activityValidator.TestValidate(model).ShouldHaveValidationErrorFor(a => a.SpeakerName);
    }

    [Fact]
    public void ActivityValidator_WhenGameIsValid_HasNoErrors()
    {
        ActivityViewModel model = new ActivityViewModel
        {
            ActivityType = "Game",
            Title = "Football",
            DurationMinutes = 60,
            MinimumAge = 12,
            EquipmentProvided = true
        };

        _activityValidator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ParticipantValidator_WhenEmailIsInvalid_HasErrorOnEmail()
    {
        ParticipantViewModel model = new ParticipantViewModel
        {
            FirstName = "Sam",
            LastName = "Lee",
            Email = "not-an-email",
            PhoneNumber = "0700000000"
        };

        _participantValidator.TestValidate(model).ShouldHaveValidationErrorFor(p => p.Email);
    }

    [Fact]
    public void LoginValidator_WhenPasswordIsEmpty_HasErrorOnPassword()
    {
        LoginViewModel model = new LoginViewModel { Email = "admin@events.com", Password = "" };

        _loginValidator.TestValidate(model).ShouldHaveValidationErrorFor(l => l.Password);
    }
}
