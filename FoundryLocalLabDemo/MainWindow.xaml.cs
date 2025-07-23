using System.Text;
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
    public partial class MainWindow : Window
    {
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
            InitializeDefaults();
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
                SendMessage();
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void SendMessage()
        {
            string message = ChatInputTextBox.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            // Add user message to chat
            AddChatMessage(message, isUser: true);

            // Generate and add bot response
            string response = GenerateBotResponse(message);
            AddChatMessage(response, isUser: false);

            // Clear input and update status
            ChatInputTextBox.Clear();
            StatusText.Text = "Message sent";

            // Auto-scroll to bottom
            ChatScrollViewer.ScrollToBottom();
        }

        private void AddChatMessage(string message, bool isUser)
        {
            var messageBlock = new TextBlock
            {
                Text = message,
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                Background = isUser ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.LightBlue),
                HorizontalAlignment = isUser ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                MaxWidth = 400
            };

            if (!isUser)
            {
                messageBlock.FontWeight = FontWeights.Normal;
            }

            ChatHistoryPanel.Children.Add(messageBlock);
        }

        private string GenerateBotResponse(string userMessage)
        {
            // Get current student profile data
            bool hasSSN = HasSSNCheckBox.IsChecked ?? false;
            bool hasLoanIssues = HasLoanIssuesCheckBox.IsChecked ?? false;
            bool hasLowGrades = HasLowGradesCheckBox.IsChecked ?? false;
            string citizenship = ((ComboBoxItem?)CitizenshipStatusComboBox.SelectedItem)?.Content?.ToString() ?? "Unknown";
            string highSchool = ((ComboBoxItem?)HighSchoolStatusComboBox.SelectedItem)?.Content?.ToString() ?? "Unknown";
            
            double.TryParse(GPATextBox.Text, out double gpa);

            // Simple rule-based response generation
            var issues = new List<string>();
            
            if (citizenship == "Not Eligible")
                issues.Add("citizenship status");
            if (!hasSSN)
                issues.Add("missing valid Social Security Number");
            if (highSchool == "Neither")
                issues.Add("lack of high school diploma or GED");
            if (hasLoanIssues)
                issues.Add("active issues with federal student loans");
            if (hasLowGrades || gpa <= 1.0)
                issues.Add("low academic performance (GPA ≤ 1.0 or failing grades)");

            if (issues.Count == 0)
            {
                return "Great news! Based on your current profile, you appear to meet the basic eligibility requirements for federal financial aid. You have valid citizenship status, proper documentation, educational credentials, and are maintaining satisfactory academic progress. I recommend completing the FAFSA (Free Application for Federal Student Aid) to apply for aid. Is there anything specific about the financial aid process you'd like to know more about?";
            }
            else
            {
                string issueList = string.Join(", ", issues);
                return $"I've identified some potential issues with your financial aid eligibility based on your current profile: {issueList}. These issues may affect your ability to receive federal financial aid. I recommend speaking with your school's financial aid office to discuss your specific situation and explore possible solutions or alternative funding options. Would you like more information about any of these requirements?";
            }
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