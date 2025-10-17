# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           WaitFor-LinuxComputerReady.ps1
## Purpose:        This script pause the script until the specified file on remote machine is ready for accessing.
## Scenarios:      1) Wait for something done in another computer (such as server config, or testing on client). 
##                   a) If "test.finished.signal" found, that means the testing finished successfully;
##
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $signalFolder:        The signal folder path
# $signalFileName:      The signal file path
# $timeoutSec:          The time out seconds
# $vnetType:            The vnet type
#----------------------------------------------------------------------------

Param ( 
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    $VM,
    [string]$signalFolder,          # "C:\Test\TestResults\Signals"
    [string]$signalFileName,        # "config.finished.signal"
    [int]$timeoutSec        = 3600,  # 3600
    [string]$vnetType        = "External"  # External or Internal
)

Import-Module .\Common\LocalLinuxFunctionLib.psm1

$localFolder = "C:"

function CheckSignalFile{
    if ( [System.IO.File]::Exists( "$localFolder\$signalFileName" ) -eq $true )
    { 
        Remove-Item "$localFolder\$signalFileName"
        return $true
    }
    
    return $false
}

function Copy-SignalFileToLocal{     
    Param ($vmIP)
    # Copy File from Linux to Windows
    Write-TestSuiteInfo "Copy $signalFolder/$signalFileName to Windows $localFolder to save the $signalFileName file"

    Execute-PscpCopyLinuxFileToWindowsCommand -VmIP $vmIP -SourceFilePath "$signalFolder/$signalFileName" -DestinationFilePath "$localFolder\"

    Write-TestSuiteInfo "Complete to copy $signalFileName to Windows $localFolder "

    Write-TestSuiteInfo "Sleep 10 seconds to wait the file $signalFileName copy over..."
    Start-Sleep -Seconds 10
}

function Main{
    $vmName = $VM.hypervname
    Write-Host "Get VM $vmName $vnetType IP"

    $currentLinuxVMIP = ""
    if($vnetType -eq "Internal"){
        $currentLinuxVMIP = Get-LinuxVMPrivateIP -VM $VM
    }else{
        $currentLinuxVMIP = Get-LinuxVMPublicIP -VM $VM
    }

    $retryCount = 0
    for (; $retryCount -lt $timeoutSec/2; $retryCount++){
        Copy-SignalFileToLocal -vMIP $currentLinuxVMIP
        if(CheckSignalFile -eq $true){
            break
        }

        Start-Sleep 3
    } 
}

Main