# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Queue-MultiplePipelinesWithParameters.ps1
# Purpose:        Queue multiple pipeline builds with specific parameters provided.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:                 The access token
# $ApiUrl:                      The API url
# $RemoteAccessToken:           The access token of a remote project
# $RemoteApiUrl:                The API url of a remote project
# $PipelineNames:               The pipeline names, separated by semicolons
# $TargetRepoBranch:            The branch name for finding pipeline definitions
# $PipelineParameters:          The parameters used to queue pipeline builds
# $ReportInfo:                  Report information about the script execution or not
#----------------------------------------------------------------------------

param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$RemoteAccessToken = "",
    [string]$RemoteApiUrl = "",
    [string]$PipelineNames,
    [string]$TargetRepoBranch = "",
    [string]$PipelineParameters = "",
    [string]$Reason = "",
    [bool]$ReportInfo = $true
) 

$Global:buildPageLinks = @()

foreach ($pipelineName in $PipelineNames.Split(';')) {
    Write-Host "Trigger pipeline build: $pipelineName"
    ./Queue-SinglePipelineWithParameters -AccessToken $AccessToken `
        -ApiUrl $ApiUrl `
        -RemoteAccessToken $RemoteAccessToken `
        -RemoteApiUrl $RemoteApiUrl `
        -PipelineName $pipelineName `
        -TargetRepoBranch $TargetRepoBranch `
        -PipelineParameters $PipelineParameters `
        -Reason $Reason `
        -ReportInfo $ReportInfo
}

if ($ReportInfo) {
    Write-Host "Web page links to the queued pipeline builds:"
    foreach ($link in $Global:buildPageLinks) {
        Write-Host "    $link"
    }
}