<#
.SYNOPSIS
    Berekent het releasenummer (YYYY.MM.dd.xxx) en schrijft het naar version.generated.props.

.DESCRIPTION
    Leest build\buildnumber.txt (formaat "YYYY.MM.dd|teller"). Bij een nieuwe dag begint de
    teller weer op 1, binnen dezelfde dag wordt de teller met 1 opgehoogd. Schrijft het
    resultaat naar version.generated.props in de solution root, die door Directory.Build.props
    wordt geimporteerd zodat elk project dezelfde AssemblyVersion/FileVersion/InformationalVersion
    krijgt.
#>

[CmdletBinding()]
param(
    [string]$RepoRoot
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = Split-Path -Parent $PSScriptRoot
}

$counterFile = Join-Path $RepoRoot 'build\buildnumber.txt'
$today = Get-Date -Format 'yyyy.MM.dd'

$lastDate = $null
$counter = 0
if (Test-Path -LiteralPath $counterFile) {
    $raw = (Get-Content -LiteralPath $counterFile -Raw).Trim()
    if ($raw -match '^(?<date>\d{4}\.\d{2}\.\d{2})\|(?<count>\d+)$') {
        $lastDate = $Matches['date']
        $counter = [int]$Matches['count']
    }
}

if ($lastDate -eq $today) {
    $counter++
} else {
    $counter = 1
}

"$today|$counter" | Set-Content -LiteralPath $counterFile -Encoding ascii -NoNewline

$parts = $today.Split('.')
$year = [int]$parts[0]
$month = [int]$parts[1]
$day = [int]$parts[2]

$displayVersion = "{0}.{1:D2}.{2:D2}.{3:D3}" -f $year, $month, $day, $counter
$numericVersion = "$year.$month.$day.$counter"

$propsPath = Join-Path $RepoRoot 'version.generated.props'
$propsContent = @"
<Project>
  <!-- Automatisch gegenereerd door build\Update-Version.ps1. Niet handmatig bewerken. -->
  <PropertyGroup>
    <AssemblyVersion>$numericVersion</AssemblyVersion>
    <FileVersion>$numericVersion</FileVersion>
    <InformationalVersion>$displayVersion</InformationalVersion>
  </PropertyGroup>
</Project>
"@
Set-Content -LiteralPath $propsPath -Value $propsContent -Encoding utf8

Write-Host "Releasenummer: $displayVersion"
return $displayVersion
