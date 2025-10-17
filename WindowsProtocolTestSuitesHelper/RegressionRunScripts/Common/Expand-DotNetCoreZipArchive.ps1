# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Expand-DotNetCoreZipArchive.ps1
# Purpose:        Expand zip archive to a local path according to archive name and archive extension provided.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $ArchiveName:           The name of the archive to be expanded
# $ArchiveExtension:      The file extension of the archive to be expanded
# $ArtifactLocalPath:     The local path to the expanded files
#----------------------------------------------------------------------------

Param(
    [string]$ArchiveName,
    [string]$ArchiveExtension = ".zip",
    [string]$ArtifactLocalPath
)

Add-Type -AssemblyName System.IO.Compression.FileSystem

$localFilePath = "$ArtifactLocalPath\$ArchiveName"
$localFileName = "$localFilePath.zip"

if (-not (Test-Path $localFileName)) {
    Rename-Item "$localFilePath$ArchiveExtension" -NewName $localFileName
}

[System.IO.Compression.ZipFile]::ExtractToDirectory($localFileName, $localFilePath)
Remove-Item $localFileName -Force