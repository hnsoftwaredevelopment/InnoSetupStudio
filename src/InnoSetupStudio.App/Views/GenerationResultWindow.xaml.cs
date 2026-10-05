using System.Diagnostics;
using System.Windows;
using InnoSetupStudio.App.Localization;
using InnoSetupStudio.App.ViewModels;
using InnoSetupStudio.Core.Generation;

namespace InnoSetupStudio.App.Views;

/// <summary>
/// Toont het resultaat van "Genereer .iss": waar het script is geschreven, of dat er door fouten geen
/// script is geschreven, plus de meldingen van de generator.
/// </summary>
public partial class GenerationResultWindow : Window
{
    private readonly string? _writtenPath;

    /// <param name="result">Het resultaat van de generator.</param>
    /// <param name="writtenPath">Het pad van het geschreven script, of null als er niets is geschreven.</param>
    public GenerationResultWindow(GenerationResult result, string? writtenPath)
    {
        InitializeComponent();

        _writtenPath = writtenPath;
        HeaderText.Text = writtenPath is null
            ? LocalizationManager.Instance["GenerateResultNotWritten"]
            : string.Format(LocalizationManager.Instance["GenerateResultWrittenFormat"], writtenPath);

        var rows = GenerationIssueFormatter.Rows(result);
        IssuesList.ItemsSource = rows;
        IssuesList.Visibility = rows.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        NoIssuesText.Visibility = rows.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        OpenFolderButton.Visibility = writtenPath is null ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OpenFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (_writtenPath is null)
        {
            return;
        }

        // Verkenner opent met het bestand geselecteerd. Mislukt dat (Verkenner niet te starten), dan
        // is er niets kritieks verloren: het pad staat in de koptekst van dit venster.
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{_writtenPath}\""));
        }
        catch (Exception ex) when (ex is InvalidOperationException or System.ComponentModel.Win32Exception)
        {
            MessageBox.Show(this, ex.Message, "Inno Setup Studio", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}
