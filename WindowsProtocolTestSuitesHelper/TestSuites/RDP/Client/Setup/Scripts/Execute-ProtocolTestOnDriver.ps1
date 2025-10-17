# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Execute-ProtocolTestOnDriver.ps1
# Purpose:        Execut the Test Case in Windows or Linux Driver.
# Requirements:   Powershell core
# Supported OS:   Windows, Linux
#
##############################################################################
param(
    [string]$BatchToRun            ="RunAllTestCases.ps1",
    [string]$SupportRDPFile        = "No",
    [string]$FilterToRun           = ""
)

function AddPrefixToTrxResultFile($prefix)
{
    $trxResultFile = Get-ChildItem "$endPointPath/TestResults/*.trx" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if($trxResultFile -ne $null)
    {
        $newName = $prefix + $trxResultFile.Name
        Write-Host "Rename result file to $newName"
        Rename-Item -Path $trxResultFile -NewName $newName
    }
}

#---------------------------------------------------
# Initialize variables
#---------------------------------------------------
Start-Transcript -Path "./Execute-ProtocolTestOnDriver.ps1.log" -Append -Force -ErrorAction SilentlyContinue
Write-Host "SupportRDPFile in Execute-ProtocolTestOnDriver.ps1: $SupportRDPFile..." 

#----------------------------------------------------------------------------
# Get content from protocol config file
#----------------------------------------------------------------------------
$protocolConfigFile = "$env:SystemDrive/Temp/Protocol.xml"
if(-not (Test-Path -Path $protocolConfigFile)) {
    $protocolConfigFile ="~/Temp/Protocol.xml"
}
[xml]$config = Get-Content "$protocolConfigFile"
if($config -eq $null)
{
    ."$PSScriptRoot/Write-Error.ps1" "protocolConfigFile $protocolConfigFile is not a valid XML file."
    exit 1
}

Write-Host "Initialize variables"
$driver = $config.lab.servers.vm | where {$_.role -eq "DriverComputer"}
$sut = $config.lab.servers.vm | where {$_.role -eq "SUT"}
$agentRemoteClient = $sut.agentRemoteClient
$endPointPath = $driver.tools.TestsuiteZip.targetFolder
$BatchDir = "$endPointPath/Batch"

$BatchFilePath = "$BatchDir/$BatchToRun"
$PtfConfig = "$endPointPath/Bin/RDP_ClientTestSuite.deployment.ptfconfig"

Push-Location -Path $BatchDir

#---------------------------------------------------
# Clean up test.finished.signal
#---------------------------------------------------
Write-Host "Clean up test.finished.signal"
$finishSignalFile = "$endPointPath/TestResults/test.finished.signal"
if(Test-Path $finishSignalFile)
{
    Remove-Item $finishSignalFile
}

$driverOS = $driver.os

#---------------------------------------------------
# Modify the ptfconfig file and execute Test Suite
#---------------------------------------------------
Write-Host "Run test cases ..."

if($agentRemoteClient -eq "xfreerdp")
{
    if($FilterToRun -ne "") 
    {
        $FilterToRun +="&"
    }

    # xfreerdp doesn't support SoftSync and UDP.
    $FilterToRun +="Name!=S3_Tunneling_StaticVirtualChannel_PositiveTest"
    # xfreerdp does not support RDSTLS.
    $FilterToRun +="&Name!=BVT_RDSTLSAuthentication_PositiveTest_ServerRedirectionWithPasswordCredentials"
    $FilterToRun +="&Name!=S11_RDSTLSAuthentication_PositiveTest_ServerRedirectionAndAutoReconnectWithCookie"
}

# Use mstsc command instead of rdp file.
if($agentRemoteClient -eq "mstsc" -and ($SupportRDPFile -eq "No"))
{
    if($FilterToRun -ne "") 
    {
        $FilterToRun +="&"
    }

    # The mstsc command without rdp file does not support RDSTLS with redirection setting.
    $FilterToRun +="Name!=BVT_RDSTLSAuthentication_PositiveTest_ServerRedirectionWithPasswordCredentials"
    $FilterToRun +="&Name!=S11_RDSTLSAuthentication_PositiveTest_ServerRedirectionAndAutoReconnectWithCookie"
}

."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Protocol" -newContent "RDP"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Negotiation" -newContent "True"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Level" -newContent "Low"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Method" -newContent "128bit"
& $BatchFilePath $FilterToRun
AddPrefixToTrxResultFile "RDPClient_RDP_"

."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Protocol" -newContent "TLS"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Negotiation" -newContent "True"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Level" -newContent "None"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Method" -newContent "None"
& $BatchFilePath $FilterToRun
AddPrefixToTrxResultFile "RDPClient_TLS_"

if($SupportRDPFile -eq "Yes")
{
    # CredSsp 
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Protocol" -newContent "CredSSP"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Negotiation" -newContent "True"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Level" -newContent "None"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Method" -newContent "None"
    & $BatchFilePath $FilterToRun
    AddPrefixToTrxResultFile "RDPClient_CredSSP_"

    # DirectCredSsp
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Protocol" -newContent "CredSSP"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Negotiation" -newContent "False"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Level" -newContent "None"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "Method" -newContent "None"
    & $BatchFilePath $FilterToRun
    AddPrefixToTrxResultFile "RDPClient_DirectCredSSP_"
}

#---------------------------------------------------
# Write test finish signal file
#---------------------------------------------------
Write-Host "Write test finish signal file"
echo "test.finished.signal" > $finishSignalFile

Stop-Transcript -ErrorAction SilentlyContinue
