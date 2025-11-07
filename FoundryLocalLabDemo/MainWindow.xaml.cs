using FoundryLocal.Core;
using FoundryLocal.Core.Services;
using FoundryLocal.Core.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FoundryLocalLabDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml - Financial Aid Support Staff Tool
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel ViewModel { get; } = new();

    private CancellationTokenSource? _currentCancellationTokenSource;

    public MainWindow()
    {
        InitializeComponent();
        InitializeInbox();
        DataContext = ViewModel;
        _ = InitializeModelsAsync();

        // TODO: Better handle text/visuals in XAML compared to code-behind manipulation.
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.ModelManager.SelectedModel))
            {
                UpdateSelectedModelText();

                _ = ProcessSelectedMessage();
            }
            else if (e.PropertyName == nameof(ViewModel.SelectedMessage))
            {
                _ = ProcessSelectedMessage();
            }
        };
    }

    private void InitializeInbox()
    {
        // Pre-populate with 6 sample student messages
        ViewModel.StudentMessages = new(SampleData.GetSampleStudentProfiles());

        StatusText.Text = $"Loaded {ViewModel.StudentMessages.Count} student messages";
    }

    private async Task InitializeModelsAsync()
    {
        try
        {
            StatusText.Text = "Starting AI service...";
            await ExecutionLogic.StartServiceAsync();
            StatusText.Text = "Loading available models...";
            await ViewModel.ModelManager.LoadAvailableModelsAsync();
            StatusText.Text = "Ready - Select a message to process";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error initializing: {ex.Message}";
        }
    }

    private void UpdateSelectedModelText()
    {
        if (ViewModel.ModelManager.SelectedModel == null)
        {
            SelectedModelText.Text = "No model selected";
            SelectedModelText.Foreground = new SolidColorBrush(Colors.Red);
        }
        else if (ViewModel.ModelManager.SelectedModel != null)
        {
            if (ViewModel.ModelManager.SelectedModel.IsLoaded)
            {
                SelectedModelText.Text = $"Selected: {ViewModel.ModelManager.SelectedModel.Name} ({ViewModel.ModelManager.SelectedModel.DeviceType}) - Ready";
                SelectedModelText.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (ViewModel.ModelManager.SelectedModel.IsDownloaded)
            {
                SelectedModelText.Text = $"Selected: {ViewModel.ModelManager.SelectedModel.Name} ({ViewModel.ModelManager.SelectedModel.DeviceType}) - Downloaded";
                SelectedModelText.Foreground = new SolidColorBrush(Colors.Blue);
            }
            else
            {
                SelectedModelText.Text = $"Selected: {ViewModel.ModelManager.SelectedModel.Name} ({ViewModel.ModelManager.SelectedModel.DeviceType}) - Not Downloaded";
                SelectedModelText.Foreground = new SolidColorBrush(Colors.Orange);
            }
        }
    }

    private async void RefreshModelsButton_Click(object sender, RoutedEventArgs e)
    {
        RefreshModelsButton.IsEnabled = false;
        try
        {
            StatusText.Text = "Refreshing models...";
            await LoadAvailableModelsAsync();
            StatusText.Text = "Models refreshed";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error refreshing models: {ex.Message}";
        }
        finally
        {
            RefreshModelsButton.IsEnabled = true;
        }
    }

    private async Task LoadModelIntoMemory(ModelViewModel model)
    {
        if (!model.IsDownloaded || model.IsLoading || model.IsLoaded)
            return;

        try
        {
            model.IsLoading = true;

            if (ViewModel.ModelManager.SelectedModel != null)
            {
                try
                {
                    StatusText.Text = "Unloading previous model from memory...";
                    await ExecutionLogic.UnloadModelAsync(ViewModel.ModelManager.SelectedModel.Name);
                    await Task.Delay(1000);
                }
                catch { }
            }

            StatusText.Text = $"Loading model into memory: {model.Name}...";
            
            await ExecutionLogic.LoadModelAsync(model.Name);
            
            model.IsLoaded = true;
            model.IsLoading = false;
            ViewModel.ModelManager.SelectedModel = model;
            StatusText.Text = $"Model loaded and ready: {model.Name}";
        }
        catch (Exception ex)
        {
            model.IsLoading = false;
            StatusText.Text = $"Error loading model into memory: {ex.Message}";
        }
    }

    private async void ModelItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.Tag is string modelName)
        {
            var model = ViewModel.ModelManager.AvailableModels.FirstOrDefault(m => m.Name == modelName);
            if (model != null)
            {
                // Step 1: Download the model if not downloaded
                if (!model.IsDownloaded && !model.IsDownloading)
                {
                    try
                    {
                        model.IsDownloading = true;
                        model.DownloadProgress = 0;
                        model.DownloadStatusText = "Starting download...";
                        StatusText.Text = $"Downloading model: {model.Name}...";
                        
                        await foreach (var progress in ExecutionLogic.DownloadModelAsync(modelName))
                        {
                            var progressValue = progress.Percentage;
                            model.DownloadProgress = progressValue;
                            model.DownloadStatusText = $"Downloading... {progressValue:F1}%";
                            StatusText.Text = $"Downloading {model.Name}: {progressValue:F1}%";
                        }
                        
                        model.IsDownloaded = true;
                        model.IsDownloading = false;
                        model.DownloadProgress = 100;
                        model.DownloadStatusText = "Download complete";
                        StatusText.Text = $"Model downloaded: {model.Name}";
                        
                        // Refresh the separated collections after download
                        ViewModel.ModelManager.RefreshModelCollections();
                    }
                    catch (Exception ex)
                    {
                        model.IsDownloading = false;
                        model.DownloadProgress = 0;
                        model.DownloadStatusText = "Download failed";
                        StatusText.Text = $"Error downloading model: {ex.Message}";
                        return;
                    }
                }
                // Step 2: Load into memory if downloaded but not loaded
                else if (model.IsDownloaded && !model.IsLoaded && !model.IsLoading)
                {
                    await LoadModelIntoMemory(model);
                }
                // Model is already loaded - just select it
                else if (model.IsLoaded)
                {
                    ViewModel.ModelManager.SelectedModel = model;
                    StatusText.Text = $"Selected model: {model.Name} ({model.DeviceType})";
                }
                // Model is currently downloading or loading - show status
                else
                {
                    if (model.IsDownloading)
                    {
                        StatusText.Text = $"Model is downloading: {model.Name} ({model.DownloadProgress:F1}%)";
                    }
                    else if (model.IsLoading)
                    {
                        StatusText.Text = $"Model is loading into memory: {model.Name}";
                    }
                }
            }
        }
    }

    private void MessageItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is StudentMessageViewModel message)
        {
            ViewModel.SelectedMessage = message;
        }
    }

    // TODO: Move to ViewModel
    private async Task ProcessSelectedMessage()
    {
        if (ViewModel.SelectedMessage == null || ViewModel.ModelManager.SelectedModel?.IsLoaded != true)
            return;

        // Cancel any existing operation
        CancelCurrentOperation();

        // Create new cancellation token source for this operation
        _currentCancellationTokenSource?.Dispose();
        _currentCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _currentCancellationTokenSource.Token;

        try
        {
            ViewModel.SelectedMessage.IsProcessing = true;
            ViewModel.IsProcessingProfile = true;
            StatusText.Text = $"Processing message from {ViewModel.SelectedMessage.StudentName}...";
            TextBlockProcessingMessageDetails.Text = "Extracting information from message...\n\n";

            // Use AI to parse student information from the message
            var studentProfileUpdates = ExecutionLogic.ParseStudentProfileStreamingAsync(
                ViewModel.ModelManager.SelectedModel.Name,
                ViewModel.SelectedMessage.MessageText,
                cancellationToken);

            // Update the current profile and form
            await foreach (var update in studentProfileUpdates)
            {
                if (update.StudentProfile != null)
                {
                    ViewModel.CurrentStudentProfile = update.StudentProfile;
                }
                else
                {
                    TextBlockProcessingMessageDetails.Text += update.Text;
                }
            }

            StatusText.Text = $"Processed message from {ViewModel.SelectedMessage.StudentName} - Profile extracted";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            StatusText.Text = "Processing cancelled";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error processing message: {ex.Message}";
        }
        finally
        {
            if (ViewModel.SelectedMessage != null)
                ViewModel.SelectedMessage.IsProcessing = false;

            ViewModel.IsProcessingProfile = false;
            
            _currentCancellationTokenSource?.Dispose();
            _currentCancellationTokenSource = null;
        }
    }

    private void CancelCurrentOperation()
    {
        if (_currentCancellationTokenSource != null && !_currentCancellationTokenSource.Token.IsCancellationRequested)
        {
            _currentCancellationTokenSource.Cancel();
            StatusText.Text = "Cancelling...";
        }
    }

    // Clean up cancellation token source when window is closed
    protected override void OnClosed(EventArgs e)
    {
        _currentCancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }
}