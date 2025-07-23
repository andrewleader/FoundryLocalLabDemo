using Microsoft.Extensions.AI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FoundryLocalLabDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _currentStudentName = "";
        private ObservableCollection<ChatMessageViewModel> _chatMessages = new();
        private ObservableCollection<ModelViewModel> _availableModels = new();
        private ObservableCollection<ModelViewModel> _downloadedModels = new();
        private ObservableCollection<ModelViewModel> _availableForDownloadModels = new();
        private CancellationTokenSource? _currentCancellationTokenSource;
        private string? _selectedModelName;

        public ObservableCollection<ChatMessageViewModel> ChatMessages
        {
            get => _chatMessages;
            set
            {
                _chatMessages = value;
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
                    string? previousModel = _selectedModelName;
                    _selectedModelName = value;
                    OnPropertyChanged();
                    UpdateSendButtonState();
                    UpdateSelectedModelText();
                    
                    // Reset chat when model changes (but not on initial load)
                    if (!string.IsNullOrEmpty(previousModel) && !string.IsNullOrEmpty(value))
                    {
                        ResetChat();
                    }
                }
            }
        }

        // Sample student data
        private readonly Dictionary<string, StudentProfile> _students = new()
        {
            ["sarah"] = new StudentProfile
            {
                Name = "Sarah Johnson",
                CitizenshipStatus = "U.S. Citizen",
                HasSSN = true,
                HighSchoolStatus = "High School Graduate",
                HasLoanIssues = false,
                GPA = 3.8,
                HasLowGrades = false,
                DefaultQuestion = "Hi! I'm Sarah, a pre-med student. I'm wondering if I qualify for federal financial aid? I have good grades but I'm worried about the requirements."
            },
            ["mike"] = new StudentProfile
            {
                Name = "Mike Rodriguez",
                CitizenshipStatus = "Eligible Noncitizen",
                HasSSN = true,
                HighSchoolStatus = "GED Holder",
                HasLoanIssues = true,
                GPA = 2.1,
                HasLowGrades = false,
                DefaultQuestion = "Hello, I'm Mike. I'm an engineering student but I have some issues with my previous federal loans. Can I still get financial aid?"
            },
            ["ashley"] = new StudentProfile
            {
                Name = "Ashley Chen",
                CitizenshipStatus = "U.S. Citizen",
                HasSSN = true,
                HighSchoolStatus = "High School Graduate",
                HasLoanIssues = false,
                GPA = 1.2,
                HasLowGrades = true,
                DefaultQuestion = "Hi there! I'm Ashley, studying business. My grades haven't been great lately - I have some courses with really low grades. How does this affect my financial aid eligibility?"
            }
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeChat();
            InitializeDefaults();
            DataContext = this;
            _ = InitializeModelsAsync();
        }

        private async Task InitializeModelsAsync()
        {
            try
            {
                StatusText.Text = "Starting AI service...";
                await ExecutionLogic.StartServiceAsync();
                StatusText.Text = "Loading available models...";
                await LoadAvailableModelsAsync();
                StatusText.Text = "Ready";
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

        private void UpdateSendButtonState()
        {
            // Enable send button only if a model is selected, loaded, and we're not currently streaming
            var selectedModel = AvailableModels.FirstOrDefault(m => m.Name == SelectedModelName);
            
            // Check if we have a valid model that's loaded
            bool hasValidModel = !string.IsNullOrEmpty(SelectedModelName) && selectedModel?.IsLoaded == true;
            
            // Check if we're currently in an active operation (not cancelled and not null)
            bool isActiveOperation = _currentCancellationTokenSource != null && 
                                    !_currentCancellationTokenSource.Token.IsCancellationRequested;
            
            SendButton.IsEnabled = hasValidModel && !isActiveOperation;
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

        private void InitializeChat()
        {
            if (ChatMessages == null)
            {
                ChatMessages = new ObservableCollection<ChatMessageViewModel>();
            }
            else
            {
                ChatMessages.Clear();
            }

            string text = "Welcome to the Financial Aid Eligibility Chat!";

            if (SelectedModelName == null)
            {
                text += " Please select an AI model to get started, then a";
            }
            else
            {
                text += " A";
            }

            text += "sk me any questions about financial aid requirements and eligibility.";

            // Add welcome message
            ChatMessages.Add(new ChatMessageViewModel
            {
                Text = text,
                IsUser = false,
                IsStreaming = false
            });
        }

        private void InitializeDefaults()
        {
            // Set default values
            CitizenshipStatusComboBox.SelectedIndex = 0;
            HasSSNCheckBox.IsChecked = true;
            HighSchoolStatusComboBox.SelectedIndex = 0;
        }

        private void StudentSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StudentSelector.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag != null)
            {
                string studentKey = selectedItem.Tag.ToString()!;
                if (_students.TryGetValue(studentKey, out StudentProfile? student))
                {
                    string previousStudentName = _currentStudentName;
                    LoadStudentProfile(student);
                    ChatInputTextBox.Text = student.DefaultQuestion;
                    StatusText.Text = $"Loaded profile for {student.Name}";
                    
                    // Reset chat when student changes (but not on initial load)
                    if (!string.IsNullOrEmpty(previousStudentName) && previousStudentName != student.Name)
                    {
                        ResetChat();
                        // Set the default question after reset
                        ChatInputTextBox.Text = student.DefaultQuestion;
                    }
                }
            }
        }

        private void LoadStudentProfile(StudentProfile student)
        {
            _currentStudentName = student.Name;

            // Set citizenship status
            for (int i = 0; i < CitizenshipStatusComboBox.Items.Count; i++)
            {
                if (((ComboBoxItem)CitizenshipStatusComboBox.Items[i]).Content.ToString() == student.CitizenshipStatus)
                {
                    CitizenshipStatusComboBox.SelectedIndex = i;
                    break;
                }
            }

            HasSSNCheckBox.IsChecked = student.HasSSN;

            // Set high school status
            for (int i = 0; i < HighSchoolStatusComboBox.Items.Count; i++)
            {
                if (((ComboBoxItem)HighSchoolStatusComboBox.Items[i]).Content.ToString() == student.HighSchoolStatus)
                {
                    HighSchoolStatusComboBox.SelectedIndex = i;
                    break;
                }
            }

            HasLoanIssuesCheckBox.IsChecked = student.HasLoanIssues;
            GPATextBox.Text = student.GPA.ToString("F1");
            HasLowGradesCheckBox.IsChecked = student.HasLowGrades;
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
                            
                            // Step 2: Automatically load into memory after download
                            await LoadModelIntoMemory(model);
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

        private void ChatInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                _ = SendMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            _ = SendMessage();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelCurrentOperation();
        }

        private void CancelCurrentOperation()
        {
            if (_currentCancellationTokenSource != null && !_currentCancellationTokenSource.Token.IsCancellationRequested)
            {
                _currentCancellationTokenSource.Cancel();
                StatusText.Text = "Cancelling...";
            }
        }

        private async Task SendMessage()
        {
            string message = ChatInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            // Check if a model is selected and loaded
            if (string.IsNullOrEmpty(SelectedModelName))
            {
                StatusText.Text = "Please select an AI model first";
                return;
            }

            var selectedModel = AvailableModels.FirstOrDefault(m => m.Name == SelectedModelName);
            if (selectedModel == null || !selectedModel.IsLoaded)
            {
                StatusText.Text = "Please wait for the selected model to be loaded into memory";
                return;
            }

            // Cancel any existing operation
            CancelCurrentOperation();

            // Create new cancellation token source for this operation
            _currentCancellationTokenSource?.Dispose();
            _currentCancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _currentCancellationTokenSource.Token;

            // Update UI state for streaming
            SendButton.IsEnabled = false;
            CancelButton.Visibility = Visibility.Visible;
            
            // Add user message to chat
            var userMessage = new ChatMessageViewModel
            {
                Text = message,
                IsUser = true,
                IsStreaming = false
            };
            ChatMessages.Add(userMessage);

            // Clear input and update status
            ChatInputTextBox.Clear();
            StatusText.Text = "Generating response...";

            // Create bot response message
            var botMessage = new ChatMessageViewModel
            {
                Text = "",
                IsUser = false,
                IsStreaming = true
            };
            ChatMessages.Add(botMessage);

            // Auto-scroll to bottom
            await Dispatcher.InvokeAsync(() => ChatScrollViewer.ScrollToBottom());

            try
            {
                // Convert to AI chat messages
                var chatMessages = ConvertToChatMessages();

                // Generate and stream bot response with cancellation support
                var responseStream = ExecutionLogic.GenerateBotResponseAsync(SelectedModelName, chatMessages, GetCurrentProfile(), cancellationToken);

                await foreach (var update in responseStream.WithCancellation(cancellationToken))
                {
                    // Check for cancellation before processing each update
                    cancellationToken.ThrowIfCancellationRequested();

                    // Handle different types of updates from the streaming response
                    if (update.Text != null)
                    {
                        botMessage.AppendText(update.Text);
                        ChatScrollViewer.ScrollToBottom();
                    }
                }

                // Mark streaming as complete
                botMessage.IsStreaming = false;
                StatusText.Text = "Response complete";
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Handle cancellation gracefully
                await Dispatcher.InvokeAsync(() =>
                {
                    if (string.IsNullOrEmpty(botMessage.Text))
                    {
                        // If no text was generated, remove the empty message
                        ChatMessages.Remove(botMessage);
                    }
                    else
                    {
                        // If partial text was generated, mark it as cancelled
                        botMessage.Text += "\n\n[Response cancelled by user]";
                        botMessage.IsStreaming = false;
                    }
                    StatusText.Text = "Response cancelled";
                });
            }
            catch (Exception ex)
            {
                // Handle any other errors during response generation
                await Dispatcher.InvokeAsync(() =>
                {
                    botMessage.Text = $"Error: {ex.Message}";
                    botMessage.IsStreaming = false;
                    StatusText.Text = $"Error: {ex.Message}";
                });
            }
            finally
            {
                // Clean up cancellation token source first
                _currentCancellationTokenSource?.Dispose();
                _currentCancellationTokenSource = null;
                
                // Then reset UI state
                await Dispatcher.InvokeAsync(() =>
                {
                    UpdateSendButtonState(); // Use the centralized method
                    CancelButton.Visibility = Visibility.Collapsed;
                    if (StatusText.Text == "Generating response..." || StatusText.Text == "Cancelling...")
                    {
                        StatusText.Text = "Ready";
                    }
                });
            }
        }

        private List<ChatMessage> ConvertToChatMessages()
        {
            var messages = new List<ChatMessage>();
            
            // Add system message with eligibility requirements and current student profile
            var systemPrompt = CreateSystemPrompt();
            messages.Add(new ChatMessage(ChatRole.System, systemPrompt));
            
            // Add user messages from chat history (excluding welcome message and the last bot message if it's being generated)
            foreach (var chatMsg in ChatMessages.Where(m => m != ChatMessages.LastOrDefault() || !m.IsStreaming))
            {
                if (chatMsg.IsUser)
                {
                    messages.Add(new ChatMessage(ChatRole.User, chatMsg.Text));
                }
                else if (chatMsg != ChatMessages.FirstOrDefault()) // Skip welcome message
                {
                    messages.Add(new ChatMessage(ChatRole.Assistant, chatMsg.Text));
                }
            }
            
            return messages;
        }

        private string CreateSystemPrompt()
        {
            var profile = GetCurrentProfile();
            var requirements = EligibilityRequirementsTextBox.Text;
            
            return $@"You are a financial aid advisor helping students understand their eligibility for federal financial aid.

CURRENT STUDENT PROFILE:
- Name: {profile.Name}
- Citizenship Status: {profile.CitizenshipStatus}
- Has Valid SSN: {profile.HasSSN}
- High School Status: {profile.HighSchoolStatus}
- Federal Loan Issues: {profile.HasLoanIssues}
- Current GPA: {profile.GPA:F1}
- Has Grades ≤ 1.0: {profile.HasLowGrades}

ELIGIBILITY REQUIREMENTS:
{requirements}

Please provide helpful, accurate advice about financial aid eligibility based on the student's profile and the requirements listed. ONLY use the eligibility requirements listed here as criteria for financial aid. Be supportive but honest about any issues that might affect eligibility. Offer specific guidance on next steps when appropriate. Keep your response very BRIEF and SHORT and to the point. Only respond with what's necessary to answer the question in the quickest way possible.";
        }

        private StudentProfile GetCurrentProfile()
        {
            // Get current student profile data
            bool hasSSN = HasSSNCheckBox.IsChecked ?? false;
            bool hasLoanIssues = HasLoanIssuesCheckBox.IsChecked ?? false;
            bool hasLowGrades = HasLowGradesCheckBox.IsChecked ?? false;
            string citizenship = ((ComboBoxItem?)CitizenshipStatusComboBox.SelectedItem)?.Content?.ToString() ?? "Unknown";
            string highSchool = ((ComboBoxItem?)HighSchoolStatusComboBox.SelectedItem)?.Content?.ToString() ?? "Unknown";

            return new StudentProfile
            {
                HasSSN = hasSSN,
                HasLoanIssues = hasLoanIssues,
                HasLowGrades = hasLowGrades,
                CitizenshipStatus = citizenship,
                HighSchoolStatus = highSchool,
                GPA = double.TryParse(GPATextBox.Text, out double gpa) ? gpa : 0.0,
                Name = _currentStudentName
            };
        }

        private void ResetChat()
        {
            // Cancel any ongoing operation first
            CancelCurrentOperation();

            InitializeChat();
            
            // Clear input box
            ChatInputTextBox.Clear();
            
            // Update status
            StatusText.Text = "Chat reset - Ready for new conversation";
        }

        private void ResetChatButton_Click(object sender, RoutedEventArgs e)
        {
            ResetChat();
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

    // Helper class for student profiles
    public class StudentProfile
    {
        public string Name { get; set; } = "";
        public string CitizenshipStatus { get; set; } = "";
        public bool HasSSN { get; set; }
        public string HighSchoolStatus { get; set; } = "";
        public bool HasLoanIssues { get; set; }
        public double GPA { get; set; }
        public bool HasLowGrades { get; set; }
        public string DefaultQuestion { get; set; } = "";
    }
}