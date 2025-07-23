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

    /// <summary>
    /// Represents an AI model that can be selected
    /// </summary>
    public class ModelViewModel : INotifyPropertyChanged
    {
        private string _name = "";
        private string _deviceType = "";
        private bool _isDownloaded;
        private bool _isDownloading;
        private bool _isLoaded;
        private bool _isLoading;
        private double _downloadProgress;
        private string _downloadStatus = "";

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DeviceType
        {
            get => _deviceType;
            set
            {
                if (_deviceType != value)
                {
                    _deviceType = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDownloaded
        {
            get => _isDownloaded;
            set
            {
                if (_isDownloaded != value)
                {
                    _isDownloaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsDownloading
        {
            get => _isDownloading;
            set
            {
                if (_isDownloading != value)
                {
                    _isDownloading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            set
            {
                if (_isLoaded != value)
                {
                    _isLoaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public double DownloadProgress
        {
            get => _downloadProgress;
            set
            {
                if (Math.Abs(_downloadProgress - value) > 0.001)
                {
                    _downloadProgress = value;
                    OnPropertyChanged();
                }
            }
        }

        public string DownloadStatus
        {
            get => _downloadStatus;
            set
            {
                if (_downloadStatus != value)
                {
                    _downloadStatus = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}