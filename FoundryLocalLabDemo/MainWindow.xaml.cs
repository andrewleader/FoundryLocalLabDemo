using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FoundryLocalLabDemo;

/// <summary>
/// Interaction logic for MainWindow.xaml - Financial Aid Support Staff Tool
/// </summary>
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private ObservableCollection<StudentMessageViewModel> _studentMessages = new();
    private ObservableCollection<ModelViewModel> _availableModels = new();
    private ObservableCollection<ModelViewModel> _downloadedModels = new();
    private ObservableCollection<ModelViewModel> _availableForDownloadModels = new();
    private CancellationTokenSource? _currentCancellationTokenSource;
    private string? _selectedModelName;
    private StudentMessageViewModel? _selectedMessage;
    private StudentProfile _currentStudentProfile = new();
    private bool _isProcessingProfile;

    public ObservableCollection<StudentMessageViewModel> StudentMessages
    {
        get => _studentMessages;
        set
        {
            _studentMessages = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ModelViewModel> AvailableModels
    {
        get => _availableModels;
        set
        {
            _availableModels = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ModelViewModel> DownloadedModels
    {
        get => _downloadedModels;
        set
        {
            _downloadedModels = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ModelViewModel> AvailableForDownloadModels
    {
        get => _availableForDownloadModels;
        set
        {
            _availableForDownloadModels = value;
            OnPropertyChanged();
        }
    }

    public string? SelectedModelName
    {
        get => _selectedModelName;
        set
        {
            if (_selectedModelName != value)
            {
                _selectedModelName = value;
                OnPropertyChanged();
                UpdateSelectedModelText();
                
                // Auto-process selected message if model is ready and message is selected
                if (!string.IsNullOrEmpty(value) && _selectedMessage != null)
                {
                    var selectedModel = AvailableModels.FirstOrDefault(m => m.Name == value);
                    if (selectedModel?.IsLoaded == true)
                    {
                        _ = ProcessSelectedMessage();
                    }
                }
            }
        }
    }

    public StudentMessageViewModel? SelectedMessage
    {
        get => _selectedMessage;
        set
        {
            if (_selectedMessage != value)
            {
                // Deselect previous message
                if (_selectedMessage != null)
                    _selectedMessage.IsSelected = false;
                
                _selectedMessage = value;
                
                // Select new message
                if (_selectedMessage != null)
                    _selectedMessage.IsSelected = true;
                
                OnPropertyChanged();
                
                // Auto-process if model is ready
                if (_selectedMessage != null && !string.IsNullOrEmpty(SelectedModelName))
                {
                    var selectedModel = AvailableModels.FirstOrDefault(m => m.Name == SelectedModelName);
                    if (selectedModel?.IsLoaded == true)
                    {
                        _ = ProcessSelectedMessage();
                    }
                }
            }
        }
    }

    public StudentProfile CurrentStudentProfile
    {
        get => _currentStudentProfile;
        set
        {
            _currentStudentProfile = value;
            OnPropertyChanged();
            UpdateFormFromProfile();
        }
    }

    public bool IsProcessingProfile
    {
        get => _isProcessingProfile;
        set
        {
            if (_isProcessingProfile != value)
            {
                _isProcessingProfile = value;
                OnPropertyChanged();
            }
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        InitializeInbox();
        DataContext = this;
        _ = InitializeModelsAsync();
    }

    private void InitializeInbox()
    {
        // Pre-populate with 6 sample student messages
        var sampleMessages = new[]
        {
            new StudentMessageViewModel
            {
                StudentName = "Sarah Johnson",
                StudentId = "SJ2024001",
                ReceivedDate = DateTime.Now.AddHours(-2),
                Subject = "Financial Aid Eligibility Question",
                MessageText = "Hi! I'm Sarah, a pre-med student with a 3.8 GPA. I'm a U.S. citizen with SSN 123-45-6789 and I graduated from high school. I'm wondering if I qualify for federal financial aid? I have good grades but I'm worried about the requirements.",
                IsUrgent = false
            },
            new StudentMessageViewModel
            {
                StudentName = "Mike Rodriguez",
                StudentId = "MR2024002", 
                ReceivedDate = DateTime.Now.AddHours(-5),
                Subject = "Previous Loan Issues - Aid Eligibility",
                MessageText = "Hello, I'm Mike Rodriguez. I'm an engineering student but I have some issues with my previous federal loans. My GPA is around 2.1. I'm a permanent resident with SSN 234-56-7890 and I have my GED. Can I still get financial aid?",
                IsUrgent = true
            },
            new StudentMessageViewModel
            {
                StudentName = "Ashley Chen",
                StudentId = "AC2024003",
                ReceivedDate = DateTime.Now.AddHours(-1),
                Subject = "Low Grades Impact on Aid",
                MessageText = "Hi there! I'm Ashley, studying business. My grades haven't been great lately - my GPA is 1.2 and I have some courses with really low grades. I'm a U.S. citizen and high school graduate. How does this affect my financial aid eligibility?",
                IsUrgent = false
            },
            new StudentMessageViewModel
            {
                StudentName = "David Kim",
                StudentId = "DK2024004",
                ReceivedDate = DateTime.Now.AddHours(-8),
                Subject = "International Student Aid Question",
                MessageText = "Hello, my name is David Kim. I'm an international student from South Korea studying computer science. My GPA is 3.5 and I completed high school. I don't have an SSN yet. What financial aid options are available for someone in my situation?",
                IsUrgent = false
            },
            new StudentMessageViewModel
            {
                StudentName = "Maria Gonzalez",
                StudentId = "MG2024005",
                ReceivedDate = DateTime.Now.AddMinutes(-30),
                Subject = "URGENT: Aid Deadline Approaching",
                MessageText = "Hi, this is Maria Gonzalez. I'm a U.S. citizen with SSN 456-78-9012, high school graduate, GPA 3.2. I need to know about financial aid ASAP as deadlines are approaching. I have no previous loan issues. Can you help me understand what I qualify for?",
                IsUrgent = true
            },
            new StudentMessageViewModel
            {
                StudentName = "James Thompson",
                StudentId = "JT2024006",
                ReceivedDate = DateTime.Now.AddDays(-1),
                Subject = "GED and Financial Aid Eligibility",
                MessageText = "Hello, I'm James Thompson. I got my GED instead of graduating traditionally. I'm a U.S. citizen with SSN 567-89-0123. My current GPA in college is 2.8. I want to know if having a GED affects my federal financial aid eligibility.",
                IsUrgent = false
            }
        };

        foreach (var message in sampleMessages)
        {
            StudentMessages.Add(message);
        }

        StatusText.Text = $"Loaded {StudentMessages.Count} student messages";
    }

    private async Task InitializeModelsAsync()
    {
        try
        {
            StatusText.Text = "Starting AI service...";
            await ExecutionLogic.StartServiceAsync();
            StatusText.Text = "Loading available models...";
            await LoadAvailableModelsAsync();
            StatusText.Text = "Ready - Select a message to process";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error initializing: {ex.Message}";
        }
    }

    private async Task LoadAvailableModelsAsync()
    {
        try
        {
            // Clear existing models
            AvailableModels.Clear();
            DownloadedModels.Clear();
            AvailableForDownloadModels.Clear();

            // Load catalog models
            var catalogModels = await ExecutionLogic.ListCatalogModelsAsync();
            var cachedModels = await ExecutionLogic.ListCachedModelsAsync();
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
                    DownloadStatus = ""
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
        catch (Exception ex)
        {
            StatusText.Text = $"Error loading models: {ex.Message}";
        }
    }

    private void UpdateSelectedModelText()
    {
        if (string.IsNullOrEmpty(SelectedModelName))
        {
            SelectedModelText.Text = "No model selected";
            SelectedModelText.Foreground = new SolidColorBrush(Colors.Red);
        }
        else
        {
            var selectedModel = AvailableModels.FirstOrDefault(m => m.Name == SelectedModelName);
            if (selectedModel != null)
            {
                if (selectedModel.IsLoaded)
                {
                    SelectedModelText.Text = $"Selected: {selectedModel.Name} ({selectedModel.DeviceType}) - Ready";
                    SelectedModelText.Foreground = new SolidColorBrush(Colors.Green);
                }
                else if (selectedModel.IsDownloaded)
                {
                    SelectedModelText.Text = $"Selected: {selectedModel.Name} ({selectedModel.DeviceType}) - Downloaded";
                    SelectedModelText.Foreground = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    SelectedModelText.Text = $"Selected: {selectedModel.Name} ({selectedModel.DeviceType}) - Not Downloaded";
                    SelectedModelText.Foreground = new SolidColorBrush(Colors.Orange);
                }
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

            if (SelectedModelName != null)
            {
                try
                {
                    StatusText.Text = "Unloading previous model from memory...";
                    await ExecutionLogic.UnloadModelAsync(SelectedModelName);
                    await Task.Delay(1000);
                }
                catch { }
            }

            StatusText.Text = $"Loading model into memory: {model.Name}...";
            
            await ExecutionLogic.LoadModelAsync(model.Name);
            
            model.IsLoaded = true;
            model.IsLoading = false;
            SelectedModelName = model.Name;
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
            var model = AvailableModels.FirstOrDefault(m => m.Name == modelName);
            if (model != null)
            {
                // Step 1: Download the model if not downloaded
                if (!model.IsDownloaded && !model.IsDownloading)
                {
                    try
                    {
                        model.IsDownloading = true;
                        model.DownloadProgress = 0;
                        model.DownloadStatus = "Starting download...";
                        StatusText.Text = $"Downloading model: {model.Name}...";
                        
                        await foreach (var progress in ExecutionLogic.DownloadModelAsync(modelName))
                        {
                            var progressValue = progress.Percentage;
                            model.DownloadProgress = progressValue;
                            model.DownloadStatus = $"Downloading... {progressValue:F1}%";
                            StatusText.Text = $"Downloading {model.Name}: {progressValue:F1}%";
                        }
                        
                        model.IsDownloaded = true;
                        model.IsDownloading = false;
                        model.DownloadProgress = 100;
                        model.DownloadStatus = "Download complete";
                        StatusText.Text = $"Model downloaded: {model.Name}";
                        
                        // Refresh the separated collections after download
                        RefreshModelCollections();
                    }
                    catch (Exception ex)
                    {
                        model.IsDownloading = false;
                        model.DownloadProgress = 0;
                        model.DownloadStatus = "Download failed";
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
                    SelectedModelName = modelName;
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

    private void RefreshModelCollections()
    {
        // Clear the separated collections
        DownloadedModels.Clear();
        AvailableForDownloadModels.Clear();
        
        // Repopulate based on current state
        foreach (var model in AvailableModels)
        {
            if (model.IsDownloaded)
            {
                DownloadedModels.Add(model);
            }
            else
            {
                AvailableForDownloadModels.Add(model);
            }
        }
    }

    private void MessageItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is StudentMessageViewModel message)
        {
            SelectedMessage = message;
        }
    }

    private async Task ProcessSelectedMessage()
    {
        if (SelectedMessage == null || string.IsNullOrEmpty(SelectedModelName))
            return;

        var selectedModel = AvailableModels.FirstOrDefault(m => m.Name == SelectedModelName);
        if (selectedModel?.IsLoaded != true)
            return;

        // Cancel any existing operation
        CancelCurrentOperation();

        // Create new cancellation token source for this operation
        _currentCancellationTokenSource?.Dispose();
        _currentCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _currentCancellationTokenSource.Token;

        try
        {
            SelectedMessage.IsProcessing = true;
            IsProcessingProfile = true;
            StatusText.Text = $"Processing message from {SelectedMessage.StudentName}...";
            TextBlockProcessingMessageDetails.Text = "Extracting information from message...\n\n";

            // Use AI to parse student information from the message
            var studentProfileUpdates = ExecutionLogic.ParseStudentProfileStreamingAsync(
                SelectedModelName,
                SelectedMessage.MessageText,
                cancellationToken);

            // Update the current profile and form
            await foreach (var update in studentProfileUpdates)
            {
                if (update.StudentProfile != null)
                {
                    CurrentStudentProfile = update.StudentProfile;
                }
                else
                {
                    TextBlockProcessingMessageDetails.Text += update.Text;
                }
            }

            StatusText.Text = $"Processed message from {SelectedMessage.StudentName} - Profile extracted";
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
            if (SelectedMessage != null)
                SelectedMessage.IsProcessing = false;
            
            IsProcessingProfile = false;
            
            _currentCancellationTokenSource?.Dispose();
            _currentCancellationTokenSource = null;
        }
    }

    private void UpdateFormFromProfile()
    {
        // Update First Name
        FirstNameTextBox.Text = CurrentStudentProfile.FirstName ?? "";
        
        // Update Last Name  
        LastNameTextBox.Text = CurrentStudentProfile.LastName ?? "";
        
        // Update SSN
        SSNTextBox.Text = CurrentStudentProfile.SSN ?? "";
        
        // Update GPA
        GPATextBox.Text = CurrentStudentProfile.GPA?.ToString("F1") ?? "";
        
        // Update Citizenship Status
        if (CurrentStudentProfile.CitizenshipStatus.HasValue)
        {
            CitizenshipStatusComboBox.SelectedIndex = (int)CurrentStudentProfile.CitizenshipStatus.Value;
        }
        else
        {
            CitizenshipStatusComboBox.SelectedIndex = -1;
        }
        
        // Update High School Status
        if (CurrentStudentProfile.HighSchoolStatus.HasValue)
        {
            HighSchoolStatusComboBox.SelectedIndex = (int)CurrentStudentProfile.HighSchoolStatus.Value;
        }
        else
        {
            HighSchoolStatusComboBox.SelectedIndex = -1;
        }

        // Update Federal Loan Issues
        if (CurrentStudentProfile.HasFederalLoanIssues == null)
        {
            HasFederalLoanIssuesCheckBox.IsChecked = false; // Unchecked state
        }
        else
        {
            HasFederalLoanIssuesCheckBox.IsChecked = CurrentStudentProfile.HasFederalLoanIssues.Value;
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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Clean up cancellation token source when window is closed
    protected override void OnClosed(EventArgs e)
    {
        _currentCancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }
}