# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

# Function to install PTMService
function Install-PTMService {
    param(
        [string]$HttpPort,
        [string]$HttpsPort
    )

    # Generate a developer certificate
    dotnet dev-certs https

    $ptmServiceRootPath = "/opt/ptmservice"
    $flagPath = "$ptmServiceRootPath/.installed"

    $installationId = "preinstalled"
    $storageRootPath = "$ptmServiceRootPath/$installationId"
    $storageDatabasePath = "$storageRootPath/ptmservice.db"

    mkdir $storageRootPath
    Copy-Item "$ptmServiceRootPath/ptmservice.db" $storageDatabasePath

    $appSettingsPath = "$ptmServiceRootPath/appsettings.json"
    $appSettings = Get-Content $appSettingsPath | ConvertFrom-Json
    $appSettings.ConnectionStrings.Database = "Data Source = `"$storageDatabasePath`""
    $appSettings.PTMServiceStorageRoot = $storageRootPath

    [System.IO.File]::WriteAllText($appSettingsPath, ($appSettings | ConvertTo-Json))

    $flag = @{
        installationId      = $installationId
        storageRootPath     = $storageRootPath
        storageDatabasePath = $storageDatabasePath
    }

    [System.IO.File]::WriteAllText($flagPath, ($flag | ConvertTo-Json))

    $envSetter = "`$env:ASPNETCORE_URLS = `"http://0.0.0.0`:$HttpPort;https://0.0.0.0`:$HttpsPort`""
    $placeholder = "# `$env:ASPNETCORE_URLS = `"http://localhost:5000;https://localhost:5001`""

    $runScriptPath = "$ptmServiceRootPath/run.ps1"
    $runScript = [System.IO.File]::ReadAllText($runScriptPath)
    $runScript = $runScript.Replace($placeholder, $envSetter)

    [System.IO.File]::WriteAllText($runScriptPath, $runScript)
}

# Function to install test suite
function Install-TestSuite {
    param(
        [string]$HttpsPort,
        [string]$TestSuiteName,
        [string]$TestSuitePackageName
    )

    $ptmServiceUrl = "https://127.0.0.1:$HttpsPort/api/management/testsuite"

    $installationRequest = @{
        TestSuiteName = $TestSuiteName
        Package       = Get-Item "/$TestSuitePackageName"
        Description   = "Preinstalled $TestSuiteName Package."
    }

    Invoke-RestMethod -Uri $ptmServiceUrl -Method Post -Form $installationRequest -SkipCertificateCheck
}

# Print the parameters passed in
Write-Host "HttpPort: $env:HttpPort"
Write-Host "HttpsPort: $env:HttpsPort"
Write-Host "ServerIPAddress: $env:ServerIPAddress"
Write-Host "TestSuiteName: $env:TestSuiteName"
Write-Host "TestSuitePackageName: $env:TestSuitePackageName"

# Update /etc/hosts
& "$PSScriptRoot/updateHosts.ps1" -ServerIPAddress $env:ServerIPAddress
if($LASTEXITCODE -ne 0) {
    Write-Host "Failed updating /etc/hosts. Tests cannot be run."
    exit -1
}

# Install PTMService
Install-PTMService -HttpPort $env:HttpPort -HttpsPort $env:HttpsPort

# Start the PTMService
bash -c "cd /opt/ptmservice && nohup /opt/ptmservice/run.sh &"

Start-Sleep -Seconds 30

# Install test suite to the PTMService
Install-TestSuite -HttpsPort $env:HttpsPort -TestSuiteName $env:TestSuiteName -TestSuitePackageName $env:TestSuitePackageName

# Wait for termination from user
try {
    Write-Host "Enter X to exit the PTMService execution..."

    while ($true) {
        Start-Sleep -Seconds 1
        $userInput = Read-Host 
        if ($userInput -match "[xX]") {
            break
        } 
    }
}
finally {
    Write-Host "PTMService execution ended."
}