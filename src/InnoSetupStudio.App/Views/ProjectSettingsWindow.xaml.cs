using System.Windows;
using InnoSetupStudio.App.ViewModels;

namespace InnoSetupStudio.App.Views;

public partial class ProjectSettingsWindow : Window
{
    public ProjectSettingsViewModel ViewModel { get; }

    public ProjectSettingsWindow(ProjectSettingsViewModel viewModel)
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
