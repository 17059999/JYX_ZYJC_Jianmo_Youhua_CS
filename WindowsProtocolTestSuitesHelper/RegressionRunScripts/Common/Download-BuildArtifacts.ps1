# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Download-BuildArtifacts.ps1
# Purpose:        According to ArtifactName to call Download-BuildArtifacts.ps1 to download build artifacts.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:           The access token
# $ApiUrl:                The API url
# $ArtifactName:          The artifact name
# $DownloadPath:          The download path
#----------------------------------------------------------------------------

Param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$ArtifactName,
    [string]$DownloadPath
)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Import-Module .\DevOpsLib.psm1

Function Main {
    $currentBuildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $env:BUILD_BUILDID
    if ($currentBuildInfo.reason -eq 'buildCompletion') {
        $sourceBuildId = $currentBuildInfo.triggeredByBuild.id
    }
    else {
        $sourceBuildId = $env:SPECIFIC_BUILDID
    }

    if ([System.Uri]::IsWellFormedUriString($env:REMOTE_APIURL, [System.UriKind]::Absolute)) {
        $ApiUrl = $env:REMOTE_APIURL
        $AccessToken = $env:REMOTE_ACCESSTOKEN
    }

    $maxRetries = 10
    $retryDelaySeconds = 60  # 1 minute
    $attempt = 1

    while ($attempt -le $maxRetries) {
        try {
            Write-Host "Attempt $attempt of $maxRetries..."
        
            GetArtifactsByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $sourceBuildId -ArtifactName $ArtifactName -DownloadPath $DownloadPath
        
            Write-Host "SUCCESS: Artifacts downloaded successfully on attempt $attempt" -ForegroundColor Green
            exit 0
        } catch {
            Write-Host "FAILED: Attempt $attempt failed with error: $($_.Exception.Message)" -ForegroundColor Red
        
            if ($attempt -eq $maxRetries) {
                Write-Host "FATAL: All $maxRetries attempts failed. Exiting with error." -ForegroundColor Red
                Write-Host "Last error: $($_.Exception.Message)" -ForegroundColor Red
                exit 1
            }
        
            Write-Host "Waiting $retryDelaySeconds seconds before retry..." -ForegroundColor Yellow
            Start-Sleep -Seconds $retryDelaySeconds
        
            $attempt++
        }
    }

}

Main
