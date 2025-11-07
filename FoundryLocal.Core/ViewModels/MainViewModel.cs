using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FoundryLocal.Core.Services;
using System.Collections.ObjectModel;

namespace FoundryLocal.Core.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private CancellationTokenSource? _currentCancellationTokenSource;
    private SynchronizationContext _uiContext;

    // TODO: Could add this as a singleton service via DI
    public ModelManager ModelManager { get; init; }

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

            Task.Run(() => SelectMessageAsync(newValue));
        }
    }

    [ObservableProperty]
    public partial StudentProfile? CurrentStudentProfile { get; set; }

    [ObservableProperty]
    public partial bool IsProcessingProfile { get; set; }

    [ObservableProperty]
    public partial string ProcessingStatusText { get; set; }

    public MainViewModel(SynchronizationContext uiContext)
    {
        ModelManager = new(uiContext);
        
        _uiContext = uiContext;
    }

    [RelayCommand]
    public async Task<bool> InitializeAsync(string modelId)
    {
        await ModelManager.StartServiceAsync();

        return await ModelManager.LoadModelAsync(modelId);
    }

    private async Task SelectMessageAsync(StudentMessageViewModel message)
    {
        CancelCurrentOperation();

        // Create new cancellation token source for this operation
        _currentCancellationTokenSource?.Dispose();
        _currentCancellationTokenSource = new();
        var cancellationToken = _currentCancellationTokenSource.Token;

        try
        {
            _uiContext.Post((state) =>
            {
                message.IsProcessing = true;
                IsProcessingProfile = true;
                ProcessingStatusText = $"Extracting message information from {message.StudentName}...\n\n";
            }, null);
            ////StatusText.Text = $"Processing message from {ViewModel.SelectedMessage.StudentName}...";

            // Use AI to parse student information from the message
            var studentProfileUpdates = ChatService.ParseStudentProfileStreamingAsync(
                ModelManager,
                message.MessageText,
                cancellationToken);

            // Update the current profile and form
            await foreach (var update in studentProfileUpdates)
            {
                _uiContext.Post((state) =>
                {
                    if (update.StudentProfile != null)
                    {
                        CurrentStudentProfile = update.StudentProfile;
                    }
                    else
                    {
                        ProcessingStatusText += update.Text;
                    }
                }, null);
            }

            ////StatusText.Text = $"Processed message from {SelectedMessage.StudentName} - Profile extracted";
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            ////StatusText.Text = "Processing cancelled";
            ProcessingStatusText = "Process cancelled.";
        }
        catch (Exception ex)
        {
            ProcessingStatusText = $"Error processing message: {ex.Message}";
        }
        finally
        {
            _uiContext.Post((state) =>
            {
                if (SelectedMessage != null)
                {
                    SelectedMessage.IsProcessing = false;
                }

                IsProcessingProfile = false;
            }, null);

            _currentCancellationTokenSource?.Dispose();
            _currentCancellationTokenSource = null;
        }
    }

    private void CancelCurrentOperation()
    {
        if (_currentCancellationTokenSource != null && !_currentCancellationTokenSource.Token.IsCancellationRequested)
        {
            _currentCancellationTokenSource.Cancel();
        }
    }
}
