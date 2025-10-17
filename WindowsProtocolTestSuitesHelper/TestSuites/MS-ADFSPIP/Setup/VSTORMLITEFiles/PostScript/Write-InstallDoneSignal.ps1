# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

Write-Host  "Write signal file to system drive."

$testDir = $env:SystemDrive + "\Temp"
$ConfigureFile = "$testDir\Protocol.xml"
[xml]$xmlContent = Get-Content $ConfigureFile

$computerName = Get-Content "$testDir\name.txt"

$VMs = $xmlContent.SelectNodes("lab/servers/vm")
foreach($VMNode in $VMs)
{
	if($VMNode.name -eq $computerName)
	{
		$VM = $VMNode
	}
}

if($VM -eq $null)
{
    Write-Host "Cannot find VM with the name Vm $computerName." -ForegroundColor Red
	Stop-Transcript
    exit 1
}

$TestsuiteZips = $VM.Tools.GetElementsByTagName("TestsuiteZip")

foreach($TestsuiteZip in $TestsuiteZips)
{
	$targetFolder = $TestsuiteZip.targetFolder
	$MSIScriptsFile = [System.IO.Directory]::GetFiles($targetFolder, "ParamConfig.xml", [System.IO.SearchOption]::AllDirectories)
    [String]$TestSuiteScriptsFullPath = [System.IO.Directory]::GetParent($MSIScriptsFile)
    cmd /c ECHO $TestSuiteScriptsFullPath >$env:HOMEDRIVE\MSIInstalled.signal
}
