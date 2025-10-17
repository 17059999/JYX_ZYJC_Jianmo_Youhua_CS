# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
    [string]$ServerIPAddress,
    [string]$SutIPAddress = "",
    [string]$PtfConfigFile = "/opt/rdpclient/Bin/RDP_ClientTestSuite.deployment.ptfconfig"
)

function Update-Hostname {
    param (
        [string]$IPAddress,
        [string]$PropertyName
    )
    [xml]$ptfConfig = Get-Content $PtfConfigFile
    $propertyNodes = $ptfConfig.GetElementsByTagName("Property")
    foreach ($node in $PropertyNodes) {
        if ($node.GetAttribute("name") -eq $PropertyName) {
            $Hostname = $node.GetAttribute("value")
            Update-Hostsfile -IPAddress $IPAddress -Hostname $Hostname
            break
        }
    }
}

function Update-Hostsfile {
    param (
        [string]$IPAddress,
        [string]$Hostname
    )
    if(-not ($IPAddress -match '^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$')) {
        Write-Host "$($IPAddress) is an invalid IPAddress"
        return 1
    }

    if(-not ($Hostname -match '^[a-zA-Z0-9.-]+$')) {
        Write-Host "$($Hostname) is an invalid Hostname"
        return 1
    }

    $hostsfile = "/etc/hosts"

    if(Test-Path $hostsfile -PathType Leaf) {
        # Get the content of the hosts file as an array of lines
        $content = Get-Content -Path $hostsfile

        # Check if the Hostname exists in the hosts file
        $match = $content | Where-Object {$_ -match $Hostname}

        if ($match) {
            # If the hostname exists, replace its IPAddress with the new one
            $content = $content | ForEach-Object {$_ -replace "\d+\.\d+\.\d+\.\d+.*$Hostname", "$IPAddress `t`t$Hostname"}
            Write-Host "Replacing IPAddress for $($Hostname) with '$($IPAddress)'"
        } else {
            # If the hostname does not exist, append a new line with its IP and name
            $content += "$IPAddress `t`t$Hostname"
            Write-Host "Updating /etc/hosts with '$($IPAddress) $($Hostname)'"
        }

        Set-Content -Path $hostsfile -Value $content
    }
    else {
        Set-Content -Path /etc/hosts -Value "`n$($IPAddress) $($Hostname)"
        Write-Host "Created /etc/hosts with '$($IPAddress) $($Hostname)'"
    }
    return 0
}

if(-not [string]::IsNullOrWhiteSpace($SutIPAddress)) {
    Update-Hostname -PropertyName "SUTName" -IPAddress $SutIPAddress
}

# The Server should be the hostname of the container
# Exit with -1 if we have any issue updating the Server entry in /etc/hosts
$serverHostname = Get-Content -Path /etc/hostname -First 1
$response = 1
if(-not [string]::IsNullOrWhiteSpace($serverHostname)) {
    $response = Update-Hostsfile -Hostname $serverHostname -IPAddress $ServerIPAddress
}

if($response -eq 1) {
    Write-Host "Failed updating /etc/hosts with the Server IPAddress. Tests cannot be run."
    exit -1
}

exit 0