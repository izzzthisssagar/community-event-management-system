namespace CommunityEventManagement.Domain.Exceptions;

/// <summary>
/// DuplicateRegistrationException is thrown when a participant tries to register for an event
/// that they are already actively registered for. It inherits from my base
/// EventManagementException class. This is one of the important edge cases the assignment asks
/// me to handle gracefully.
/// </summary>
public class DuplicateRegistrationException : EventManagementException
{
    public DuplicateRegistrationException(string sMessage) : base(sMessage) { }
    public DuplicateRegistrationException(string sMessage, Exception innerException) : base(sMessage, innerException) { }
}
