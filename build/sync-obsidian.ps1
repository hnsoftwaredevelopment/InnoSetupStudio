<#
.SYNOPSIS
    Kopieert alle Markdown-documentatie van InnoSetupStudio naar de Obsidian-vault.

.DESCRIPTION
    Uitvoeren na elke afgeronde feature. Mirrort:
      - README.md
      - docs\**\*.md
    naar de vault-locatie, met behoud van de relatieve mapstructuur.
#>

[CmdletBinding()]
param(
    [string]$ProjectRoot,
    [string]$VaultTarget = 'c:\DevOps\hnsoftwaredevelopment\Obsidian\Development\HNSoftwareDevelopment\Inno Setup Studio'
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $scriptDir = if ($PSScriptRoot) { $PSScriptRoot }
                 elseif ($PSCommandPath) { Split-Path -Parent $PSCommandPath }
                 else { 'c:\DevOps\hnsoftwaredevelopment\InnoSetupStudio\build' }
    $ProjectRoot = Split-Path -Parent $scriptDir
}

Write-Host "Project root : $ProjectRoot"
Write-Host "Vault target : $VaultTarget"

New-Item -ItemType Directory -Force -Path $VaultTarget | Out-Null

Copy-Item -Path (Join-Path $ProjectRoot 'README.md') `
          -Destination (Join-Path $VaultTarget 'README.md') -Force

$docsRoot = Join-Path $ProjectRoot 'docs'
if (Test-Path $docsRoot) {
    $mdFiles = Get-ChildItem -Path $docsRoot -Recurse -Filter *.md -File
    foreach ($file in $mdFiles) {
        $relative = $file.FullName.Substring($docsRoot.Length).TrimStart('\')
        $dest     = Join-Path (Join-Path $VaultTarget 'docs') $relative
        $destDir  = Split-Path -Parent $dest
        New-Item -ItemType Directory -Force -Path $destDir | Out-Null
        Copy-Item -Path $file.FullName -Destination $dest -Force
        Write-Host "  synced: docs\$relative"
    }
}

Write-Host "Obsidian sync complete." -ForegroundColor Green
