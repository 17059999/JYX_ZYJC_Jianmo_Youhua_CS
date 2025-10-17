#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

#############################################################################
##
## Microsoft Windows PowerShell Scripting
## File:       UpdatePTFConfig.ps1
## Purpose:    Update configure value from protocol.xml.
## Requirements:   Windows PowerShell 2.0
## Supported OS:   Windows 7 or later versions, Linux
##
##############################################################################

$protocolConfigFile = "c:\temp\Protocol.xml"
$ScriptsSignalFile = "$env:HOMEDRIVE\updateptfconfig.finished.signal"

if(-not(Test-Path -Path $protocolConfigFile)) {
    $protocolConfigFile = "~/Temp/Protocol.xml"
    $ScriptsSignalFile = "~/updateptfconfig.finished.signal"
}

if (Test-Path -Path $ScriptsSignalFile)
{
    Write-Host "The script execution is complete." -foregroundcolor Red
    exit 0
}

$scriptsPath = Split-Path $MyInvocation.MyCommand.Definition

Write-Host "Put current dir as $scriptsPath."
Push-Location $scriptsPath

[XML]$vmConfig = Get-Content $protocolConfigFile
$driverComputerSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "DriverComputer"}
$sutSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "SUT"}
$coreSettings = $vmConfig.lab.core

Function ConfigPTFConfig
{
    $ServerName 			= $sutSettings.name
    $serverIp               = $sutSettings.ip
    $ServerUserName    		= $coreSettings.username
	$ServerUserPassword    	= $coreSettings.password
	$ClientName    			= $driverComputerSettings.name
	$RDPVersion    			= "10.8"	##default value
	
	Write-Host "`$ServerName     			= $ServerName"
	Write-Host "`$ServerUserName     		= $ServerUserName"
	Write-Host "`$ServerUserPassword     	= $ServerUserPassword"
	Write-Host "`$ClientName     			= $ClientName"
	
    if($driverComputerSettings.os -ne "Linux") {
        Write-Host "Trying to connect to computer $DriverIPAddress" -ForegroundColor Yellow
        Set-Item WSMan:\localhost\Client\TrustedHosts -Value * -Force
        $RDPVersion = & "$env:HOMEDRIVE\temp\Get-RDPVersionFromWindowsSUT.ps1" -ComputerName $serverIp -UserName $ServerUserName -UserPassword $ServerUserPassword
    }
	
    Write-Host "`$RDPVersion     			= $RDPVersion"

    # Update deployment.ptfconfig
    
    $endPointPath = $driverComputerSettings.tools.TestsuiteZip.targetFolder
    $DepPtfConfigFolder = "$endPointPath/Bin"
    $DepPtfConfig = "$DepPtfConfigFolder/RDP_ServerTestSuite.deployment.ptfconfig"
    write-host $DepPtfConfig

    Write-Host "Start to configure deployment.ptfconfig" -ForegroundColor Yellow
    ./TurnOff-FileReadonly.ps1 $DepPtfConfig

    ./Modify-ConfigFileNode.ps1 -sourceFileName $DepPtfConfig -nodeName "RDP.ServerName" -newContent $ServerName
    ./Modify-ConfigFileNode.ps1 -sourceFileName $DepPtfConfig -nodeName "RDP.ServerUserName" -newContent $ServerUserName
    ./Modify-ConfigFileNode.ps1 -sourceFileName $DepPtfConfig -nodeName "RDP.ServerUserPassword" -newContent $ServerUserPassword
    ./Modify-ConfigFileNode.ps1 -sourceFileName $DepPtfConfig -nodeName "RDP.ClientName" -newContent $ClientName
    ./Modify-ConfigFileNode.ps1 -sourceFileName $DepPtfConfig -nodeName "RDP.Version" -newContent $RDPVersion
}

ConfigPTFConfig