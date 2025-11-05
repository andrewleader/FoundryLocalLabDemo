using CommunityToolkit.Mvvm.ComponentModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FoundryLocal.Core.ViewModels;

/// <summary>
/// Represents a student message in the support staff inbox
/// </summary>
public partial class StudentMessageViewModel : ObservableObject
{
    public string StudentName { get; set; } = "";
    public string StudentId { get; set; } = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TimeAgo))]
    public partial DateTime ReceivedDate { get; set; }

    public string Subject { get; set; } = "";
    public string MessageText { get; set; } = "";
    public bool IsUrgent { get; set; }

    [ObservableProperty]    
    
    public partial bool IsSelected { get; set; }

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    // TODO: Should be a converter
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
}