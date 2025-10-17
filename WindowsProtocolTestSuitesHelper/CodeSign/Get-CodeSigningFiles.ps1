# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Get-CodeSigningFiles.ps1
# Purpose:        Get fles to be singed in the ESRP CodeSigning task.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $ExpandedArchivePath:           The path to the expanded archive
# $ProductName:                   The value of ProductName property of files to be signed
# $CodeSigningFilesPath:          The path to the JSON file output with the list of files to be signed
#----------------------------------------------------------------------------

param(
    [string]$ExpandedArchivePath,
    [string]$ProductName = "",
    [string]$CodeSigningFilesPath
) 

$includedExtensions = @(".dll", ".exe", ".ps1", ".psm1")
$files = Get-ChildItem $ExpandedArchivePath -Recurse | Where-Object { $_.Extension -in $includedExtensions }
$codeSigningFiles = $files | Where-Object {
    if (($_.Extension -eq ".dll") -or ($_.Extension -eq ".exe")) {
        if ($ProductName -ne "") {
            $item = Get-Item $_.FullName
            $item.VersionInfo.ProductName -eq $ProductName
        }
        else {
            $true
        }
    }
    else {
        $true
    }
} | ForEach-Object {
    $_.Name
}

$codeSigningFiles | ConvertTo-Json | Out-File $CodeSigningFilesPath -Encoding utf8
$codeSigningFilesString = [string]::Join(",", $codeSigningFiles)

Write-Host "##vso[task.setvariable variable=codeSign.codeSigningFiles]$codeSigningFilesString"
