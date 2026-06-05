namespace CommunityEventManagement.Application.Services;

/// <summary>
/// The kind of toast, which decides its colour and icon.
/// </summary>
public enum ToastLevel
{
    Success,
    Error,
    Info
}

/// <summary>
/// A single toast message (a small pop-up notification).
/// </summary>
public class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Text { get; }
    public ToastLevel Level { get; }

    public ToastMessage(string sText, ToastLevel level)
    {
        Text = sText;
        Level = level;
    }
}

/// <summary>
/// IToastService is the contract for showing toast notifications anywhere in the app.
/// </summary>
public interface IToastService
{
    // Fired whenever the list of toasts changes, so the Toaster component knows to re-render.
    event Action? OnChange;

    // The toasts that are currently showing.
    IReadOnlyList<ToastMessage> Messages { get; }

    void ShowSuccess(string sText);
    void ShowError(string sText);
    void ShowInfo(string sText);
    void Remove(Guid guidId);
}

/// <summary>
/// ToastService lets any page show a small pop-up notification (for example "Event saved"). It is
/// a nice example of the Observer pattern: pages call ShowSuccess/ShowError, and the Toaster
/// component listens to the OnChange event and updates the screen. It is registered as Scoped, so
/// there is one shared instance per user connection (circuit), which both the pages and the
/// Toaster use.
/// </summary>
public class ToastService : IToastService
{
    private readonly List<ToastMessage> _messages = new();

    public event Action? OnChange;

    public IReadOnlyList<ToastMessage> Messages => _messages.AsReadOnly();

    public void ShowSuccess(string sText) => Add(new ToastMessage(sText, ToastLevel.Success));

    public void ShowError(string sText) => Add(new ToastMessage(sText, ToastLevel.Error));

    public void ShowInfo(string sText) => Add(new ToastMessage(sText, ToastLevel.Info));

    public void Remove(Guid guidId)
    {
        _messages.RemoveAll(m => m.Id == guidId);
        OnChange?.Invoke();
    }

    private void Add(ToastMessage message)
    {
        _messages.Add(message);
        OnChange?.Invoke();
    }
}
