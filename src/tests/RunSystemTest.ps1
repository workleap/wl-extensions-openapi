# Stop the script when a cmdlet or a native command fails
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

# Define the paths
$projectPath = Join-Path $PSScriptRoot "WebApi.OpenAPI.SystemTest"
$generatedFilePath = Join-Path $projectPath "openapi-v1.yaml"
$expectedFilePath = Join-Path $PSScriptRoot "expected-openapi-document.yaml"

# Compile the project
dotnet build $projectPath -c Release

# Check if the build was successful
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed."
    exit $LASTEXITCODE
}

# Compare the generated file with the expected file
$generatedFileContent = Get-Content -Path $generatedFilePath
$expectedFileContent = Get-Content -Path $expectedFilePath

$diff = Compare-Object -ReferenceObject $generatedFileContent -DifferenceObject $expectedFileContent

if ($diff) {
    $diff | Format-Table
    Write-Error "The generated file does not match the expected file."
    exit 1
} else {
    Write-Host "The generated file matches the expected file."
}