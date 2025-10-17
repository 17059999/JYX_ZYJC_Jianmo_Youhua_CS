#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Update-PTFConfigFiles.ps1
## Purpose:        Update PTF Config files for protocol test suite.
## Requirements:   Windows Powershell 2.0 or newer.
## Supported OS:   Windows Server 2012 or newer.
## Copyright (c) Microsoft Corporation. All rights reserved.
##
##############################################################################
Param(
[string]$testDirInVM = "$env:SystemDrive\Temp",
[String]$paramConfigFile  = "$env:SystemDrive\Temp\Protocol.xml"
)

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue | Out-Null
$logPath = "$testDirInVM\TestLog"
if ((Test-Path -Path $logPath) -eq $false)
{
    md $logPath
}
Start-Transcript -Path "$logPath\Update-PTFConfigFiles.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Remove config.finished.signal if exit.
#----------------------------------------------------------------------------
$configSignalFile = "$testDirInVM\config.finished.signal"
if (Test-Path -Path $configSignalFile)
{
    Remove-Item -Path $configSignalFile
}

#----------------------------------------------------------------------------
# Verify if paramConfigFile and ptfConfig file exist
#----------------------------------------------------------------------------
# Verify param configure file eixst.
if (!(Test-Path -Path $paramConfigFile))
{
    Write-Host "ParamConfigFile $paramConfigFile does not exist."
	exit 1
}

# Verify PTF configure file exist
$testsuite = get-childitem "$env:SystemDrive\MicrosoftProtocolTests\*"
$endpoint = get-childitem ("$env:SystemDrive\MicrosoftProtocolTests\" + $testsuite.Name + "\*") | where {$_.Name -like "*Endpoint*"}
$endPointPath = "$env:SystemDrive\MicrosoftProtocolTests\" + $testsuite.Name + "\" + $endpoint.Name
$version = Get-ChildItem $endPointPath | where {$_.Attributes -eq "Directory" -and $_.Name -like "1.0.*.*"} | Sort-Object Name -descending | Select-Object -first 1
$binPath = "$endPointPath\$version\bin"

#----------------------------------------------------------------------------
# Define Common Functions
#----------------------------------------------------------------------------
Function ModifyPtfConfigFile($pftConfigFile)
{
	if (!(Test-Path -Path $pftConfigFile))
	{
    	Write-Host "PTF configure file doesn't exist." -ForegroundColor Yellow
		return
	}
	[xml]$ptfconfigContent = Get-Content $pftConfigFile
	$ptfProperties = $ptfconfigContent.TestSite.Properties.Property

	Push-Location "$testDirInVM\Scripts"
	foreach($parameter in $parameters)
	{    
		$paramObj = $ptfProperties | where {$_.name -eq $parameter.Name}
		if($paramObj -ne $null -and $paramObj -ne "")
		{
	    	.\Modify-ConfigFileNode.ps1 $pftConfigFile $parameter.Name $parameter.Value
		}
	}
	Pop-Location
}


#----------------------------------------------------------------------------
# Modify ptfConfig file
#----------------------------------------------------------------------------
[xml]$paramConfigFileContent = Get-Content $paramConfigFile
$parameters = $paramconfigFilecontent.lab.Parameters.Parameter
$ptfFiles = Get-ChildItem -Path "$binPath\*.deployment.ptfconfig"
foreach($ptf in $ptfFiles)
{
    ModifyPtfConfigFile $ptf.FullName
}

	
#-----------------------------------------------------
# Finished to config driver computer
#-----------------------------------------------------
Write-Host "Write signal file: config.finished.signal to system drive."
CMD /C ECHO "CONFIG FINISHED" > "$testDirInVM\config.finished.signal"

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
Stop-Transcript
exit 0