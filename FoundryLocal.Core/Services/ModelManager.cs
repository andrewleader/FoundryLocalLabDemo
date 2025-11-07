using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoundryLocal.Core.ViewModels;
using Microsoft.AI.Foundry.Local;
using OwlCore.Extensions;
using System.Collections.ObjectModel;

namespace FoundryLocal.Core.Services;

public partial class ModelManager : ObservableObject
{
    private FoundryLocalManager _foundryLocalManager = new();
    private SynchronizationContext _uiContext;

    // TODO: Encapsulate these better for the ChatService
    public string ApiKey => _foundryLocalManager.ApiKey;
    public Uri Endpoint => _foundryLocalManager.Endpoint;

    public bool IsModelSelected => SelectedModel != null;

    [ObservableProperty]
    public partial ObservableCollection<ModelViewModel> AvailableModels { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<ModelViewModel> DownloadedModels { get; set; } = new();

    [ObservableProperty]
    public partial ObservableCollection<ModelViewModel> AvailableForDownloadModels { get; set; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsModelSelected))]
    public partial ModelViewModel? SelectedModel { get; set; }

    partial void OnSelectedModelChanged(ModelViewModel? oldValue, ModelViewModel? newValue)
    {
        if (oldValue != null)
        {
            oldValue.IsLoading = false;
            oldValue.IsLoaded = false;

            _foundryLocalManager.UnloadModelAsync(oldValue.Name);
        }

        if (newValue != null)
        {
            Task.Run(() => SelectModelAsync(newValue));
        }
    }

    public ModelManager(SynchronizationContext uiContext)
    {
        this._uiContext = uiContext;
    }

    /// <summary>
    /// Called to initialize app with the context of a single model.
    /// </summary>
    /// <param name="modelId"></param>
    /// <returns></returns>
    public async Task<bool> LoadModelAsync(string modelId)
    {
        await LoadAvailableModelsAsync();

        var model = AvailableModels.FirstOrDefault(m => m.Name == modelId);

        if (model is null)
        {
            return false;
        }

        SelectedModel = model;

        return true;
    }

    /// <summary>
    /// Called when the <see cref="SelectedModel"/> property changes to download the model (if not already) and load into memory.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    private async Task SelectModelAsync(ModelViewModel model)
    {
        if (DownloadedModels.Contains(model))
        {
            _uiContext.Post((state) =>
            {
                model.IsDownloaded = true;
                model.DownloadStatusText = "Loading model...";
                model.IsLoaded = false;
                model.IsLoading = true;
            }, null);

            await _foundryLocalManager.LoadModelAsync(model.Name);

            _uiContext.Post((state) =>
            {
                model.IsLoaded = true;
                model.IsLoading = false;
                model.DownloadStatusText = "Model loaded...";
            }, null);            
        }
        else
        {
            try
            {
                _uiContext.Post((state) =>
                {
                    model.IsLoading = true;
                    model.IsDownloaded = false;
                    model.IsDownloading = true;
                    model.DownloadProgress = 0;
                    // TODO: Move to UI with Converter
                    model.DownloadStatusText = "Starting download...";
                }, null);

                await foreach (var progress in _foundryLocalManager.DownloadModelWithProgressAsync(model.Name))
                {
                    _uiContext.Post((state) =>
                    {
                        var progressValue = progress.Percentage;
                        model.DownloadProgress = progressValue;
                        model.DownloadStatusText = $"Downloading... {progressValue:F1}%";
                    }, null);
                }

                // Ensure our collections are updated
                await _uiContext.PostAsync(async () =>
                {
                    model.IsDownloaded = true;
                    model.IsDownloading = false;
                    model.DownloadProgress = 100;
                    model.DownloadStatusText = "Download complete";

                    AvailableForDownloadModels.Remove(model);
                    DownloadedModels.Add(model);
                });

                // Rerun our load logic
                await SelectModelAsync(model);
            }
            catch (Exception e)
            {
                _uiContext.Post((state) =>
                {
                    model.IsDownloaded = false;
                    model.IsDownloading = false;
                    model.DownloadProgress = 0;
                    model.DownloadStatusText = $"Download failed: {e.Message}";
                }, null);
            }
        }
    }

    [RelayCommand]
    public async Task LoadAvailableModelsAsync()
    {
        // Clear existing models
        AvailableModels.Clear();
        DownloadedModels.Clear();
        AvailableForDownloadModels.Clear();

        // Load catalog models
        var catalogModels = await _foundryLocalManager.ListCatalogModelsAsync();
        var cachedModels = await _foundryLocalManager.ListCachedModelsAsync();
        var cachedModelNames = cachedModels.Select(m => m.ModelId).ToHashSet();

        foreach (var model in catalogModels)
        {
            var modelViewModel = new ModelViewModel
            {
                Name = model.ModelId,
                DeviceType = model.Runtime.DeviceType.ToString(),
                IsDownloaded = cachedModelNames.Contains(model.ModelId),
                IsDownloading = false,
                IsLoaded = false, // Models need to be loaded into memory after download
                IsLoading = false,
                DownloadProgress = 0,
                DownloadStatusText = ""
            };

            // Add to main collection
            AvailableModels.Add(modelViewModel);

            // Add to appropriate separated collection
            if (modelViewModel.IsDownloaded)
            {
                DownloadedModels.Add(modelViewModel);
            }
            else
            {
                AvailableForDownloadModels.Add(modelViewModel);
            }
        }
    }

    public async Task StartServiceAsync()
    {
        await _foundryLocalManager.StartServiceAsync();
    }
}
