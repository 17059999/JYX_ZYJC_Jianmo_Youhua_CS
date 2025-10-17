# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Wait-SinglePipelineCompletionByBuildId.ps1
# Purpose:        Wait for the completion of a pipline build by the build ID.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:                 The access token
# $ApiUrl:                      The API url
# $BuildId:                     The build ID of the pipeline build to be waited
# $TimeoutInMinutes:            The timeout in minutes for waiting the build completion, 0 indicates infinite timeout
#----------------------------------------------------------------------------

Param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$BuildId,
    [int]$TimeoutInMinutes = 60
)

Add-Type -AssemblyName System.Net.Http
Add-Type -AssemblyName System.IO.Compression.FileSystem

$tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
$authString = [System.Convert]::ToBase64String($tokenBytes)
$authHeaders = @{ "Authorization" = "Basic $authString" }

if ($BuildId -le 0) {
    throw "The build ID: $BuildId is invalid..."
}

$infoUrl = "$ApiUrl/build/builds/$($BuildId)?api-version=5.0"  

$count = $TimeoutInMinutes
$waitUntilComplete = $TimeoutInMinutes -eq 0
$buildInfo = $null
while (($count -gt 0) -or ($waitUntilComplete -eq $true)) {
    Start-Sleep -Seconds 60
    
    try {
        $buildInfo = Invoke-RestMethod -Uri $infoUrl -Method Get -Headers $authHeaders
    }
    catch [System.InvalidOperationException] {
        continue
    }
    
    if ($buildInfo.status -eq "completed") {
        break
    }
    else {
        $count -= 1
    }
}

$buildInfo = Invoke-RestMethod -Uri $infoUrl -Method Get -Headers $authHeaders

# We have some partiallySucceeded results for now (2023/11/03). Revert to '$buildInfo.result -ne "succeeded")' when resolved
if (($buildInfo.status -ne "completed") -or ($buildInfo.result -notmatch "succeeded")) {
    throw "Build failed or timed out..."
}