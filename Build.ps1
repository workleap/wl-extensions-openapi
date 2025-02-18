#Requires -Version 5.0

Begin {
    $ErrorActionPreference = "stop"
}

Process {
    function Exec([scriptblock]$Command) {
        & $Command
        if ($LASTEXITCODE -ne 0) {
            throw ("An error occurred while executing command: {0}" -f $Command)
        }
    }

    function Compare-GeneratedAndExpectedFiles {
        param(
            [Parameter(Mandatory=$true)]
            [string]$generatedFilePath,

            [Parameter(Mandatory=$true)]
            [string]$expectedFilePath
        )

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
    }

    $workingDir = Join-Path $PSScriptRoot "src"
    $outputDir = Join-Path $PSScriptRoot ".output"
    $nupkgsPath = Join-Path $outputDir "*.nupkg"

    $projectPath = Join-Path $workingDir "tests/WebApi.OpenAPI.SystemTest"
    $generatedFilePath = Join-Path $projectPath "openapi-v1.yaml"
    $expectedFilePath = Join-Path $workingDir "tests/expected-openapi-document.yaml"


    try {
        Push-Location $workingDir
        Remove-Item $outputDir -Force -Recurse -ErrorAction SilentlyContinue

        Exec { & dotnet clean -c Release }
        Exec { & dotnet build -c Release }
        Exec { & dotnet test  -c Release --no-build --results-directory "$outputDir" --no-restore -l "trx" -l "console;verbosity=detailed" }
        Exec { & Compare-GeneratedAndExpectedFiles -generatedFilePath $generatedFilePath -expectedFilePath $expectedFilePath }
        Exec { & dotnet pack  -c Release -o "$outputDir" }

        if (($null -ne $env:NUGET_SOURCE ) -and ($null -ne $env:NUGET_API_KEY)) {
            Exec { & dotnet nuget push "$nupkgsPath" -s $env:NUGET_SOURCE -k $env:NUGET_API_KEY --skip-duplicate }
        }
    }
    finally {
        Pop-Location
    }
}
