using InnoSetupStudio.Core.Generation;

namespace InnoSetupStudio.App.ViewModels;

/// <summary>Eén regel in het resultaatvenster van "Genereer .iss": de ernst (voor de volgorde en de
/// kleur) en de al vertaalde teksten.</summary>
public sealed record GenerationIssueRow(GenerationSeverity Severity, string SeverityLabel, string Message);
