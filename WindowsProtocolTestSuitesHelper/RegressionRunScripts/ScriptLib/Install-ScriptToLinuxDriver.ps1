# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           InstallScriptToLinuxDriver.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7
##
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $VM:                     The driver VM configured xml data
# $Setup:                  The Setup configured xml data
# $TestSuiteName:          The test suite name
# $EnvironmentName:        The test suite environment name
#----------------------------------------------------------------------------

Param(
    [Parameter(ValueFromPipeline=$True, Mandatory = $True)]
    $VM,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
	[Xml]$Setup,
	[Parameter(ValueFromPipeline = $True, Mandatory = $True)]
	[string]$TestSuiteName,
	[Parameter(ValueFromPipeline = $True, Mandatory = $True)]
	[string]$EnvironmentName
)

Import-Module .\Common\LocalLinuxFunctionLib.psm1

Function Main{
	$testDir = $env:SystemDrive + "\Temp"

	Start-Transcript -Path "$testDir\InstallScriptToLinuxDriver.ps1.log" -Append -Force

	if($null -eq $VM){
		Write-Host "Cannot find Vm configure for Vm $VM." -ForegroundColor Red
		Stop-Transcript
		exit 1
	}

	$localScriptFolderName = "Scripts"
	$localScriptPath = "$PSScriptRoot\..\ProtocolTestSuite\$TestSuiteName\$localScriptFolderName"

    # Move Protocol configuration file to Script folder
	$protocolConfigFile = "$PSScriptRoot\..\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\$EnvironmentName"
	$destinationProtocolFileName = "Protocol.xml"
	Copy-Item $protocolConfigFile -Destination "$localScriptPath\$destinationProtocolFileName" -Force -Recurse

	Write-Host "Get VM IP $currentLinuxVMIP"
	$vmName = $Vm.hypervname
	$currentLinuxVMIP = Get-LinuxVMPublicIP -VM $Vm

	if(!(Test-Path -Path $localScriptPath)){
		Write-Host "$localScriptPath is not existed"
		return
	}

	Write-Host "Copy $localScriptPath to Linux / "

	Execute-PscpCopyWindowsFolderToLinuxCommand -VmIP $currentLinuxVMIP -SourceFilePath $localScriptPath -DestinationFilePath "/"

	Write-TestSuiteInfo "Sleep 120 seconds to wait the Script files Copy over..."
	Start-Sleep -Seconds 120
	
	# Update folder name "Scripts" to "Temp" folder
	$remoteLinuxFolderName = "Temp"
	Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
		-ShCommand "mv /$localScriptFolderName /$remoteLinuxFolderName" `
		-ShCommandKey "mv_folderName" 

	Write-TestSuiteInfo "Sleep 10 seconds to wait the $localScriptFolderName folder changed over..."
	Start-Sleep -Seconds 10	
}

Main