namespace CommunityEventManagement.Domain.Entities;

/// <summary>
/// ICancelable is an interface that defines a common "cancel" behaviour.
/// I created this interface so that more than one entity can be cancelled in the same way.
/// In my system both Event and Registration implement this interface, which means I can
/// treat them through the same contract even though they are completely different classes.
/// This is a real demonstration of polymorphism through an interface.
/// </summary>
public interface ICancelable
{
    // IsCancelled tells me whether the object has already been cancelled or not.
    bool IsCancelled { get; }

    // CancellationReason stores the reason text that explains why it was cancelled.
    string? CancellationReason { get; }

    // Cancel is the actual behaviour. Each class that implements this interface
    // will write its own version of how it cancels itself.
    void Cancel(string sReason);
}
