#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

#############################################################################
##
## Microsoft Windows PowerShell Scripting
## File:       UpdateParamConfig.ps1
## Purpose:    Update configure value from protocol.xml.
## Requirements:   Windows PowerShell 2.0
## Supported OS:   Windows 7 or later versions, Linux
##
##############################################################################

param(
    [string]$SupportCompression    = "No",
    [string]$ComputerName          = ""
)

$protocolConfigFile = "c:\temp\Protocol.xml"
$scriptsSignalFile = "$env:HOMEDRIVE\updateParamConfig.finished.signal"

if(-not(Test-Path -Path $protocolConfigFile)) {
    $protocolConfigFile = "~/Temp/Protocol.xml"
    $scriptsSignalFile = "~/updateParamConfig.finished.signal"
}

if (Test-Path -Path $ScriptsSignalFile)
{
    Write-Host "The script execution is complete." -foregroundcolor Red
    exit 0
}

$scriptsPath = Split-Path $MyInvocation.MyCommand.Definition

Write-Host "Put current dir as $scriptsPath."
Push-Location "$scriptsPath/../"

[XML]$vmConfig = Get-Content $protocolConfigFile
$driverComputerSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "DriverComputer"}
$sutSettings = $vmConfig.lab.servers.vm | where {$_.role -eq "SUT"}
$coreSettings = $vmConfig.lab.core



Function ConfigPTFConfig
{
    $sutComputerName 		= $sutSettings.name
    $sutIp                  = $sutSettings.ip
    $driverComputerName     = $driverComputerSettings.name
    $driverIp               = $driverComputerSettings.ip
    $userNameInVM    		= $coreSettings.username
	$userPwdInVM        	= $coreSettings.password

    $agentPort = "4488"
    if($sutSettings.SelectSingleNode("agentPort")) {
        $agentPort = $sutSettings.agentPort
    }
    if($sutSettings.SelectSingleNode("agentRemoteClient")) {
        $agentRemoteClient = $sutSettings.agentRemoteClient
    }

    $RDPListeningPort = "3389"
    if($driverComputerSettings.SelectSingleNode("RDPListeningPort")) {
        $RDPListeningPort = $driverComputerSettings.RDPListeningPort
    }

    $workgroupDomain       = "Workgroup"
	$RDPVersion    		   = "10.8"	##default value
    $IPVersion             = "IPv4"
	$sutOS             	   = "Windows" #SUT
    $sutOStype = $sutSettings.get_ChildNodes() | where {$_.Name -eq "os"}	
    if ($sutOStype -ne $null ) {	
        if($sutSettings.os -eq "Linux") {	
            $sutOS             = "NonWindows"  		 	
        }	
        elseif ($sutSettings.os -eq "Windows") {	
            $sutOS             	   = "Windows"	
        }	
    }
    if($driverComputerSettings.os -eq "Linux") {
        $sutComputerName = $sutIp
        $driverComputerName = $driverIp
    }
	Write-Host "`$sutComputerName     		= $sutComputerName"
	Write-Host "`$sutIp     		        = $sutIp"
	Write-Host "`$driverComputerName     	= $driverComputerName"
    Write-Host "`$driverIp     		        = $driverIp"
	Write-Host "`$userNameInVM     			= $userNameInVM"
    Write-Host "`$userPwdInVM     			= $userPwdInVM"
	Write-Host "`$ComputerName     			= $ComputerName"
 
    # Update deployment.ptfconfig
    $currentSettings = $vmConfig.lab.servers.vm | where {$_.name -eq $ComputerName}
    if($driverComputerSettings.os -ne "Linux") {
        Write-Host "Trying to connect to computer $sutIp" -ForegroundColor Yellow
        Set-Item WSMan:\localhost\Client\TrustedHosts -Value * -Force
        # Detect the RDP version of SUT
        if($ComputerName -eq $sutComputerName)
        {
            $RDPVersion = & "$env:HOMEDRIVE\temp\Get-RDPVersionFromWindowsSUT.ps1"
        }else{
            $RDPVersion = & "$env:HOMEDRIVE\temp\Get-RDPVersionFromWindowsSUT.ps1" -ComputerName $sutIp -UserName "$sutComputerName\$userNameInVM" -UserPassword $userPwdInVM
        }
        
    }
	
    Write-Host "`$RDPVersion     			= $RDPVersion"
    $endPointPath = $currentSettings.tools.TestsuiteZip.targetFolder
    $DepParamConfigFolder = "$endPointPath/Scripts"
    $DepParamConfig = "$DepParamConfigFolder/ParamConfig.xml"
    write-host "update $DepParamConfig on VMs ..."
    if($currentSettings.os -ne "Linux") {
        ./TurnOff-FileReadonly.ps1 $DepParamConfig
    }
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "workgroupDomain" -newContent $workgroupDomain
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "userNameInTC" -newContent $userNameInVM
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "userPwdInTC" -newContent $userPwdInVM
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "tcComputerName" -newContent $sutComputerName
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "driverComputerName" -newContent $driverComputerName
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "ipVersion" -newContent $IPVersion
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "osVersion" -newContent $sutOS
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "CredSSPUser" -newContent $userNameInVM
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "CredSSPPwd" -newContent $userPwdInVM
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "RDPVersion" -newContent $RDPVersion
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "RDPListeningPort" -newContent $RDPListeningPort
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "compressionInTC" -newContent $SupportCompression
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "agentPort" -newContent $agentPort
    ./Modify-ParamConfigFileNode.ps1 -sourceFileName $DepParamConfig -attrName "agentRemoteClient" -newContent $agentRemoteClient

    #-----------------------------------------------------
    # Finished to update Param Config
    #-----------------------------------------------------
    Pop-Location
    if($currentSettings.os -ne "Linux") {
        Write-Host "Write signal file: $scriptsSignalFile to system drive."
        cmd /C ECHO CONFIG FINISHED>$scriptsSignalFile
    }
}

ConfigPTFConfig