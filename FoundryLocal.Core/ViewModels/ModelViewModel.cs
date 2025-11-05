using CommunityToolkit.Mvvm.ComponentModel;

namespace FoundryLocal.Core.ViewModels;

/// <summary>
/// Represents an AI model that can be selected
/// </summary>
public partial class ModelViewModel : ObservableObject
{
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

    [ObservableProperty]
    public partial string DownloadStatus { get; set; }
}