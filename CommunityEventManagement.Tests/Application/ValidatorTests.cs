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

    // ----- EventValidator — additional boundary and field tests -----

    [Fact]
    public void EventValidator_WhenNameIsEmpty_HasErrorOnName()
    {
        EventViewModel model = ValidEvent();
        model.Name = "";

        _eventValidator.TestValidate(model).ShouldHaveValidationErrorFor(e => e.Name);
    }

    [Fact]
    public void EventValidator_WhenDescriptionIsEmpty_HasErrorOnDescription()
    {
        EventViewModel model = ValidEvent();
        model.Description = "";

        _eventValidator.TestValidate(model).ShouldHaveValidationErrorFor(e => e.Description);
    }

    [Fact]
    public void EventValidator_WhenDateIsToday_IsValid()
    {
        // Boundary: today should be accepted (the rule is >= today, not > today).
        EventViewModel model = ValidEvent();
        model.Date = DateTime.Today;

        _eventValidator.TestValidate(model).ShouldNotHaveValidationErrorFor(e => e.Date);
    }

    [Fact]
    public void EventValidator_WhenStartTimeEqualsEndTime_HasErrorOnEndTime()
    {
        // Boundary: an event cannot start and end at the same time.
        EventViewModel model = ValidEvent();
        model.StartTime = new TimeSpan(10, 0, 0);
        model.EndTime = new TimeSpan(10, 0, 0);

        _eventValidator.TestValidate(model).ShouldHaveValidationErrorFor(e => e.EndTime);
    }

    [Fact]
    public void EventValidator_WhenNegativeCapacity_HasErrorOnMaxCapacity()
    {
        EventViewModel model = ValidEvent();
        model.MaxCapacity = -1;

        _eventValidator.TestValidate(model).ShouldHaveValidationErrorFor(e => e.MaxCapacity);
    }

    // ----- ActivityValidator — duration boundary -----

    [Fact]
    public void ActivityValidator_WhenDurationIsZero_HasError()
    {
        // Boundary: a zero-minute activity is meaningless.
        ActivityViewModel model = new ActivityViewModel
        {
            ActivityType = "Game",
            Title = "Chess",
            DurationMinutes = 0,
            MinimumAge = 0,
            EquipmentProvided = false
        };

        _activityValidator.TestValidate(model).ShouldHaveValidationErrorFor(a => a.DurationMinutes);
    }

    [Fact]
    public void ActivityValidator_WhenWorkshopHasInstructor_HasNoErrors()
    {
        // Boundary: a fully filled-out Workshop must pass all rules.
        ActivityViewModel model = new ActivityViewModel
        {
            ActivityType = "Workshop",
            Title = "Pottery",
            DurationMinutes = 60,
            InstructorName = "Jane",
            MaterialsRequired = "Clay"
        };

        _activityValidator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void ActivityValidator_WhenTalkHasSpeakerAndTopic_HasNoErrors()
    {
        ActivityViewModel model = new ActivityViewModel
        {
            ActivityType = "Talk",
            Title = "Climate",
            DurationMinutes = 45,
            SpeakerName = "Dr Green",
            Topic = "Sustainability"
        };

        _activityValidator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    // ----- ParticipantValidator — additional fields -----

    [Fact]
    public void ParticipantValidator_WhenFirstNameIsEmpty_HasError()
    {
        ParticipantViewModel model = new ParticipantViewModel
        {
            FirstName = "",
            LastName = "Lee",
            Email = "sam@example.com",
            PhoneNumber = "0700000000"
        };

        _participantValidator.TestValidate(model).ShouldHaveValidationErrorFor(p => p.FirstName);
    }

    [Fact]
    public void ParticipantValidator_WhenAllFieldsAreValid_HasNoErrors()
    {
        ParticipantViewModel model = new ParticipantViewModel
        {
            FirstName = "Sam",
            LastName = "Lee",
            Email = "sam@example.com",
            PhoneNumber = "0700000000"
        };

        _participantValidator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }

    // ----- SignUpViewModelValidator — the new validator we added -----

    private readonly SignUpViewModelValidator _signUpValidator = new();

    private static SignUpViewModel ValidSignUp() => new SignUpViewModel
    {
        FirstName = "Alice",
        LastName = "Smith",
        Email = "alice@example.com",
        PhoneNumber = "0700000001",
        Password = "Secret1!",
        ConfirmPassword = "Secret1!"
    };

    [Fact]
    public void SignUpValidator_WhenAllFieldsAreValid_HasNoErrors()
    {
        _signUpValidator.TestValidate(ValidSignUp()).ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void SignUpValidator_WhenPasswordIsTooShort_HasError()
    {
        // Boundary: 5 characters is one below the 6-character minimum.
        SignUpViewModel model = ValidSignUp();
        model.Password = "Ab1!x";
        model.ConfirmPassword = "Ab1!x";

        _signUpValidator.TestValidate(model).ShouldHaveValidationErrorFor(s => s.Password);
    }

    [Fact]
    public void SignUpValidator_WhenPasswordIsExactlyMinLength_IsValid()
    {
        // Boundary: 6 characters is the minimum and must be accepted.
        SignUpViewModel model = ValidSignUp();
        model.Password = "Ab1!xy";
        model.ConfirmPassword = "Ab1!xy";

        _signUpValidator.TestValidate(model).ShouldNotHaveValidationErrorFor(s => s.Password);
    }

    [Fact]
    public void SignUpValidator_WhenPasswordIsExactlyMaxLength_IsValid()
    {
        // Boundary: 72 characters is the BCrypt limit and must be accepted.
        string s72 = new string('A', 70) + "1!";
        SignUpViewModel model = ValidSignUp();
        model.Password = s72;
        model.ConfirmPassword = s72;

        _signUpValidator.TestValidate(model).ShouldNotHaveValidationErrorFor(s => s.Password);
    }

    [Fact]
    public void SignUpValidator_WhenPasswordExceedsMaxLength_HasError()
    {
        // Boundary: 73 characters exceeds the BCrypt truncation limit and must be rejected.
        string s73 = new string('A', 71) + "1!";
        SignUpViewModel model = ValidSignUp();
        model.Password = s73;
        model.ConfirmPassword = s73;

        _signUpValidator.TestValidate(model).ShouldHaveValidationErrorFor(s => s.Password);
    }

    [Fact]
    public void SignUpValidator_WhenPasswordsDoNotMatch_HasErrorOnConfirmPassword()
    {
        // Cross-property rule: the confirmation must equal the password field.
        SignUpViewModel model = ValidSignUp();
        model.Password = "Secret1!";
        model.ConfirmPassword = "Different!";

        _signUpValidator.TestValidate(model).ShouldHaveValidationErrorFor(s => s.ConfirmPassword);
    }

    [Fact]
    public void SignUpValidator_WhenEmailIsInvalid_HasErrorOnEmail()
    {
        SignUpViewModel model = ValidSignUp();
        model.Email = "not-an-email";

        _signUpValidator.TestValidate(model).ShouldHaveValidationErrorFor(s => s.Email);
    }

    [Fact]
    public void SignUpValidator_WhenFirstNameIsEmpty_HasErrorOnFirstName()
    {
        SignUpViewModel model = ValidSignUp();
        model.FirstName = "";

        _signUpValidator.TestValidate(model).ShouldHaveValidationErrorFor(s => s.FirstName);
    }

    // ----- LoginValidator — additional -----

    [Fact]
    public void LoginValidator_WhenEmailIsEmpty_HasErrorOnEmail()
    {
        LoginViewModel model = new LoginViewModel { Email = "", Password = "Secret1!" };

        _loginValidator.TestValidate(model).ShouldHaveValidationErrorFor(l => l.Email);
    }

    [Fact]
    public void LoginValidator_WhenBothFieldsAreValid_HasNoErrors()
    {
        LoginViewModel model = new LoginViewModel { Email = "admin@events.com", Password = "Admin123!" };

        _loginValidator.TestValidate(model).ShouldNotHaveAnyValidationErrors();
    }
}
