param(
    [Parameter(Mandatory = $false)]
    [string]$PackagePath = "artifacts/nuget/VAutomationCore.1.0.1-beta.1.nupkg",

    [Parameter(Mandatory = $false)]
    [string]$SymbolsPackagePath = "artifacts/nuget/VAutomationCore.1.0.1-beta.1.snupkg",

    [Parameter(Mandatory = $true)]
    [string]$CertificatePath,

    [Parameter(Mandatory = $true)]
    [string]$CertificatePassword,

    [Parameter(Mandatory = $false)]
    [string]$TimestampUrl = "http://timestamp.digicert.com",

    [Parameter(Mandatory = $true)]
    [string]$NuGetSource,

    [Parameter(Mandatory = $false)]
    [string]$ApiKey = "",

    [Parameter(Mandatory = $false)]
    [switch]$SignSymbols = $true,

    [Parameter(Mandatory = $false)]
    [switch]$PushSymbols = $true
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Require-Command {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command not found: $Name"
    }
}

function Resolve-NuGetCommand {
    $cmd = Get-Command nuget -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $fallback = "C:\Users\coyot.RWE\tools\nuget.exe"
    if (Test-Path $fallback) { return $fallback }
    throw "Required command not found: nuget (and fallback not found at $fallback)"
}

function Run-Step {
    param(
        [string]$Name,
        [scriptblock]$Action
    )
    Write-Host "==> $Name"
    & $Action
}

function Push-Package {
    param(
        [string]$PathToPush
    )
    if ([string]::IsNullOrWhiteSpace($ApiKey)) {
        dotnet nuget push $PathToPush --source $NuGetSource --skip-duplicate
    } else {
        dotnet nuget push $PathToPush --source $NuGetSource --api-key $ApiKey --skip-duplicate
    }
}

Require-Command "dotnet"
$nugetCmd = Resolve-NuGetCommand

if (-not (Test-Path $PackagePath)) {
    throw "Package not found: $PackagePath"
}
if (-not (Test-Path $CertificatePath)) {
    throw "Certificate not found: $CertificatePath"
}

Run-Step "Sign main package" {
    & $nugetCmd sign $PackagePath `
        -CertificatePath $CertificatePath `
        -CertificatePassword $CertificatePassword `
        -Timestamper $TimestampUrl
}

Run-Step "Verify main package signature" {
    & $nugetCmd verify -Signatures $PackagePath
}

if ($SignSymbols -and (Test-Path $SymbolsPackagePath)) {
    Run-Step "Sign symbols package" {
        & $nugetCmd sign $SymbolsPackagePath `
            -CertificatePath $CertificatePath `
            -CertificatePassword $CertificatePassword `
            -Timestamper $TimestampUrl
    }

    Run-Step "Verify symbols package signature" {
        & $nugetCmd verify -Signatures $SymbolsPackagePath
    }
}

Run-Step "Push main package" {
    Push-Package -PathToPush $PackagePath
}

if ($PushSymbols -and (Test-Path $SymbolsPackagePath)) {
    Run-Step "Push symbols package" {
        Push-Package -PathToPush $SymbolsPackagePath
    }
}

Write-Host "Completed sign/verify/push pipeline."
