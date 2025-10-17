#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           CopyLabScriptToTestSuite.ps1
## Purpose:        Copy CodeSign and script files for Jenkins jobs.
## Requirements:   Windows Powershell 2.0 CTP2
## Supported OS:   Windows 7 and later versions
##
##############################################################################
param(
        [string]$TestSuiteRootPath, # The root folder of all the test suites
        [string]$TestSuitePath # The specific test suite path which you want to copy to, it' the relative path of the TestSuiteRootPath. 
                               # For example, TestSuites\RDP\Client, TestSuites\FileServer
                               # If it's not specified, the scripts and VSTORMLITEFiles from helper repo will be copied to all test suites' folder.
)

$currentDir = split-path -parent $MyInvocation.MyCommand.Definition | select -first 1
Write-Host "LabScript Folder: $currentDir"

if ($TestSuiteRootPath -eq "")
{
    $TestSuiteRootPath = split-path $currentDir    
}
Write-Host "TestSuite Root Folder: $TestSuiteRootPath"

if ($TestSuitePath -eq "")
{
    $TestSuitePath = "TestSuites"
}
Write-Host "TestSuite relative path: $TestSuitePath"

## The dest path should be the parent path of the test suite path.
## E.g. The dest path of RDPClient should be "$TestSuiteRootPath\TestSuites\RDP"
## The dest path of FileServer should be "$TestSuiteRootPath\TestSuites"
$TestSuiteDestPath = Join-Path "$TestSuiterootpath" -ChildPath "$TestSuitePath" | Split-Path -parent 
Write-Host "Dest path of Copy: $TestSuiteDestPath"

Copy-Item -Path "$currentDir\..\..\ProtoSDK" -Dest $TestSuiteRootPath -Recurse -Force
Copy-Item -Path "$currentDir\..\..\$TestSuitePath" -Dest $TestSuiteDestPath -Recurse -Force
