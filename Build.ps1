param(
    [Parameter(Mandatory = $false)][string] $Configuration = "Release",
    [Parameter(Mandatory = $false)][string] $VersionSuffix = "",
    [Parameter(Mandatory = $false)][string] $OutputPath = "",
    [Parameter(Mandatory = $false)][switch] $PatchVersion,
    [Parameter(Mandatory = $false)][switch] $SkipTests,
    [Parameter(Mandatory = $false)][switch] $DisableCodeCoverage
)

$ErrorActionPreference = "Stop"

$solutionPath = Split-Path $MyInvocation.MyCommand.Definition
$solutionFile = Join-Path $solutionPath "Website.sln"
$sdkFile      = Join-Path $solutionPath "global.json"

$dotnetVersion = (Get-Content $sdkFile | ConvertFrom-Json).sdk.version

if ($OutputPath -eq "") {
    $OutputPath = Join-Path "$(Convert-Path "$PSScriptRoot")" "artifacts"
}

if ($env:CI -ne $null -Or $env:TF_BUILD -ne $null) {
    $PatchVersion = $true
}

$installDotNetSdk = $false;

if (((Get-Command "dotnet.exe" -ErrorAction SilentlyContinue) -eq $null) -and ((Get-Command "dotnet" -ErrorAction SilentlyContinue) -eq $null)) {
    Write-Host "The .NET Core SDK is not installed."
    $installDotNetSdk = $true
}
else {
    $installedDotNetVersion = (dotnet --version | Out-String).Trim()
    if ($installedDotNetVersion -ne $dotnetVersion) {
        Write-Host "The required version of the .NET Core SDK is not installed. Expected $dotnetVersion but $installedDotNetVersion was found."
        $installDotNetSdk = $true
    }
}

if ($installDotNetSdk -eq $true) {
    $env:DOTNET_INSTALL_DIR = Join-Path "$(Convert-Path "$PSScriptRoot")" ".dotnetcli"

    if (!(Test-Path $env:DOTNET_INSTALL_DIR)) {
        mkdir $env:DOTNET_INSTALL_DIR | Out-Null
        $installScript = Join-Path $env:DOTNET_INSTALL_DIR "install.ps1"
        Invoke-WebRequest "https://raw.githubusercontent.com/dotnet/cli/v$dotnetVersion/scripts/obtain/dotnet-install.ps1" -OutFile $installScript -UseBasicParsing
        & $installScript -Version "$dotnetVersion" -InstallDir "$env:DOTNET_INSTALL_DIR" -NoPath
    }

    $env:PATH = "$env:DOTNET_INSTALL_DIR;$env:PATH"
    $dotnet = Join-Path "$env:DOTNET_INSTALL_DIR" "dotnet.exe"
}
else {
    $dotnet = "dotnet"
}

function DotNetTest {
    param([string]$Project)

    if ($DisableCodeCoverage -eq $true) {
        & $dotnet test $Project --output $OutputPath
    }
    else {

        if ($installDotNetSdk -eq $true) {
            $dotnetPath = $dotnet
        }
        else {
            $dotnetPath = (Get-Command "dotnet.exe").Source
        }

        $nugetPath = Join-Path $env:USERPROFILE ".nuget\packages"
        $openCoverVersion = "4.6.519"
        $openCoverPath = Join-Path $nugetPath "OpenCover\$openCoverVersion\tools\OpenCover.Console.exe"
        $coverageOutput = Join-Path $OutputPath "code-coverage.xml"

        & $openCoverPath `
            `"-target:$dotnetPath`" `
            `"-targetargs:test $Project --output $OutputPath`" `
            -output:$coverageOutput `
            -hideskipped:All `
            -mergebyhash `
            -oldstyle `
            -register:user `
            -skipautoprops `
            `"-filter:+[Website]* -[WebsiteTests]*`"
    }

    if ($LASTEXITCODE -ne 0) {
        throw "dotnet test failed with exit code $LASTEXITCODE"
    }
}

function DotNetPublish {
    param([string]$Project)
    $publishPath = (Join-Path $OutputPath "publish")
    if ($VersionSuffix) {
        & $dotnet publish $Project --output $publishPath --configuration $Configuration --version-suffix "$VersionSuffix"
    }
    else {
        & $dotnet publish $Project --output $publishPath --configuration $Configuration
    }
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet publish failed with exit code $LASTEXITCODE"
    }
}

if ($PatchVersion -eq $true) {

    $gitBranch = $env:BUILD_SOURCEBRANCHNAME

    if ([string]::IsNullOrEmpty($gitBranch)) {
        $gitBranch = (git rev-parse --abbrev-ref HEAD | Out-String).Trim()
    }

    $gitRevision = (git rev-parse HEAD | Out-String).Trim()
    $timestamp = [DateTime]::UtcNow.ToString("yyyy-MM-ddTHH:mm:ssK")

    $assemblyVersion = Get-Content ".\AssemblyVersion.cs" -Raw
    $assemblyVersionWithMetadata = "{0}using System.Reflection;`r`n`r`n[assembly: AssemblyMetadata(""CommitHash"", ""{1}"")]`r`n[assembly: AssemblyMetadata(""CommitBranch"", ""{2}"")]`r`n[assembly: AssemblyMetadata(""BuildTimestamp"", ""{3}"")]" -f $assemblyVersion, $gitRevision, $gitBranch, $timestamp

    Set-Content ".\AssemblyVersion.cs" $assemblyVersionWithMetadata -Encoding utf8
}

$testProjects = @(
    (Join-Path $solutionPath "tests\Website.Tests\Website.Tests.csproj")
)

$publishProjects = @(
    (Join-Path $solutionPath "src\Website\Website.csproj")
)

Write-Host "Publishing solution..." -ForegroundColor Green
ForEach ($project in $publishProjects) {
    DotNetPublish $project $Configuration $PrereleaseSuffix
}

if ($SkipTests -eq $false) {
    Write-Host "Testing $($testProjects.Count) project(s)..." -ForegroundColor Green
    ForEach ($project in $testProjects) {
        DotNetTest $project
    }
}

if ($PatchVersion -eq $true) {
    Set-Content ".\AssemblyVersion.cs" $assemblyVersion -Encoding utf8
}
