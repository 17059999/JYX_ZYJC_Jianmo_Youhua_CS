# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Get-DotNetCoreArtifactByBuildId.ps1
# Purpose:        Get the specific files in an artifact of a pipeline build by the build ID.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:                      The access token
# $ApiUrl:                           The API url
# $BuildId:                          The build ID of the upstream pipeline build
# $ArtifactName:                     The name of the requested artifact
# $GetAllFiles:                      Get all files in the artifact or not
# $FileName:                         The file name pattern of requested files
# $FileExtension:                    The file extension pattern of requested files
# $UpstreamFileNameVariableName:     The name of the variable of the file name when there is only 1 file requested
# $ArtifactLocalPath:                The local path to the downloaded files
# $ReportInfo:                       Report information about the script execution or not
#----------------------------------------------------------------------------

Param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$BuildId,
    [string]$ArtifactName = "drop",
    [bool]$GetAllFiles = $false,
    [string]$FileName = "",
    [string]$FileExtension = ".zip",
    [string]$UpstreamFileNameVariableName = "",
    [string]$ArtifactLocalPath,
    [bool]$ReportInfo = $true
)

Add-Type -AssemblyName System.Net.Http
Add-Type -AssemblyName System.IO.Compression.FileSystem

$tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
$authString = [System.Convert]::ToBase64String($tokenBytes)
$authHeaders = @{ "Authorization" = "Basic $authString" }

$artifactUrl = "$ApiUrl/build/builds/$($BuildId)/artifacts?artifactName=$ArtifactName&api-version=5.0"
$artifactInfo = Invoke-RestMethod -Uri $artifactUrl -Method Get -Headers $authHeaders
$downloadUrl = $artifactInfo.resource.downloadUrl

if (-not (Test-Path $ArtifactLocalPath)) {
    New-Item -ItemType Directory $ArtifactLocalPath
}
    
$localFilePath = "$ArtifactLocalPath/$BuildId"
$localFileName = "$localFilePath.zip"

if ($ReportInfo) {
    Write-Host "The artifacts download url is: $downloadUrl"
}

Write-Host "Retry to wait until the artifacts can be downloaded..."
    
$count = 60
$fileStream = [System.IO.File]::Create($localFileName)
while ($count -gt 0) {
    try {
        Start-Sleep -Seconds 5
        $httpClient = New-Object System.Net.Http.HttpClient
        $httpClient.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Basic", $authString)
            
        $stream = $httpClient.GetStreamAsync($downloadUrl).GetAwaiter().GetResult()
        $fileStream.Seek(0, [System.IO.SeekOrigin]::Begin)
        $stream.CopyTo($fileStream)
            
        $stream.Close()
        $httpClient.Dispose()
        break
    }
    catch {
        $message = $_.Exception.Message

        if ($ReportInfo) {
            Write-Host "There is an exception thrown with the following message: $message"
        }

        $count -= 1
        if ($count -eq 0) {
            $fileStream.Close()
            throw $_
        }
    }
}
$fileStream.Close()

[System.IO.Compression.ZipFile]::ExtractToDirectory($localFileName, $localFilePath)
Remove-Item $localFileName -Force
    
$artifactFilesPath = "$localFilePath/$ArtifactName"
[array]$files = Get-ChildItem -R $artifactFilesPath | Where-Object {
    if ($GetAllFiles) {
        $true
    }
    else {
        $_.Name -match "$FileName$FileExtension"
    }
}

if ($files.Length -eq 0) {
     Write-Host "The requested file not existed."
}

if ($files.Length -eq 1) {
    if ($UpstreamFileNameVariableName -ne "") {
        $upstreamFileName = $files[0].Name.Substring(0, $files[0].Name.Length - $FileExtension.Length)
        Write-Host "##vso[task.setvariable variable=$UpstreamFileNameVariableName]$upstreamFileName"
    }
}

$files | Foreach-Object {
    $fullPath = $_.FullName
    Write-Host "The source file is located at: $fullPath"
    
    try {
        $newPath = "$ArtifactLocalPath/$($_.Name)"
        Move-Item -Path $fullPath -Destination $newPath -Force
    } catch {
        # Handle the error or ignore it
        Write-Host "File does not exist in path as it might have been moved."
    }
          
    Write-Host "The requested file is located at: $newPath"
}
#$files | Move-Item -Destination $ArtifactLocalPath
Remove-Item $localFilePath -Recurse -Force