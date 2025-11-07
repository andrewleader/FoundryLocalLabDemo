using CommunityToolkit.Mvvm.ComponentModel;

namespace FoundryLocal.Core.ViewModels;

/// <summary>
/// Represents an AI model that can be selected
/// </summary>
public partial class ModelViewModel : ObservableObject
{
    // TODO: Name/DeviceType could be in a model object.
    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string DeviceType { get; set; }

    [ObservableProperty]
    public partial bool IsDownloaded { get; set; }

    [ObservableProperty]
    public partial bool IsDownloading { get; set; }

    [ObservableProperty]
    public partial bool IsLoaded { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; }

    [ObservableProperty]
    public partial double DownloadProgress { get; set; }

    // TODO: Make enum, handle text in UI for formatting from DownloadProgress
    [ObservableProperty]
    public partial string DownloadStatusText { get; set; }
}