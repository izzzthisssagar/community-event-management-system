namespace CommunityEventManagement.Domain.Exceptions;

/// <summary>
/// EventManagementException is my own base exception class for the whole system. Every custom
/// exception I create inherits from this one. The big advantage is that I can catch this single
/// base type and automatically catch all of my specific exceptions at the same time, while
/// still being able to catch the more specific ones separately when I need to.
/// This is custom exception handling built using inheritance.
/// </summary>
public class EventManagementException : Exception
{
    // Constructor that just takes a message.
    public EventManagementException(string sMessage) : base(sMessage) { }

    // Constructor that also takes an inner exception, so I can wrap a lower level error
    // (like a database error) inside a friendlier domain error without losing the original.
    public EventManagementException(string sMessage, Exception innerException) : base(sMessage, innerException) { }
}
