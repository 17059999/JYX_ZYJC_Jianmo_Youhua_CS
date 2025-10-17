# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           WaitPipeline.ps1
# Purpose:        According to PipelineId to call WaitPipeline.ps1 to check jobs status.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:           The access token
# $ApiUrl:                The API url
# $PipelineId:            The Pipeline Id
# $BuildId:               The Build Id
#----------------------------------------------------------------------------

param
(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$PipelineId,
    [string]$BuildId
)
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Import-Module .\DevOpsLib.psm1

$RunningCount = 0
do {
        $RunningCount = Get-RunningPreBuildCount -ApiUrl $ApiUrl -AccessToken $AccessToken -PipelineId $PipelineId -BuildId $BuildId
        if($RunningCount -gt 1) {
            Write-Host "Start waiting jobs complete."
           Start-Sleep -Seconds 60
        }
} until($RunningCount -eq 1)