# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-LinuxTestResult.ps1
## Purpose:        Copy test results from VM (Client VM and Server VM) to VM host.
##
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $VM:                    The driver VM configured xml data
# $testResultFolder:      The test result folder path in Linux driver
# $testSuiteName:         The test suite name
# $vnetType:              The vnetType
#----------------------------------------------------------------------------

Param ( 
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    $VM,
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    [string]$testResultFolder,          # "/Test/TestResults"
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    $testSuiteName,
    [string]$vnetType        = "External"  # External or Internal
)

Import-Module .\Common\LocalLinuxFunctionLib.psm1

$localFolder = "$PSScriptRoot\..\TestResults\$testSuiteName"
$currentLinuxVMIP = $null

function Copy-TestResultFolderToLocal{     
    Param ($vmIP)
    # Copy Folder from Linux to Windows
    Write-TestSuiteInfo "Copy $testResultFolder to Windows $localFolder"
    Execute-PscpCopyLinuxFolderToWindowsCommand -VmIP $vmIP -SourceFilePath $testResultFolder -DestinationFilePath "$localFolder\"

    Write-TestSuiteInfo "Sleep 30 seconds to wait the file $testResultFolder copy over..."
    Start-Sleep -Seconds 30

    # Move the trx file to the top folder
    Write-TestSuiteInfo "Move the trx file to the top folder $localFolder"
    if($testSuiteName -eq "FileServer"){
        $localTestResultFolderPath = $testResultFolder.Replace('/','\');
        $trxList = Get-ChildItem "$localFolder\$localTestResultFolderPath\TestResults\" -Filter "*.trx"
        $trxList | Copy-Item -Destination "$localFolder" -Force
    }else{
        $trxList = Get-ChildItem "$localFolder\TestResults\" -Filter "*.trx"
        $trxList | Copy-Item -Destination "$localFolder" -Force
    }

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

    Copy-TestResultFolderToLocal -vMIP $currentLinuxVMIP 
}

Main