using CommunityToolkit.Mvvm.ComponentModel;

namespace FoundryLocal.Core.ViewModels;

/// <summary>
/// Represents a chat message that supports property change notifications
/// </summary>
public partial class ChatMessageViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Text { get; set; }

    [ObservableProperty]
    public partial bool IsUser { get; set; }

    [ObservableProperty]
    public partial bool IsStreaming { get; set; }


    /// <summary>
    /// Appends text to the existing message content (used for streaming)
    /// </summary>
    /// <param name="additionalText">Text to append</param>
    public void AppendText(string additionalText)
    {
        Text += additionalText;
    }
}
