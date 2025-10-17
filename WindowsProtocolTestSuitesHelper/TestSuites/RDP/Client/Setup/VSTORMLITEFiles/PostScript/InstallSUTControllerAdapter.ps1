# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

$rootPath          = Split-Path $MyInvocation.MyCommand.Definition -parent
$RDPSUTControlPath = "$rootPath\Tools\RDPSUTControlAgent"
$scriptsPath       = "$rootPath\Scripts"
$FreeRDPPath       = "$rootPath\Tools\FreeRDP"
$configPath	 	   = "$rootPath\protocol.xml"

Push-Location $scriptsPath
#----------------------------------------------------------------------------
# Start logging using start-transcript cmdlet
#----------------------------------------------------------------------------
try {
    Stop-Transcript -ErrorAction SilentlyContinue | Out-Null
 }
 catch [System.InvalidOperationException] {}
 Start-Transcript -Path "$rootPath\InstallSUTControllerAdapter.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
$settingFile     = "$scriptsPath\ParamConfig.xml"
$userNameInTC    = .\Get-Parameter.ps1 $settingFile userNameInTC
$workgroupDomain = .\Get-Parameter.ps1 $settingFile workgroupDomain
$domainName      = .\Get-Parameter.ps1 $settingFile domainName

$driverComputerIP = "192.168.173.1"
$pwd = "Password01!"
$sutControlAgentPort = "4488"
$sutControlAgentRemoteClient = "mstsc"

$listeningPort      = .\Get-Parameter.ps1 $settingFile RDPListeningPort
if (Test-Path "$configPath") {
    [xml]$vmConfig = Get-Content $configPath
    $labUserName = $vmConfig.lab.core.username
    $pwd = $vmConfig.lab.core.password
    $driverComputerSettting = $vmConfig.lab.servers.vm | where {$_.role -eq "DriverComputer"}
    $driverComputerIP     = $driverComputerSettting.ip
    if(($driverComputerSettting.ip | Measure-Object ).Count -gt 1){
        $driverComputerIP = $driverComputerSettting.ip[0]
    }

    $sutSettting = $vmConfig.lab.servers.vm | where {$_.role -eq "SUT"}
    if($sutSettting.agentPort)
    {
        $sutControlAgentPort = $sutSettting.agentPort
    }

    if($sutSettting.agentRemoteClient){
        $sutControlAgentRemoteClient = $sutSettting.agentRemoteClient
    }
}

$targetAddress = "${driverComputerIP}:${listeningPort}"
#----------------------------------------------------------------------------
# Create Windows tasks for Remote SUT control adapter
#----------------------------------------------------------------------------
if($labUserName -ne $null)
{
    $taskUser = $labUserName
}
else
{
    $taskUser= $env:UserName
}

if ($workgroupDomain.ToUpper() -eq "DOMAIN") {
    $taskUser = "$domainName\$taskUser"
}

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$OLDPATH = [System.Environment]::GetEnvironmentVariable('PATH','machine')
$NEWPATH = "$OLDPATH;$FreeRDPPath"
[Environment]::SetEnvironmentVariable("PATH", "$NEWPATH", "Machine")

if(Test-Path "$RDPSUTControlPath\CSharp") {
    Write-Host "Creating task to trigger client to initiate the SUT Control Agent(CSharp)..."

    ((Get-Content -path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config -Raw) -replace '{{remote_client}}', $sutControlAgentRemoteClient) | Set-Content -Path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config
    ((Get-Content -path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config -Raw) -replace 'PUT_THE_USERNAME_HERE', $taskUser) | Set-Content -Path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config
    ((Get-Content -path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config -Raw) -replace 'PUT_THE_PASSWORD_HERE', $pwd) | Set-Content -Path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config
    ((Get-Content -path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config -Raw) -replace '{{address}}:{{port}}', $targetAddress) | Set-Content -Path $RDPSUTControlPath\CSharp\RDPSUTControlAgent.dll.config

    cmd /c schtasks /Create /RU $taskUser /SC ONSTART /TN CSharpAgent /TR "$RDPSUTControlPath\CSharp\RDPSUTControlAgent.exe /p:$sutControlAgentPort" /IT /F
}
if(Test-Path "$RDPSUTControlPath\Java") {
    Write-Host "Creating task to trigger client to initiate the SUT Control Agent(Java)..."
    ## Expand-Archive is only supported in Powerhshell 5.0 or later
    $ZipFile = "$RDPSUTControlPath\Java\distributions\RDPSUTControlAgent.zip"
    $AgentFolder = "$RDPSUTControlPath\Java\"
    if ($psversiontable.PSVersion.Major -ge 5) {
        Write-Host "Extract Java Agent files"
        Expand-Archive $ZipFile $AgentFolder
    } else {
        $shell = New-Object -com shell.application
        $zip = $shell.NameSpace($ZipFile)
        if(!(Test-Path -Path $AgentFolder))
        {
            New-Item -ItemType directory -Path $AgentFolder
        }
        $shell.Namespace($AgentFolder).CopyHere($zip.items(), 0x14)
    }
    ## Replace config file    
    attrib -R $RDPSUTControlPath\Java\freerdp.config
    ((Get-Content -path $RDPSUTControlPath\Java\freerdp.config -Raw) -replace 'PUT_THE_USERNAME_HERE', $taskUser) | Set-Content -Path $RDPSUTControlPath\Java\freerdp.config
    ((Get-Content -path $RDPSUTControlPath\Java\freerdp.config -Raw) -replace 'PUT_THE_PASSWORD_HERE', $pwd) | Set-Content -Path $RDPSUTControlPath\Java\freerdp.config
    ((Get-Content -path $RDPSUTControlPath\Java\freerdp.config -Raw) -replace '{{address}}:{{port}}', $targetAddress) | Set-Content -Path $RDPSUTControlPath\Java\freerdp.config
    cmd /c schtasks /Create /RU $taskUser /SC ONSTART /TN JavaAgent /TR "$RDPSUTControlPath\Java\RDPSUTControlAgent\bin\RDPSUTControlAgent.bat -c $RDPSUTControlPath\Java\freerdp.config -p $sutControlAgentPort" /IT /F
}
if(Test-Path "$RDPSUTControlPath\Python") {
    ## Replace config file    
    attrib -R $RDPSUTControlPath\Python\settings.ini
    ((Get-Content -path $RDPSUTControlPath\Python\settings.ini -Raw) -replace 'PUT_THE_USERNAME_HERE', $taskUser) | Set-Content -Path $RDPSUTControlPath\Python\settings.ini
    ((Get-Content -path $RDPSUTControlPath\Python\settings.ini -Raw) -replace 'PUT_THE_PASSWORD_HERE', $pwd) | Set-Content -Path $RDPSUTControlPath\Python\settings.ini
    ((Get-Content -path $RDPSUTControlPath\Python\settings.ini -Raw) -replace '{{ address }}', $targetAddress) | Set-Content -Path $RDPSUTControlPath\Python\settings.ini
    ((Get-Content -path $RDPSUTControlPath\Python\settings.ini -Raw) -replace '4488', $sutControlAgentPort) | Set-Content -Path $RDPSUTControlPath\Python\settings.ini

    Write-Host "Creating task to trigger client to initiate the SUT Control Agent(Python)..."
    cmd /c schtasks /Create /RU $taskUser /SC ONSTART /TN PythonAgent /TR "cmd /c cd $RDPSUTControlPath\Python && python RDPSUTControlAgent.py" /IT /F
}

#----------------------------------------------------------------------------
# Stop logging
#----------------------------------------------------------------------------
Stop-Transcript