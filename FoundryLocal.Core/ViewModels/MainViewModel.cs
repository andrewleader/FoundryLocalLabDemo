using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoundryLocal.Core.Services;
using Microsoft.AI.Foundry.Local;
using System.Collections.ObjectModel;

namespace FoundryLocal.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // TODO: Could add this as a singleton service via DI
    public ModelManager ModelManager { get; }

    [ObservableProperty]
    public partial ObservableCollection<StudentMessageViewModel> StudentMessages { get; set; } = new();

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
    public partial StudentProfile? CurrentStudentProfile { get; set; }

    [ObservableProperty]
    public partial bool IsProcessingProfile { get; set; }

    public MainViewModel(SynchronizationContext uiContext)
    {
        ModelManager = new(uiContext);
    }

    [RelayCommand]
    public async Task InitializeAsync(string modelId)
    {
        await ModelManager.StartServiceAsync();

        await ModelManager.LoadModelAsync(modelId);
    }
}
