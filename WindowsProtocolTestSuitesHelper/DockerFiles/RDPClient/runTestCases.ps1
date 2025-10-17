# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$ptfConfigFile = "/opt/rdpclient/Bin/RDP_ClientTestSuite.deployment.ptfconfig"

# Function to update PTF config file
function Update-PTFConfig {
    param (
        [string]$PropertyName,
        [string]$PropertyValue
    )

    [xml]$ptfConfig = Get-Content $ptfConfigFile
    $propertyNodes = $ptfConfig.GetElementsByTagName("Property")
    foreach ($node in $PropertyNodes) {
        if ($node.GetAttribute("name") -eq $PropertyName) {
            $node.SetAttribute("value", $PropertyValue)
            $ptfConfig.Save($ptfConfigFile)

            if($PropertyName -eq "ServerName") {
                & "$PSScriptRoot\UpdateHosts.ps1" $enc:ServerIPAddress $HostName
            }
            break
        }
    }
}

# Helper function for converting *nix environment variables to PowerShell booleans
# adapted from here: https://docs.python.org/3/distutils/apiref.html?highlight=strtobool#distutils.util.strtobool
function Test-EnvString {
    param(
        [string]$EnvString
    )
    $envStringLower = $EnvString.ToLower()
    !(("n","no","f","false","off","0","").contains($envStringLower))
}

# Print the parameters passed in
Write-Host "Filter: $env:Filter"
Write-Host "DryRun: $env:DryRun"
Write-Host "ServerIPAddress: $env:ServerIPAddress"

if (Test-Path "/data/rdpclient") {
    # Copy config files if they exist
    $ptfConfigFiles = Get-Item "/data/rdpclient/*.ptfconfig"
    if ($ptfConfigFiles.Length -gt 0) {
        Copy-Item "/data/rdpclient/*.ptfconfig" "/opt/rdpclient/Bin" -Force
    }

    # Update PTF config files
    if(-not [string]::IsNullOrWhiteSpace($env:ServerPort)) {
        Update-PTFConfig -PropertyName "ServerPort" -PropertyValue $env:ServerPort
    }
    if(-not [string]::IsNullOrWhiteSpace($env:SutName)) {
        Update-PTFConfig -PropertyName "SutName" -PropertyValue $env:SutName
    }
    if(-not [string]::IsNullOrWhiteSpace($env:SutUserName)) {
        Update-PTFConfig -PropertyName "SutUserName" -PropertyValue $env:SutUserName
    }
    if(-not [string]::IsNullOrWhiteSpace($env:SUTUserPassword)) {
        Update-PTFConfig -PropertyName "SutUserPassword" -PropertyValue $env:SutUserPassword
    }
    if(-not [string]::IsNullOrWhiteSpace($env:ServerUserName)) {
        Update-PTFConfig -PropertyName "ServerUserName" -PropertyValue $env:ServerUserName
    }
    if(-not [string]::IsNullOrWhiteSpace($env:ServerUserPassword)) {
        Update-PTFConfig -PropertyName "ServerUserPassword" -PropertyValue $env:ServerUserPassword
    }

    # Update /etc/hosts
    & "$PSScriptRoot/updateHosts.ps1" -ServerIPAddress $env:ServerIPAddress
    if($LASTEXITCODE -ne 0) {
        Write-Host "Failed updating /etc/hosts. Tests cannot be run."
        exit -1
    }

    # Start the script to execute test cases
    if (Test-EnvString $env:DryRun) {
        & /opt/rdpclient/Batch/RunTestCasesByFilter.ps1 -Filter $env:Filter -DryRun
        Write-Host "Listed all the test cases which match the filter condition."
    }
    else {
        & /opt/rdpclient/Batch/RunTestCasesByFilter.ps1 -Filter $env:Filter
        Copy-Item "/opt/rdpclient/TestResults" "/data/rdpclient" -Recurse -Force
    }
}
else {
    Write-Host "The path /data/rdpclient does not exist, please use -v to mount to it."
}
