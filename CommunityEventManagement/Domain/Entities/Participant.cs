namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// The Participant class represents a person who signs up to attend community events.
/// It inherits from BaseEntity so it automatically gets the Id and the audit dates.
/// One participant can have many registrations (one for each event they join), which I
/// model with a private list of Registration objects.
/// </summary>
public class Participant : BaseEntity
{
    // Private backing field for the registrations. I keep it private for encapsulation and
    // use the EF Core underscore convention so it maps automatically.
    private readonly List<Registration> _registrations = new();

    // Private parameterless constructor needed by EF Core for materialising the object.
    private Participant() { }

    /// <summary>
    /// Public constructor used by my application code to create a valid participant.
    /// </summary>
    public Participant(string sFirstName, string sLastName, string sEmail, string sPhoneNumber)
    {
        FirstName = sFirstName;
        LastName = sLastName;
        Email = sEmail;
        PhoneNumber = sPhoneNumber;
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    // The participant's registrations are exposed as read-only so they can only be changed
    // through the proper event registration logic, not directly from outside.
    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();

    // A small helper property that joins the first and last name together. I use this a lot
    // in the user interface so I do not have to combine the two names every single time.
    public string FullName => $"{FirstName} {LastName}";
}
