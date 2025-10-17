#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################
param (
    [string]$testExec = ""
)

[string]$vsPath = (Get-ItemProperty "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Microsoft\VisualStudio\SxS\VS7" -Name "15.0")."15.0"
if([string]::IsNullOrEmpty($vsPath)){
    $vsPath = (Get-ItemProperty "Registry::HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\VisualStudio\SxS\VS7" -Name "15.0")."15.0"
    if([string]::IsNullOrEmpty($vsPath)){
        throw "Visual Studio 2017 is not installed."
    }
}

[string]$testPath = ""
if ($testExec -eq "mstest") {
    $testPath = ($vsPath + "Common7\IDE\MSTest.exe")
} else {
    if ($testExec -eq "vstest") {
        $testPath = ($vsPath + "Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe")
    } else {
        throw "Unknown test executable: $testExec"
    }
}

if ([string]::IsNullOrEmpty($testPath)) {
	throw "Cannot find $testExec in given path: $testPath"
}

$ifFileExist = Test-Path $testPath
if($ifFileExist -eq $null -or $ifFileExist -eq $false) {
    throw "File $testPath does not exist"
}

return $testPath
