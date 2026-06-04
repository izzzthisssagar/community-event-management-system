namespace CommunityEventManagement.Domain.Exceptions;

/// <summary>
/// EventNotFoundException is thrown whenever I try to find an event by its Id but no event with
/// that Id exists in the database. It inherits from my base EventManagementException class.
/// </summary>
public class EventNotFoundException : EventManagementException
{
    public EventNotFoundException(string sMessage) : base(sMessage) { }
    public EventNotFoundException(string sMessage, Exception innerException) : base(sMessage, innerException) { }
}
