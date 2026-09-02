namespace InnoSetupStudio.Core.Project;

/// <summary>Laadt en bewaart een <see cref="InstallerProject"/> als projectbestand op schijf.</summary>
public interface IInstallerProjectService
{
    Task<InstallerProject> LoadAsync(string filePath);

    Task SaveAsync(string filePath, InstallerProject project);
}
