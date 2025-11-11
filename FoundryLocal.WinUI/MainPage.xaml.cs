using FoundryLocal.Core;
using FoundryLocal.Core.ViewModels;
using Microsoft.UI.Xaml.Controls;
using System.Threading;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FoundryLocal.WinUI;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page
{
    /// <summary>
    /// Model to use for this example, run <c>foundry model list</c> to see available models.
    /// </summary>
    ////private static string modelId = "phi-3.5-mini-instruct-qnn-npu:1";
    private static string modelId = "Phi-3.5-mini-instruct-generic-cpu:1";

    public MainViewModel ViewModel { get; } = new(SynchronizationContext.Current!);

    public MainPage()
    {
        this.InitializeComponent();

        // Populate Sample Data
        ViewModel.StudentMessages = new(SampleData.GetSampleStudentProfiles());

        // Select our model to use
        // TODO: Need to show issue if initializing somewhere...
        _ = ViewModel.InitializeAsync(modelId);
    }
}
