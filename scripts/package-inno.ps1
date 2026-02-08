param(
    [string]$Configuration = "Release",
    [string]$Version = "1.0.0",
    [switch]$SkipIscc
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$solutionPath = Join-Path $repoRoot "DesktopMemo.sln"
$appOutput = Join-Path $repoRoot "src/DesktopMemo.App/bin/$Configuration/net48"
$stagingDir = Join-Path $repoRoot "artifacts/staging/net48"
$installerDir = Join-Path $repoRoot "artifacts/installer"
$issPath = Join-Path $repoRoot "deploy/inno/DesktopMemo.iss"

Write-Host "Building DesktopMemo ($Configuration)..."
dotnet build $solutionPath -c $Configuration

if (-not (Test-Path $appOutput)) {
    throw "App output folder not found: $appOutput"
}

New-Item -ItemType Directory -Force $stagingDir | Out-Null
New-Item -ItemType Directory -Force $installerDir | Out-Null
Remove-Item (Join-Path $stagingDir "*") -Recurse -Force -ErrorAction SilentlyContinue

Write-Host "Staging files..."
Copy-Item (Join-Path $appOutput "*") $stagingDir -Recurse -Force
Get-ChildItem $stagingDir -Recurse -Filter *.pdb | Remove-Item -Force -ErrorAction SilentlyContinue

$exePath = Join-Path $stagingDir "DesktopMemo.App.exe"
if (-not (Test-Path $exePath)) {
    throw "DesktopMemo.App.exe not found in staging."
}

if ($SkipIscc) {
    Write-Host "SkipIscc enabled. Staged files at: $stagingDir"
    exit 0
}

function Find-Iscc {
    $candidates = @(
        "ISCC.exe",
        "$env:ProgramFiles(x86)\\Inno Setup 6\\ISCC.exe",
        "$env:ProgramFiles\\Inno Setup 6\\ISCC.exe"
    )

    foreach ($candidate in $candidates) {
        if (Get-Command $candidate -ErrorAction SilentlyContinue) {
            return (Get-Command $candidate).Source
        }

        if (Test-Path $candidate) {
            return $candidate
        }
    }

    return $null
}

$iscc = Find-Iscc
if (-not $iscc) {
    throw "ISCC.exe not found. Install Inno Setup 6 or run with -SkipIscc."
}

Write-Host "Building installer..."
& $iscc "/DAppVersion=$Version" "/DSourceDir=$stagingDir" "/DOutputDir=$installerDir" $issPath

Write-Host "Installer output:"
Get-ChildItem $installerDir -Filter *.exe | Select-Object -ExpandProperty FullName
