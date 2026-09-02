<#
.SYNOPSIS
    Hoogt het releasenummer op en bouwt de solution.
#>

[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot 'Update-Version.ps1') -RepoRoot $repoRoot

dotnet build (Join-Path $repoRoot 'InnoSetupStudio.slnx') --configuration $Configuration
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE." }
