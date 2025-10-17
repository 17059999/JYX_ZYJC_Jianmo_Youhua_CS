#############################################################################
## Copyright (c) Microsoft. All rights reserved.
##
## Microsoft Windows PowerShell Scripting
## File:    CopyTestSuiteToShare.ps1
## Purpose: This script will copy the compiled test suite to a share folder
##
#############################################################################

#----------------------------------------------------------------------------
# Parameters
# $TestSuite:               Test suite to be copied to share
# $EndpointName:            Endpoint name under $TestSuite
# $TestSuiteSubFolder:      If Test suite be checked out to a sub folder, this parameter is the sub folder name 
# $TestSuiteShareFolder:    Test suite release file share folder
# $TargetTestSuiteName:     Target test suite name under $TestSuiteShareFolder
#----------------------------------------------------------------------------
param (
    [Parameter(Mandatory=$True)]
	[string]$TestSuite,
	[Parameter(Mandatory=$False)]
	[string]$EndpointName,
	[Parameter(Mandatory=$False)]
	[string]$TestSuiteSubFolder,
	[Parameter(Mandatory=$True)]
	[string]$TestSuiteShareFolder,
	[Parameter(Mandatory=$False)]
	[string]$TargetTestSuiteName   = $TestSuite
)

$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
$helperRoot = (Get-Item $currentPath).parent.FullName
$testSuiteRoot = (Get-Item $helperRoot).parent.FullName

if($TestSuiteSubFolder -ne "")
{
	$testSuiteRoot = Join-Path $testSuiteRoot $TestSuiteSubFolder
	Write-Host "TestSuite root path: $testSuiteRoot"
}else{
	Write-Host "Use the default value $testSuiteRoot as testSuiteRoot"
}

if($EndpointName -ne "")
{
	Write-Host "Endpoint name is with value $EndpointName."
	$TestSuiteWithEndpoint = "$TestSuite\$EndpointName"
}else{
	Write-Host "Endpoint name is empty."
	$TestSuiteWithEndpoint = "$TestSuite"
}

Write-Host "Search with relative path $TestSuiteWithEndpoint under testSuiteRoot."

$share = "$TestSuiteShareFolder\$TargetTestSuiteName\"

Write-Host "Copy lab scripts to drop folder"
Copy-Item -Recurse -Force "$testSuiteRoot\TestSuites\$TestSuiteWithEndpoint\Setup\*" -Destination "$testSuiteRoot\drop\TestSuites\$TestSuiteWithEndpoint\"

Write-Host "Clean files on $share"
Remove-Item -Recurse "$share\*"

Write-Host "Copy files from $testSuiteRoot\CommonScripts\* to $TestSuiteShareFolder\..\ScriptLib\"
robocopy $testSuiteRoot\CommonScripts\ $TestSuiteShareFolder"\..\ScriptLib\" 

Write-Host "Copy files to $share"
xcopy $testSuiteRoot\drop\TestSuites\$TestSuiteWithEndpoint\* $share /S /y /I
