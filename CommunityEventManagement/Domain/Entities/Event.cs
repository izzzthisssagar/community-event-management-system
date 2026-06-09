using CommunityEventManagement.Domain.Exceptions;

namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// The Event class represents a single community event, for example a charity fun day
/// or a coding workshop day. It is the central entity of my whole system.
/// It inherits from BaseEntity (so it gets the Id, CreatedAt etc.) and it also implements
/// the ICancelable interface (so an event can be cancelled). This shows both inheritance
/// and interface-based polymorphism in one class.
/// </summary>
public class Event : BaseEntity, ICancelable
{
    // These are my private backing fields. I keep the real lists private so that no outside
    // code can directly add, remove or clear items and break my business rules. The only way
    // to change them is through my own methods below (this is encapsulation).
    // EF Core requires the underscore "_camelCase" naming so it can automatically map these
    // backing fields to the navigation properties, so I keep that exact convention here.
    private readonly List<Registration> _registrations = new();
    private readonly List<Venue> _venues = new();
    private readonly List<Activity> _activities = new();

    // Private parameterless constructor. EF Core needs this so it can build the object using
    // reflection when it reads a row from the database. It is private so my own code never
    // creates an empty, invalid event by mistake.
    private Event() { }

    /// <summary>
    /// This is the public constructor that my application code actually uses to create a new
    /// event in the correct way, making sure all the important values are provided up front.
    /// </summary>
    public Event(string sName, DateTime dtDate, TimeSpan tsStartTime, TimeSpan tsEndTime, string sDescription, int iMaxCapacity)
    {
        Name = sName;
        Date = dtDate;
        StartTime = tsStartTime;
        EndTime = tsEndTime;
        Description = sDescription;
        MaxCapacity = iMaxCapacity;
    }

    // All of my public properties stay in PascalCase because EF Core maps each one of them
    // to a column in the database, and PascalCase is the normal C# convention for properties.
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Description { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }

    // These two come from the ICancelable interface. The setters are private so only the
    // Cancel() method inside this class is allowed to change them.
    public bool IsCancelled { get; private set; }
    public string? CancellationReason { get; private set; }

    // I expose my private lists to the outside world as IReadOnlyCollection. This means other
    // code can read and loop through the items, but it can NOT add or remove anything directly.
    public IReadOnlyCollection<Registration> Registrations => _registrations.AsReadOnly();
    public IReadOnlyCollection<Venue> Venues => _venues.AsReadOnly();
    public IReadOnlyCollection<Activity> Activities => _activities.AsReadOnly();

    /// <summary>
    /// Cancels this event and records the reason. This is my implementation of the
    /// ICancelable interface for the Event class.
    /// </summary>
    public void Cancel(string sReason)
    {
        IsCancelled = true;
        CancellationReason = sReason;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Registers a participant for this event and returns the new Registration that was created.
    /// I put the business rules inside the entity itself so the rules can never be skipped, no
    /// matter where the call comes from.
    /// </summary>
    public Registration AddRegistration(Participant participant, string sStatus)
    {
        // Rule 1: the same participant must not be able to register twice for the same event.
        // I only count registrations that are still active (not cancelled ones).
        bool bAlreadyRegistered = _registrations.Any(r => r.ParticipantId == participant.Id && !r.IsCancelled);
        if (bAlreadyRegistered)
        {
            throw new DuplicateRegistrationException(
                $"Participant '{participant.FirstName} {participant.LastName}' is already registered for this event.");
        }

        // Rule 2: I must not accept more registrations than the maximum capacity allows.
        int iActiveRegistrations = _registrations.Count(r => !r.IsCancelled);
        if (MaxCapacity > 0 && iActiveRegistrations >= MaxCapacity)
        {
            throw new VenueCapacityExceededException(
                $"Event '{Name}' has reached its maximum capacity of {MaxCapacity}.");
        }

        // If both rules pass, I create the new registration, add it to my private list and return
        // it so the service layer can save it to the database.
        Registration newRegistration = new Registration(Id, participant.Id, sStatus);
        _registrations.Add(newRegistration);
        return newRegistration;
    }

    /// <summary>
    /// Links a venue to this event. I check first so the same venue can not be added twice.
    /// </summary>
    public void AddVenue(Venue venue)
    {
        bool bAlreadyLinked = _venues.Any(v => v.Id == venue.Id);
        if (!bAlreadyLinked)
        {
            _venues.Add(venue);
        }
    }

    /// <summary>
    /// Removes a venue from this event if it is currently linked to it.
    /// </summary>
    public void RemoveVenue(Venue venue)
    {
        Venue? existingVenue = _venues.FirstOrDefault(v => v.Id == venue.Id);
        if (existingVenue is not null)
        {
            _venues.Remove(existingVenue);
        }
    }

    /// <summary>
    /// Links an activity to this event. Again I check first to avoid duplicates.
    /// </summary>
    public void AddActivity(Activity activity)
    {
        bool bAlreadyLinked = _activities.Any(a => a.Id == activity.Id);
        if (!bAlreadyLinked)
        {
            _activities.Add(activity);
        }
    }

    /// <summary>
    /// Removes an activity from this event if it is currently linked to it.
    /// </summary>
    public void RemoveActivity(Activity activity)
    {
        Activity? existingActivity = _activities.FirstOrDefault(a => a.Id == activity.Id);
        if (existingActivity is not null)
        {
            _activities.Remove(existingActivity);
        }
    }

    /// <summary>
    /// Works out how many seats are still free for this event by taking the maximum
    /// capacity and subtracting the registrations that are still active.
    /// </summary>
    public int GetAvailableSeats()
    {
        int iActiveRegistrations = _registrations.Count(r => !r.IsCancelled);
        return MaxCapacity - iActiveRegistrations;
    }
}
