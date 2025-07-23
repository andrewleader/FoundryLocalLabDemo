using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoundryLocalLabDemo
{
    /// <summary>
    /// Represents a chat message that supports property change notifications
    /// </summary>
    public class ChatMessageViewModel : INotifyPropertyChanged
    {
        private string _text = "";
        private bool _isUser;
        private bool _isStreaming;

        public string Text
        {
            get => _text;
            set
            {
                if (_text != value)
                {
                    _text = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsUser
        {
            get => _isUser;
            set
            {
                if (_isUser != value)
                {
                    _isUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsStreaming
        {
            get => _isStreaming;
            set
            {
                if (_isStreaming != value)
                {
                    _isStreaming = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Appends text to the existing message content (used for streaming)
        /// </summary>
        /// <param name="additionalText">Text to append</param>
        public void AppendText(string additionalText)
        {
            Text += additionalText;
        }
    }
}