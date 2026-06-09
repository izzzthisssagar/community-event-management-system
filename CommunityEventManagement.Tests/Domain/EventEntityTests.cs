using CommunityEventManagement.Domain.Entities;
using CommunityEventManagement.Domain.Exceptions;

namespace CommunityEventManagement.Tests.Domain;

/// <summary>
/// These tests check the business rules that live INSIDE the Event entity itself. Testing the
/// domain logic directly (with no database and no mocks) is the most focused way to prove the
/// rules work, including the important edge cases and boundary conditions around capacity and
/// duplicate registrations.
/// </summary>
public class EventEntityTests
{
    private static Event MakeEvent(int iCapacity = 10) =>
        new Event("Test Event", DateTime.Today.AddDays(1), new TimeSpan(9, 0, 0), new TimeSpan(11, 0, 0), "desc", iCapacity);

    private static Participant MakeParticipant(string sEmail = "a@b.com") =>
        new Participant("Test", "Person", sEmail, "0700000000");

    [Fact]
    public void AddRegistration_ReturnsRegistration_AndAddsItToTheCollection()
    {
        Event sut = MakeEvent();
        Participant participant = MakeParticipant();

        Registration result = sut.AddRegistration(participant, "Confirmed");

        Assert.NotNull(result);
        Assert.Equal(participant.Id, result.ParticipantId);
        Assert.Single(sut.Registrations);
    }

    [Fact]
    public void AddRegistration_SameParticipantTwice_ThrowsDuplicateRegistrationException()
    {
        Event sut = MakeEvent();
        Participant participant = MakeParticipant();
        sut.AddRegistration(participant, "Confirmed");

        Assert.Throws<DuplicateRegistrationException>(() => sut.AddRegistration(participant, "Confirmed"));
    }

    [Fact]
    public void AddRegistration_WhenCapacityIsExactlyReached_ThrowsVenueCapacityExceededException()
    {
        // Boundary condition: capacity of 1, one active registration, then one more must fail.
        Event sut = MakeEvent(iCapacity: 1);
        sut.AddRegistration(MakeParticipant("first@b.com"), "Confirmed");

        Assert.Throws<VenueCapacityExceededException>(() => sut.AddRegistration(MakeParticipant("second@b.com"), "Confirmed"));
    }

    [Fact]
    public void AddRegistration_AfterTheFirstOneIsCancelled_IsAllowedAgain()
    {
        // Edge case: a cancelled registration must not block the same participant from re-registering.
        Event sut = MakeEvent();
        Participant participant = MakeParticipant();
        Registration first = sut.AddRegistration(participant, "Confirmed");
        first.Cancel("Changed my mind");

        Registration second = sut.AddRegistration(participant, "Confirmed");

        Assert.Equal(2, sut.Registrations.Count);
        Assert.Single(sut.Registrations, r => !r.IsCancelled);
        Assert.NotEqual(first.Id, second.Id);
    }

    [Fact]
    public void GetAvailableSeats_CountsOnlyActiveRegistrations()
    {
        Event sut = MakeEvent(iCapacity: 5);
        sut.AddRegistration(MakeParticipant("one@b.com"), "Confirmed");
        Registration second = sut.AddRegistration(MakeParticipant("two@b.com"), "Confirmed");
        second.Cancel("gone");

        // 5 capacity, 2 registrations but 1 cancelled => 4 seats left.
        Assert.Equal(4, sut.GetAvailableSeats());
    }

    [Fact]
    public void Cancel_SetsIsCancelledAndStoresTheReason()
    {
        Event sut = MakeEvent();

        sut.Cancel("Venue flooded");

        Assert.True(sut.IsCancelled);
        Assert.Equal("Venue flooded", sut.CancellationReason);
    }

    [Fact]
    public void AddVenue_DoesNotAddTheSameVenueTwice()
    {
        Event sut = MakeEvent();
        Venue venue = new Venue("Hall", "Street", 100, true);

        sut.AddVenue(venue);
        sut.AddVenue(venue);

        Assert.Single(sut.Venues);
    }

    [Fact]
    public void RemoveActivity_RemovesAPreviouslyAddedActivity()
    {
        Event sut = MakeEvent();
        WorkshopActivity activity = new WorkshopActivity("Art", 60, "Tutor", "Paint");
        sut.AddActivity(activity);

        sut.RemoveActivity(activity);

        Assert.Empty(sut.Activities);
    }

    // ----- GetAvailableSeats edge cases -----

    [Fact]
    public void GetAvailableSeats_WhenNoRegistrationsExist_ReturnsFullCapacity()
    {
        // Boundary: fresh event with no registrations — all seats available.
        Event sut = MakeEvent(iCapacity: 10);

        Assert.Equal(10, sut.GetAvailableSeats());
    }

    [Fact]
    public void GetAvailableSeats_WhenAllRegistrationsAreCancelled_ReturnsFullCapacity()
    {
        // Edge case: cancelled registrations must not count against available seats.
        Event sut = MakeEvent(iCapacity: 3);
        Registration r1 = sut.AddRegistration(MakeParticipant("a@b.com"), "Confirmed");
        Registration r2 = sut.AddRegistration(MakeParticipant("c@d.com"), "Confirmed");
        r1.Cancel("withdrawn");
        r2.Cancel("withdrawn");

        Assert.Equal(3, sut.GetAvailableSeats());
    }

    [Fact]
    public void AddRegistration_CancelledRegistrationDoesNotBlockCapacity()
    {
        // Edge case: a cancelled registration must free the seat for someone new.
        Event sut = MakeEvent(iCapacity: 1);
        Registration first = sut.AddRegistration(MakeParticipant("a@b.com"), "Confirmed");
        first.Cancel("leaving");

        // This should succeed — the one seat is free again.
        Registration second = sut.AddRegistration(MakeParticipant("b@c.com"), "Confirmed");

        Assert.NotNull(second);
        Assert.Equal(1, sut.Registrations.Count(r => !r.IsCancelled));
    }

    // ----- Activity deduplication (parallel to venue deduplication) -----

    [Fact]
    public void AddActivity_DoesNotAddTheSameActivityTwice()
    {
        Event sut = MakeEvent();
        WorkshopActivity activity = new WorkshopActivity("Painting", 60, "Bob", "Paint");

        sut.AddActivity(activity);
        sut.AddActivity(activity);

        Assert.Single(sut.Activities);
    }

    // ----- Venue removal -----

    [Fact]
    public void RemoveVenue_RemovesAPreviouslyAddedVenue()
    {
        Event sut = MakeEvent();
        Venue venue = new Venue("Hall", "Road", 200, true);
        sut.AddVenue(venue);

        sut.RemoveVenue(venue);

        Assert.Empty(sut.Venues);
    }

    // ----- Cancel idempotency -----

    [Fact]
    public void Cancel_WhenCalledTwice_CancellationReasonIsFromFirstCall()
    {
        // Cancelling an already-cancelled event should not overwrite the original reason.
        Event sut = MakeEvent();
        sut.Cancel("First reason");
        sut.Cancel("Second reason");

        // The event is cancelled and the first reason is preserved.
        Assert.True(sut.IsCancelled);
        Assert.Equal("First reason", sut.CancellationReason);
    }

    // ----- UpdatedAt is set on Cancel -----

    [Fact]
    public void Cancel_SetsUpdatedAtToApproximatelyNow()
    {
        Event sut = MakeEvent();
        DateTime dtBefore = DateTime.UtcNow.AddSeconds(-1);

        sut.Cancel("rain");

        Assert.True(sut.UpdatedAt >= dtBefore);
    }
}
