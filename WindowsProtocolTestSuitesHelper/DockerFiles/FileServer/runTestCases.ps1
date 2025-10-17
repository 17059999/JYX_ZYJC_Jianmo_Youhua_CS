# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

# Function to update PTF config file
function Update-PTFConfig {
    param (
        [string]$PropertyName,
        [string]$PropertyValue
    )

    [xml]$ptfConfig = Get-Content "/opt/fileserver/Bin/CommonTestSuite.deployment.ptfconfig"
    $propertyNodes = $ptfConfig.GetElementsByTagName("Property")
    foreach ($node in $PropertyNodes) {
        if ($node.GetAttribute("name") -eq $PropertyName) {
            $node.SetAttribute("value", $PropertyValue)
            $ptfConfig.Save("/opt/fileserver/Bin/CommonTestSuite.deployment.ptfconfig")
            break
        }
    }
}

# Function to update SharePath
function Update-SharePath {
    param(
        [string]$ShareHost
    )

    [xml]$ptfConfig = Get-Content "/opt/fileserver/Bin/MS-SMB2_ServerTestSuite.deployment.ptfconfig"
    $ptfConfig.PreserveWhitespace = $true
    $propertyNodes = $ptfConfig.GetElementsByTagName("Property")
    foreach ($node in $PropertyNodes) {
        if ($node.GetAttribute("name") -eq "SharePath") {
            $currentValue = $node.GetAttribute("value")
            $newValue = $currentValue -replace "\\\\[a-zA-Z0-9\.]*\\", "\\$ShareHost\"
            $node.SetAttribute("value", $newValue)
            $ptfConfig.Save("/opt/fileserver/Bin/MS-SMB2_ServerTestSuite.deployment.ptfconfig")
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
Write-Host "SutComputerName: $env:SutComputerName"
Write-Host "SutIPAddress: $env:SutIPAddress"
Write-Host "DomainName: $env:DomainName"
Write-Host "AdminUserName: $env:AdminUserName"
Write-Host "PasswordForAllUsers: $env:PasswordForAllUsers"

if (Test-Path "/data/fileserver") {
    # Copy config files if they exist
    $ptfConfigFiles = Get-Item "/data/fileserver/*.ptfconfig"
    if ($ptfConfigFiles.Length -gt 0) {
        Copy-Item "/data/fileserver/*.ptfconfig" "/opt/fileserver/Bin" -Force
    }

    # Update PTF config files
    Update-PTFConfig -PropertyName "SutComputerName" -PropertyValue $env:SutComputerName
    Update-SharePath -ShareHost $env:SutComputerName
    Update-PTFConfig -PropertyName "SutIPAddress" -PropertyValue $env:SutIPAddress
    Update-PTFConfig -PropertyName "DomainName" -PropertyValue $env:DomainName
    Update-PTFConfig -PropertyName "AdminUserName" -PropertyValue $env:AdminUserName
    Update-PTFConfig -PropertyName "PasswordForAllUsers" -PropertyValue $env:PasswordForAllUsers

    # Start the script to execute test cases
    if (Test-EnvString $env:DryRun) {
        & /opt/fileserver/Batch/RunTestCasesByFilter.ps1 -Filter $env:Filter -DryRun
        Write-Host "Listed all the test cases which match the filter condition."
    }
    else {
        & /opt/fileserver/Batch/RunTestCasesByFilter.ps1 -Filter $env:Filter
        Copy-Item "/opt/fileserver/TestResults" "/data/fileserver" -Recurse -Force
    }
}
else {
    Write-Host "The path /data/fileserver does not exist, please use -v to mount to it."
}
