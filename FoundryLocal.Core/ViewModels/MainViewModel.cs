using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace FoundryLocal.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    public bool IsModelSelected => SelectedModel != null;

    [ObservableProperty]
    public partial ObservableCollection<StudentMessageViewModel> StudentMessages { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<ModelViewModel> AvailableModels { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<ModelViewModel> DownloadedModels { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<ModelViewModel> AvailableForDownloadModels { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsModelSelected))]
    public partial ModelViewModel? SelectedModel { get; set; }

    [ObservableProperty]
    public partial StudentMessageViewModel? SelectedMessage { get; set; }

    partial void OnSelectedMessageChanged(StudentMessageViewModel? oldValue, StudentMessageViewModel? newValue)
    {
        // Deselect previous message
        if (oldValue != null)
        {
            oldValue.IsSelected = false;
        }

        // Select new message
        if (newValue != null)
        {
            newValue.IsSelected = true;
        }

        // TODO: Should trigger processing here instead within the VM
        // Probably as an interface to an injected service
    }

    [ObservableProperty]
    public partial StudentProfile CurrentStudentProfile { get; set; }

    [ObservableProperty]
    public partial bool IsProcessingProfile { get; set; }
}
