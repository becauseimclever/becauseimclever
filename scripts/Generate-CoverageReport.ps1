<#
.SYNOPSIS
    Generates a code coverage report for the BecauseImClever solution.

.DESCRIPTION
    This script runs all unit tests with code coverage collection enabled,
    then generates an HTML report using ReportGenerator.

.PARAMETER OutputDirectory
    The directory where the coverage report will be generated.
    Default: ./coverage-report

.PARAMETER OpenReport
    If specified, opens the generated HTML report in the default browser.

.EXAMPLE
    .\Generate-CoverageReport.ps1

.EXAMPLE
    .\Generate-CoverageReport.ps1 -OutputDirectory "./my-report" -OpenReport
#>

param(
    [string]$OutputDirectory = "./coverage-report",
    [switch]$OpenReport
)

$ErrorActionPreference = "Stop"

# Get the solution root directory
$SolutionRoot = Split-Path -Parent $PSScriptRoot

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Code Coverage Report Generator" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Clean up previous test results
$TestResultsPath = Join-Path $SolutionRoot "TestResults"
if (Test-Path $TestResultsPath) {
    Write-Host "Cleaning up previous test results..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $TestResultsPath
}

# Clean up previous coverage report
$ReportPath = Join-Path $SolutionRoot $OutputDirectory
if (Test-Path $ReportPath) {
    Write-Host "Cleaning up previous coverage report..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $ReportPath
}

# Ensure ReportGenerator is installed
Write-Host "Checking for ReportGenerator tool..." -ForegroundColor Yellow
$reportGeneratorInstalled = dotnet tool list -g | Select-String "dotnet-reportgenerator-globaltool"
if (-not $reportGeneratorInstalled) {
    Write-Host "Installing ReportGenerator..." -ForegroundColor Yellow
    dotnet tool install -g dotnet-reportgenerator-globaltool
}

# Run tests with coverage
Write-Host ""
Write-Host "Running tests with code coverage..." -ForegroundColor Green
Push-Location $SolutionRoot
try {
    dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage" --results-directory TestResults
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests failed! See output above for details." -ForegroundColor Red
        exit 1
    }
}
finally {
    Pop-Location
}

# Find coverage files
$CoverageFiles = Get-ChildItem -Path $TestResultsPath -Recurse -Filter "coverage.cobertura.xml"
if ($CoverageFiles.Count -eq 0) {
    Write-Host "No coverage files found!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Found $($CoverageFiles.Count) coverage file(s)" -ForegroundColor Green

# Generate report
Write-Host ""
Write-Host "Generating HTML report..." -ForegroundColor Green
$CoverageFilePaths = ($CoverageFiles | ForEach-Object { $_.FullName }) -join ";"

reportgenerator `
    -reports:"$CoverageFilePaths" `
    -targetdir:"$ReportPath" `
    -reporttypes:"Html;Badges;TextSummary" `
    -title:"BecauseImClever Coverage Report"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to generate report!" -ForegroundColor Red
    exit 1
}

# Display summary
$SummaryFile = Join-Path $ReportPath "Summary.txt"
if (Test-Path $SummaryFile) {
    Write-Host ""
    Write-Host "======================================" -ForegroundColor Cyan
    Write-Host "  Coverage Summary" -ForegroundColor Cyan
    Write-Host "======================================" -ForegroundColor Cyan
    Get-Content $SummaryFile | Write-Host
}

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Report Generated Successfully!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Report location: $ReportPath" -ForegroundColor Yellow
Write-Host "Open: $ReportPath\index.html" -ForegroundColor Yellow

# Open report if requested
if ($OpenReport) {
    $IndexPath = Join-Path $ReportPath "index.html"
    if (Test-Path $IndexPath) {
        Start-Process $IndexPath
    }
}
