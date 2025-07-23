using Microsoft.Extensions.AI;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private CancellationTokenSource? _currentCancellationTokenSource;

        public ObservableCollection<ChatMessageViewModel> ChatMessages
        {
            get => _chatMessages;
            set
            {
                _chatMessages = value;
                OnPropertyChanged();
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
        }

        private void InitializeChat()
        {
            ChatMessages = new ObservableCollection<ChatMessageViewModel>();
            
            // Add welcome message
            ChatMessages.Add(new ChatMessageViewModel
            {
                Text = "Welcome to the Financial Aid Eligibility Chat! Ask me any questions about financial aid requirements and eligibility.",
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
                    LoadStudentProfile(student);
                    ChatInputTextBox.Text = student.DefaultQuestion;
                    StatusText.Text = $"Loaded profile for {student.Name}";
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

                await ExecutionLogic.StartModelAsync();

                // Generate and stream bot response with cancellation support
                var responseStream = ExecutionLogic.GenerateBotResponseAsync(chatMessages, GetCurrentProfile(), cancellationToken);
                
                await foreach (var update in responseStream.WithCancellation(cancellationToken))
                {
                    // Check for cancellation before processing each update
                    cancellationToken.ThrowIfCancellationRequested();

                    // Handle different types of updates from the streaming response
                    if (update.Text != null)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            botMessage.AppendText(update.Text);
                            ChatScrollViewer.ScrollToBottom();
                        });
                    }
                }
                
                // Mark streaming as complete
                await Dispatcher.InvokeAsync(() =>
                {
                    botMessage.IsStreaming = false;
                    StatusText.Text = "Response complete";
                });
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
                // Reset UI state
                await Dispatcher.InvokeAsync(() =>
                {
                    SendButton.IsEnabled = true;
                    CancelButton.Visibility = Visibility.Collapsed;
                    if (StatusText.Text == "Generating response..." || StatusText.Text == "Cancelling...")
                    {
                        StatusText.Text = "Ready";
                    }
                });

                // Clean up cancellation token source
                _currentCancellationTokenSource?.Dispose();
                _currentCancellationTokenSource = null;
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

ELIGIBILITY REQUIREMENTS:
{requirements}

Please provide helpful, accurate advice about financial aid eligibility based on the student's profile and the requirements listed. Be supportive but honest about any issues that might affect eligibility. Offer specific guidance on next steps when appropriate.";
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