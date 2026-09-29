using System.Windows;
using InnoSetupStudio.App.ViewModels;

namespace InnoSetupStudio.App.Views;

public partial class LanguagesWindow : Window
{
    public LanguagesViewModel ViewModel { get; }

    public LanguagesWindow(LanguagesViewModel viewModel)
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
