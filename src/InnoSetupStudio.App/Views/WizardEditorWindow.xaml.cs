using System.Windows;
using InnoSetupStudio.App.ViewModels;

namespace InnoSetupStudio.App.Views;

public partial class WizardEditorWindow : Window
{
    public WizardEditorViewModel ViewModel { get; }

    public WizardEditorWindow(WizardEditorViewModel viewModel)
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
