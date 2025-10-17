# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-TestSuiteToLinuxDriver.ps1
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

Function Main{
	$testDir = $env:SystemDrive + "\Temp"

	Start-Transcript -Path "$testDir\Install-TestSuiteToLinuxDriver.ps1.log" -Append -Force

	if($null -eq $VM){
		Write-Host "Cannot find Vm configure for Vm $VM." -ForegroundColor Red
		Stop-Transcript
		exit 1
	}

	Write-Host "Get VM IP $currentLinuxVMIP"
	$vmName = $Vm.hypervname
	$currentLinuxVMIP = Get-LinuxVMPublicIP -VM $Vm

	$remoteLinuxFolder = "/"

	# Copy and unzip the test suites.
	[array]$TestsuiteZips = $VM.Tools.GetElementsByTagName("TestsuiteZip")
	Write-Host "TestsuiteZip_Name: $TestsuiteZips"
	foreach($TestsuiteZip in $TestsuiteZips)
	{
		$zipName = $TestsuiteZip.ZipName
		$targetFolder = $TestsuiteZip.targetFolder

		write-host "Copy test suite: $zipName"
		if ($zipName -match ".zip") {
			$localPath = "$PSScriptRoot\..\ProtocolTestSuite\$TestSuiteName\deploy\$zipName"
			if(!(Test-Path -Path $localPath)){
				Write-Host "$localPath is not existed"
				return
			}

			Write-Host "Copy $localPath to Linux $remoteLinuxFolder "
			Execute-PscpCopyWindowsFileToLinuxCommand -VmIP $currentLinuxVMIP -SourceFilePath $localPath -DestinationFilePath $remoteLinuxFolder		

            Write-TestSuiteInfo "Sleep 60 seconds to wait the Test Suite Case folder zip copy over..."
			Start-Sleep -Seconds 60
			
			# Create target unzip folder
			$unzipLinuxFolder = $targetFolder
			Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
			-ShCommand "mkdir -p $unzipLinuxFolder" `
			-ShCommandKey "mkdir_unzipLinuxFolder"	

			Start-Sleep -Seconds 10
			
			# Unzip the file
			$zipPathInLinux = $remoteLinuxFolder + $zipName
			Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
				-ShCommand "unzip $zipPathInLinux -d $unzipLinuxFolder" `
				-ShCommandKey "unzip"

			#Write a MSI install completed signal file
		    $signFileName = "MSIInstalled.signal"

			Execute-PlinkShCommand -VmIP $currentLinuxVMIP `
				-ShCommand "echo $unzipLinuxFolder/Scripts > /$signFileName" `
				-ShCommandKey "Write_Signal_File"

			Write-TestSuiteInfo "Sleep 120 seconds to wait the unzip Test Suite Case folder over"
			Start-Sleep -Seconds 120
		}
	}
}

Main