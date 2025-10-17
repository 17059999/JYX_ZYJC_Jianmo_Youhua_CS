# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Queue-SinglePipelineWithParameters.ps1
# Purpose:        Queue a pipeline build with specific parameters provided.
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
# $PipelineName:                The pipeline name
# $TargetRepoBranch:            The branch name for finding pipeline definitions
# $PipelineParameters:          The parameters used to queue pipeline builds
# $BuildIdVariableName:         The name of the variable of the queued pipeline build ID
# $ReportInfo:                  Report information about the script execution or not
#----------------------------------------------------------------------------

param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$RemoteAccessToken = "",
    [string]$RemoteApiUrl = "",
    [string]$PipelineName,
    [string]$TargetRepoBranch = "",
    [string]$PipelineParameters = "",
    [string]$Reason = "",
    [string]$BuildIdVariableName = "",
    [bool]$ReportInfo = $true
) 

$tokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$AccessToken")
$authString = [System.Convert]::ToBase64String($tokenBytes)
$authHeaders = @{ "Authorization" = "Basic $authString" }

if ($RemoteAccessToken -ne "") {
    $remoteTokenBytes = [System.Text.Encoding]::UTF8.GetBytes(":$RemoteAccessToken")
    $remoteAuthString = [System.Convert]::ToBase64String($remoteTokenBytes)
    $remoteAuthHeaders = @{ "Authorization" = "Basic $remoteAuthString" }
}

function Get-PipelineIdByName {
    param(
        [string]$PipelineName
    )

    $definitionsUrl = if ($RemoteApiUrl -ne "") {
        "$RemoteApiUrl/build/definitions?api-version=5.1"
    }
    else {
        "$ApiUrl/build/definitions?api-version=5.1"
    }

    $definitionsAuthHeaders = if ($RemoteAccessToken -ne "") {
        $remoteAuthHeaders
    }
    else {
        $authHeaders
    }

    if ($ReportInfo) {
        Write-Host "The definitions URL is: $definitionsUrl"
    }

    $definitionsInfo = Invoke-RestMethod -Uri $definitionsUrl -Method Get -Headers $definitionsAuthHeaders

    $pipelineId = ($definitionsInfo.value | Where-Object {
            $_.name -eq $PipelineName
        } | Select-Object -Property id).id

    return $pipelineId
}

function Get-CurrentBuildInfo {
    $infoUrl = "$ApiUrl/build/builds/$($env:BUILD_BUILDID)?api-version=5.1"

    Write-Host "Get Current BuildInfo from: $infoUrl"
    $currentBuildInfo = Invoke-RestMethod -Uri $infoUrl -Method Get -Headers $authHeaders

    return $currentBuildInfo
}

function Trigger-Build {
    param (
        [string]$PipelineId
    )

    $triggerUrl = if ($RemoteApiUrl -ne "") {
        "$RemoteApiUrl/build/builds?api-version=5.1"
    }
    else {
        "$ApiUrl/build/builds?api-version=5.1"
    }
    
    $triggerAuthHeaders = if ($RemoteAccessToken -ne "") {
        $remoteAuthHeaders
    }
    else {
        $authHeaders
    }
    
    $selfInfo = Get-CurrentBuildInfo
    
    $parameters = if (($selfInfo.reason -eq "pullRequest") -or ($selfInfo.parameters -match "system.pullRequest")) {
        Write-Host "Strip out PR parameters from build info..."
        Write-Host "selfParameters: $($selfInfo.parameters)"
        $selfParameters = $selfInfo.parameters | ConvertFrom-Json
        $pullRequestParameters = @{
            "system.pullRequest.mergedAt"            = $selfParameters.'system.pullRequest.mergedAt'
            "system.pullRequest.pullRequestId"       = $selfParameters.'system.pullRequest.pullRequestId'
            "system.pullRequest.pullRequestNumber"   = $selfParameters.'system.pullRequest.pullRequestNumber'
            "system.pullRequest.sourceBranch"        = $selfParameters.'system.pullRequest.sourceBranch'
            "system.pullRequest.sourceCommitId"      = $selfParameters.'system.pullRequest.sourceCommitId'
            "system.pullRequest.sourceRepositoryUri" = $selfParameters.'system.pullRequest.sourceRepositoryUri'
            "system.pullRequest.targetBranch"        = $selfParameters.'system.pullRequest.targetBranch'
        }

        $pullRequestParameters = $pullRequestParameters | ConvertTo-Json
        $pullRequestParameters = $pullRequestParameters.Replace('{', '').Replace('}', '')
        if ($PipelineParameters -ne "") { "$pullRequestParameters, $PipelineParameters" } else { "$pullRequestParameters" }
    }
    else {
        $PipelineParameters
    }

    $triggerReason = if ($Reason -eq "") { $selfInfo.reason } else { $Reason }
    $body = @{
        definition   = @{
            id = $PipelineId
        }
        parameters   = "{$parameters}"
        reason       = $triggerReason
        requestedFor = $selfInfo.requestedFor
        sourceBranch = if ($TargetRepoBranch -eq "") { $selfInfo.sourceBranch } else { $TargetRepoBranch }
        triggerInfo  = if ($triggerReason -eq "manual") { @{} } else { $selfInfo.triggerInfo }
    } | ConvertTo-Json -Depth 100

    $buildInfo = Invoke-RestMethod -Uri $triggerUrl -Method Post -Headers $triggerAuthHeaders -Body $body -ContentType "application/json"
    
    if ($ReportInfo) {
        Write-Host "Triggered build info:"
        Write-Host ($buildInfo | ConvertTo-Json)
    }
    
    [int]$buildId = $buildInfo.id
    $buildPageLink = $buildInfo._links.web.href
    $Global:buildPageLinks += $buildPageLink

    if ($ReportInfo) {
        Write-Host "Web page link to the current triggered build:"
        Write-Host "    $buildPageLink"
    }    
    
    Write-Host "Triggered build Id: $($buildId)"
    Write-Host "==================== Trigger Completed ====================="

    if ($buildId -gt 0) {
        return $buildId
    }
    else {
        throw "Trigger build Failed"
    }
}

$pipelineId = Get-PipelineIdByName -PipelineName $PipelineName
$buildId = Trigger-Build -PipelineId $pipelineId

if ($BuildIdVariableName -ne "") {
    Write-Host "##vso[task.setvariable variable=$BuildIdVariableName]$buildId"
    Write-Host "##vso[task.setvariable variable=$BuildIdVariableName;isOutput=true]$buildId"
}