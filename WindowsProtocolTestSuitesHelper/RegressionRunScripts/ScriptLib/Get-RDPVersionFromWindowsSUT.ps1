# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
    [string]$ComputerName,
    [string]$UserName,
    [string]$UserPassword
)

$rdpVersion = "8.1"
$secureUserPassword = ConvertTo-SecureString $UserPassword -AsPlainText  -Force
$userCred = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList @($UserName, $secureUserPassword)

Write-Host "Try to connect to computer $ComputerName ..."
$waitTimeout = 600
$sysInfo = $null
$retryCount = 0
for (; $retryCount -lt $waitTimeout / 2; $retryCount++ ) {
    if($ComputerName){
        $sysInfo = Get-WmiObject Win32_OperatingSystem -ComputerName $ComputerName -Credential $userCred
    } else {
        $sysInfo = Get-WmiObject Win32_OperatingSystem
    }
    
    if ($null -ne $sysInfo) {
        break;  
    }
    
    $noNewLineIndicator = $true
    if ($retryCount % 60 -eq 59) {
        $noNewLineIndicator = $False
    }
    Write-Host "." -NoNewLine:$noNewLineIndicator -ForegroundColor White
    
    Start-Sleep -Seconds 2
}

if ($null -eq $sysInfo) {
    Write-Host "Connect to computer $ComputerName failed."
}

if ($null -ne $sysInfo) {
    $version = $sysInfo.Version
    Write-Host "System Version is $version"

    $versionRegex = "(\d+\.\d+).(\d+)"
    $version -match $versionRegex | Out-Null
    [decimal]$osVersion = $Matches[1]
    [int]$osBuildNumber = $Matches[2]
    
    $rdpVersion = if ($osVersion -ge 10.0) {
        if ($osBuildNumber -ge 19041) {
            "10.8"
        }
        elseif ($osBuildNumber -ge 18362) {
            "10.7"
        }
        elseif ($osBuildNumber -ge 17763) {
            "10.6"
        }
        elseif ($osBuildNumber -ge 17134) {
            "10.5"
        }
        elseif ($osBuildNumber -ge 16299) {
            "10.4"
        }
        elseif ($osBuildNumber -ge 15063) {
            "10.3"
        }
        elseif ($osBuildNumber -ge 14393) {
            "10.2"
        }
        elseif ($osBuildNumber -ge 10586) {
            "10.1"
        }
        else {
            "10.0"
        }
    }
    elseif ($osVersion -ge 6.3) {
        "8.1"
    }
    elseif ($osVersion -ge 6.2) {
        "8.0"
    }
    elseif ($osVersion -ge 6.1) {
        if ($osBuildNumber -ge 7601) {
            "7.1"
        }
        else {
            "7.0"
        }
    }

    Write-Host "Set `$rdpVersion to $rdpVersion"
}
else {
    Write-Host "Cannot detect the Windows Version. Keep `$rdpVersion to 8.1."
}

return $rdpVersion