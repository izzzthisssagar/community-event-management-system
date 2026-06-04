namespace CommunityEventManagement.Domain.Exceptions;

/// <summary>
/// VenueCapacityExceededException is thrown when someone tries to register for an event that is
/// already full (the number of active registrations has reached the maximum capacity). It
/// inherits from my base EventManagementException class. This is another key edge case from the
/// assignment brief.
/// </summary>
public class VenueCapacityExceededException : EventManagementException
{
    public VenueCapacityExceededException(string sMessage) : base(sMessage) { }
    public VenueCapacityExceededException(string sMessage, Exception innerException) : base(sMessage, innerException) { }
}
