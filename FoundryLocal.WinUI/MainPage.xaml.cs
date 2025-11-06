using FoundryLocal.Core;
using FoundryLocal.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FoundryLocal.WinUI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; } = new();

    public MainPage()
    {
        this.InitializeComponent();

        // Populate Sample Data
        ViewModel.StudentMessages = new(SampleData.GetSampleStudentProfiles());
    }
}
