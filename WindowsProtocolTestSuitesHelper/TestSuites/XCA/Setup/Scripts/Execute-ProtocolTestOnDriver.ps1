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
$PtfConfig = "$endPointPath/Bin/MS-XCA_TestSuite.deployment.ptfconfig"

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

if($driverOS -eq "Linux") {
    $exeDir = Resolve-Path "$endPointPath/Utils/XcaTestApp"
    #."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "WorkingDirectory" -newContent "$exeDir"
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "PLAIN_LZ77_COMPRESSION_COMMAND" -newContent "dotnet"
    ."$PSScriptRoot/Prefix-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "PLAIN_LZ77_COMPRESSION_ARGUMENTS" -prefix "$exeDir/XcaTestApp.dll "
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZ77_HUFFMAN_COMPRESSION_COMMAND" -newContent "dotnet"
    ."$PSScriptRoot/Prefix-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZ77_HUFFMAN_COMPRESSION_ARGUMENTS" -prefix "$exeDir/XcaTestApp.dll "
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZNT1_COMPRESSION_COMMAND" -newContent "dotnet"
    ."$PSScriptRoot/Prefix-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZNT1_COMPRESSION_ARGUMENTS" -prefix "$exeDir/XcaTestApp.dll "
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "PLAIN_LZ77_DECOMPRESSION_COMMAND" -newContent "dotnet"
    ."$PSScriptRoot/Prefix-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "PLAIN_LZ77_DECOMPRESSION_ARGUMENTS" -prefix "$exeDir/XcaTestApp.dll "
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZ77_HUFFMAN_DECOMPRESSION_COMMAND" -newContent "dotnet"
    ."$PSScriptRoot/Prefix-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZ77_HUFFMAN_DECOMPRESSION_ARGUMENTS" -prefix "$exeDir/XcaTestApp.dll "
    ."$PSScriptRoot/Modify-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZNT1_DECOMPRESSION_COMMAND" -newContent "dotnet"
    ."$PSScriptRoot/Prefix-ConfigFileNode.ps1" -sourceFileName $PtfConfig -nodeName "LZNT1_DECOMPRESSION_ARGUMENTS" -prefix "$exeDir/XcaTestApp.dll "
}

& $BatchFilePath $FilterToRun
AddPrefixToTrxResultFile "XCA_"

#---------------------------------------------------
# Write test finish signal file
#---------------------------------------------------
Write-Host "Write test finish signal file"
echo "test.finished.signal" > $finishSignalFile

Stop-Transcript -ErrorAction SilentlyContinue
