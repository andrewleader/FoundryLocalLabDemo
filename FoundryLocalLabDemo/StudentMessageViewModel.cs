using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoundryLocalLabDemo;

/// <summary>
/// Represents a student message in the support staff inbox
/// </summary>
public class StudentMessageViewModel : INotifyPropertyChanged
{
    private bool _isSelected;
    private bool _isProcessing;

    public string StudentName { get; set; } = "";
    public string StudentId { get; set; } = "";
    public DateTime ReceivedDate { get; set; }
    public string Subject { get; set; } = "";
    public string MessageText { get; set; } = "";
    public bool IsUrgent { get; set; }
    
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
    }

    public bool IsProcessing
    {
        get => _isProcessing;
        set
        {
            if (_isProcessing != value)
            {
                _isProcessing = value;
                OnPropertyChanged();
            }
        }
    }

    public string TimeAgo
    {
        get
        {
            var diff = DateTime.Now - ReceivedDate;
            if (diff.TotalMinutes < 1) return "Just now";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}h ago";
            return $"{(int)diff.TotalDays}d ago";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}