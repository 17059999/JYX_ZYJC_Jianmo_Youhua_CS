# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Execute-ProtocolTestOnDriver.ps1
# Purpose:        Execute the Test Case in Windows or Linux Driver.
# Requirements:   Powershell core
# Supported OS:   Windows, Linux
#
##############################################################################

function AddPrefixToTrxResultFile($prefix) {
    $trxResultFile = Get-ChildItem "$endPointPath/TestResults/*.trx" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if ($trxResultFile -ne $null) {
        $newName = $prefix + $trxResultFile.Name
        Write-Host "Rename result file to $newName"
        Rename-Item -Path $trxResultFile -NewName $newName
    }
}

#-------------------------
# Global Variables
#-------------------------
$batchToRun = "RunAllTestCases.ps1"

#---------------------------------------------------
# Initialize variables
#---------------------------------------------------
Start-Transcript -Path "./Execute-ProtocolTestOnDriver.ps1.log" -Append -Force -ErrorAction SilentlyContinue

#----------------------------------------------------------------------------
# Get content from protocol config file
#----------------------------------------------------------------------------
$tempPath = "$env:SystemDrive/temp"
$protocolConfigFile = "$tempPath/Protocol.xml"
[xml]$config = Get-Content "$protocolConfigFile"
if ($config -eq $null) {
    ."$tempPath/Write-Error.ps1" "ProtocolConfigFile $protocolConfigFile is not a valid XML file."
    exit 1
}

Write-Host "Initialize variables"
$userNameInVM = $config.lab.core.username
$userPwdInVM = $config.lab.core.password
$server = $config.lab.servers.vm | Where-Object { $_.role -eq "Server" }
$client = $config.lab.servers.vm | Where-Object { $_.role -eq "DriverComputer" }
$endPointPath = $client.tools.TestsuiteZip.targetFolder
$batchDir = "$endPointPath/Batch"

$batchFilePath = "$batchDir/$batchToRun"
$ptfConfig = "$endPointPath/Bin/MS-WSP_ServerTestSuite.deployment.ptfconfig"

Push-Location -Path $batchDir

#---------------------------------------------------
# Clean up test.finished.signal
#---------------------------------------------------
Write-Host "Clean up test.finished.signal"
$finishSignalFile = "$endPointPath/TestResults/test.finished.signal"
if (Test-Path $finishSignalFile) {
    Remove-Item $finishSignalFile
}

#---------------------------------------------------
# Write test.started.signal
#---------------------------------------------------
$startSignalFile = "$endPointPath/test.started.signal"
Set-Content -Path $startSignalFile -Value "test.started.signal" 

#---------------------------------------------------
# Modify the ptfconfig file and execute Test Suite
#---------------------------------------------------
Write-Host "Run test cases ..."

."$tempPath/Modify-ConfigFileNode.ps1" -SourceFileName $ptfConfig -NodeName "ServerComputerName" -NewContent $server.name
."$tempPath/Modify-ConfigFileNode.ps1" -SourceFileName $ptfConfig -NodeName "UserName" -NewContent $userNameInVM
."$tempPath/Modify-ConfigFileNode.ps1" -SourceFileName $ptfConfig -NodeName "Password" -NewContent $userPwdInVM
."$tempPath/Modify-ConfigFileNode.ps1" -SourceFileName $ptfConfig -NodeName "QueryPath" -NewContent "file://$($server.name)/Test/"

& $batchFilePath

AddPrefixToTrxResultFile "MS-WSP_"

#---------------------------------------------------
# Write test finish signal file
#---------------------------------------------------
Write-Host "Write test finish signal file"
Set-Content -Path $finishSignalFile -Value "test.finished.signal"

Stop-Transcript -ErrorAction SilentlyContinue
