using System.Windows;
using InnoSetupStudio.App.ViewModels;

namespace InnoSetupStudio.App.Views;

public partial class WizardScreensWindow : Window
{
    public WizardScreensViewModel ViewModel { get; }

    public WizardScreensWindow(WizardScreensViewModel viewModel)
    {
        InitializeComponent();

        ViewModel = viewModel;
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, bool saved)
    {
        DialogResult = saved;
        Close();
    }
}
