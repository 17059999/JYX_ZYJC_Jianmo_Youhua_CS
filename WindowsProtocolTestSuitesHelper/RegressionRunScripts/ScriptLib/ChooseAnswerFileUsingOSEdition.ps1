#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Remove-VMs.ps1
## Purpose:        Destroy and Remove VM folders
## Requirements:   Windows Powershell 2.0, 3.0
## Supported OS:   Windows Server 8, Windows Server 2012
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################

#Version_OSEdition e.g. 14300_ServerDataCenter
#Powershell [WorkingDir]\ScriptLib\ChooseAnswerFileUsingOSEdition.ps1 -WorkingDir [WorkingDir] -RoleDisk [ServerRoleDisk] -OSEdition [ServerRoleOSEdition] -IsClient $false
#Powershell [WorkingDir]\ScriptLib\ChooseAnswerFileUsingOSEdition.ps1 -WorkingDir [WorkingDir] -RoleDisk [ClientRoleDisk] -OSEdition [ClientRoleOSEdition] -IsClient $true

param(
[string]$WorkingDir = "D:WinteropProtocolTesting\", #[WorkingDir]
[string]$RoleDisk = "14300.0.amd64fre.fbl_impressive.150424-1350_server_ServerDataCenter_en-us.vhd",
[string]$OSEdition = "ServerDataCenter", # ServerStandard, Enterprise, ServerDataCenter
[string]$IsClient = "FALSE",

[string]$OSBuildNumber = $RoleDisk.Split(".")[0],
[string]$parentPath = ($WorkingDir + "\VSTORMLITE\parent"),
[string]$DesFile = ($parentPath + "\Enterpriseanswerfile.xml") # is change to edition argument, should delete this hard code!
)

#-----------------------------------------------------------------------------
# Check Parameters
#-----------------------------------------------------------------------------

if($OSBuildNumber -eq $null -or $OSBuildNumber.Trim() -eq "")
{
	Write-Info.ps1 "RoleDisk Could not be null or empty." -ForegroundColor Red
	return 1;
}

if($OSEdition -eq $null -or $OSEdition.Trim() -eq "")
{
	Write-Info.ps1 "OSEdition Could not be null or empty." -ForegroundColor Red
	return 1;
}

if($RoleDisk -eq $null -or $RoleDisk.Trim() -eq "")
{

	Write-Info.ps1 "RoleDisk Could not be null or empty." -ForegroundColor Red
	return 1;
}

#------------------------------------------------------------------------------
# Start Choosing the answer file
#------------------------------------------------------------------------------
$Key = "ServerRoleAnswerfile"
#for server
$ChooseAnswerEdition = "" + $OSBuildNumber + "_" + $OSEdition + ".xml"
#for client
$ChoosedAnswerFile = $parentPath + "\" + $OSBuildNumber + "_" + $OSEdition + ".xml"
$NonAnswerFile = $false

if(!(Test-Path $ChoosedAnswerFile))
{
	$NonAnswerFile = $true
	$currentVersion = [convert]::ToInt32($OSBuildNumber)
	$preVersion = 0
	
	Get-ChildItem $parentPath -Filter *_$OSEdition.xml | 
	Foreach-Object {
		$ver = [convert]::ToInt32($_.Name.Split("_")[0])
		if( ($ver -lt $currentVersion) -and ($ver -ge $preVersion))
		{
			$NonAnswerFile = $false
			$preVersion = $ver
		}
	}
    $ChoosedAnswerFile = $parentPath + "\" + $preVersion + "_" + $OSEdition + ".xml"
	$ChooseAnswerEdition = "" + $preVersion + "_" + $OSEdition + ".xml"
}

if($NonAnswerFile)
{
	Write-Info.ps1 "Can not find an answer file." -ForegroundColor Red
	return 1;
}

if($IsClient.ToLower() -eq "true")
{
    #write-host "enterprise_" + $ChoosedAnswerFile + $DesFile
	if (Test-Path $DesFile)
	{
		Remove-Item $DesFile -Force
	}
	Write-Info.ps1 "choose Client answer file $ChoosedAnswerFile"
	Copy-Item $ChoosedAnswerFile $DesFile -Force
}
else
{
    #write-host $OSEdition + "_" + $ChooseAnswerEdition
	Write-Info.ps1 "choose Server answer file $ChooseAnswerEdition"
	wttcmd.exe /SysInitKey /Key:$Key /Value:$ChooseAnswerEdition
}

exit 0