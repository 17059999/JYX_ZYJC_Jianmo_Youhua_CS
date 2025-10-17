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

function RunOneCaseToTestSUTReady($endPointPath){
    $maxRetryTimes = 20
    $tryTime = 0
    while($tryTime -lt $maxRetryTimes){
        ."$endPointPath/Batch/RunTestCasesByFilter.ps1" "S1_Connection_ConnectionFinalization_PositiveTest"
        Start-sleep 15
        $filenames = Get-ChildItem "$endPointPath/TestResults/*.*"
        foreach($filename in $filenames){
            if($filename.Name.Contains(".trx")){
                $trxFile = $filename
                break
            }
        }

        $result = Select-String '<ResultSummary outcome="Completed">' $trxFile
        Remove-Item $filename -Recurse -Force
        if($result.count -gt 0){
            Write-Host "SUT computer is ready." -foregroundcolor Green
            break
        }

        $tryTime++
        Write-Host "SUT computer is not ready. Try again." -foregroundcolor Red
        Start-sleep 15
    }

    if($tryTime -ge $maxRetryTimes){
        Write-Host "SUT computer is failed to ready." -foregroundcolor Red
    }
}

#---------------------------------------------------
# Initialize variables
#---------------------------------------------------
Start-Transcript -Path "./Execute-ProtocolTestOnDriver.ps1.log" -Append -Force -ErrorAction SilentlyContinue

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
$endPointPath = $driver.tools.TestsuiteZip.targetFolder
$BatchDir = "$endPointPath/Batch"

$BatchFilePath = "$BatchDir/$BatchToRun"
$PtfConfig = "$endPointPath/Bin/RDP_ServerTestSuite.deployment.ptfconfig"

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

# Run a case repeatedly to ensure the windows SUT is ready
if($driverOS -eq "Linux"){
    RunOneCaseToTestSUTReady $endPointPath
}

#---------------------------------------------------
# Modify the ptfconfig file and execute Test Suite
#---------------------------------------------------
Write-Host "Run test cases ..."

."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Negotiation" -newContent "true"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Protocol" -newContent "TLS"
& $BatchFilePath $FilterToRun
AddPrefixToTrxResultFile "RDPServer_TLS_"

."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Negotiation" -newContent "false"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Protocol" -newContent "RDP"
& $BatchFilePath $FilterToRun
AddPrefixToTrxResultFile "RDPServer_RDP_"


# NegotiationCredSsp 
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Negotiation" -newContent "true"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Protocol" -newContent "CredSSP"
& $BatchFilePath $FilterToRun
AddPrefixToTrxResultFile "RDPServer_NegotiationCredSsp_"

# DirectCredSsp
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Negotiation" -newContent "false"
."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "RDP.Security.Protocol" -newContent "CredSSP"
& $BatchFilePath $FilterToRun
AddPrefixToTrxResultFile "RDPServer_DirectCredSsp_"


#---------------------------------------------------
# Write test finish signal file
#---------------------------------------------------
Write-Host "Write test finish signal file"
echo "test.finished.signal" > $finishSignalFile

Stop-Transcript -ErrorAction SilentlyContinue
