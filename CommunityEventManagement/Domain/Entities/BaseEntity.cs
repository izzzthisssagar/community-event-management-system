namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// BaseEntity is the abstract parent class for every entity in my system.
/// I created this class so that all of my entities (Event, Participant, Venue, Activity,
/// Registration and User) can inherit these common properties from one single place
/// instead of me repeating the same code again and again in every class.
/// This is a clear demonstration of inheritance, which is one of the main OOP principles.
/// </summary>
public abstract class BaseEntity
{
    // Id is the primary key for every entity. I am using a Guid (globally unique identifier)
    // instead of a normal int because a Guid is always unique and is generated automatically
    // the moment the object is created, so I never have to worry about duplicate keys.
    public Guid Id { get; protected set; } = Guid.NewGuid();

    // CreatedAt stores the exact date and time when the record was first created.
    // The setter is protected on purpose so that outside code can never change the
    // creation date once the object exists.
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    // UpdatedAt stores the date and time when the record was last modified.
    // My services update this value every time they save a change.
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ConcurrencyToken is used by Entity Framework Core for optimistic concurrency control.
    // It basically stops two different users from overwriting each other's changes at the
    // same time, because EF checks this value before it saves anything to the database.
    public byte[]? ConcurrencyToken { get; set; }
}
