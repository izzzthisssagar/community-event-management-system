namespace CommunityEventManagement.Domain.Exceptions;

/// <summary>
/// EntityNotFoundException is a more general "not found" exception. I throw it when any entity
/// (for example a Participant or a Venue) cannot be found by its Id. It inherits from my base
/// EventManagementException class.
/// </summary>
public class EntityNotFoundException : EventManagementException
{
    public EntityNotFoundException(string sMessage) : base(sMessage) { }
    public EntityNotFoundException(string sMessage, Exception innerException) : base(sMessage, innerException) { }
}
