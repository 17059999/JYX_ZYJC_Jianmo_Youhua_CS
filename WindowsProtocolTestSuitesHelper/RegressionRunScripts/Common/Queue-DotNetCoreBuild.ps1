# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Queue-DotNetCoreBuild.ps1
# Purpose:        Trigger the .net core build pipeline and get the build id.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:               The access token
# $ApiUrl:                    The API url
# $TargetRepoBranch:          The target repo branch name
# $BuildParameters:           The build parameters
# $BuildPipelineName:         The build pipeline name
# $BuildIdVariableName:       The build ID variable name
#----------------------------------------------------------------------------

Param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$TargetRepoBranch = "",
    [string]$BuildParameters = "",
    [string]$BuildPipelineName,
    [string]$BuildIdVariableName
)

Import-Module .\DevOpsLib.psm1

function Trigger-DotNetCoreBuild {
    param (
        [string]$PipelineId
    )

    $selfInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $env:BUILD_BUILDID

    $jobParameters = if (($selfInfo.reason -eq "pullRequest") -or ($selfInfo.parameters -match "system.pullRequest")) {
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
        if ($BuildParameters -ne "") { "$pullRequestParameters, $BuildParameters" } else { "$pullRequestParameters" }
    }
    else {
        $BuildParameters
    }

    # Assume that the name of PTF build pipeline contains "PTF"
    $triggerReason = if ($BuildPipelineName -match "PTF") { "manual" } else { $selfInfo.reason }

    $buildId = Trigger-Build -ApiUrl $ApiUrl `
        -AccessToken $AccessToken `
        -CurrBuildId $env:BUILD_BUILDID `
        -PipelineId $PipelineId `
        -JobParameters $jobParameters `
        -TargetRepoBranch $TargetRepoBranch `
        -Reason $triggerReason
        
    if ($buildId -gt 0) {
        return $buildId
    }
    else {
        throw "Trigger build Failed"
    }
}

Write-Host "Get pipeline ID By pipeline name: $BuildPipelineName"
$pipelineId = Get-PipelineIdByName -ApiUrl $ApiUrl -AccessToken $AccessToken -PipelineName $BuildPipelineName
$buildId = Trigger-DotNetCoreBuild -PipelineId $pipelineId
$count = 30
$dotNetCoreBuildInfo = $null
while ($count -gt 0) {
    Start-Sleep -Seconds 60
    try {
        $dotNetCoreBuildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $buildId
    }
    catch [System.InvalidOperationException] {
        continue
    }
    Write-Host "job status: $($dotNetCoreBuildInfo.status)"
    if($dotNetCoreBuildInfo.status -eq "notStarted"){
        continue
    }
    if ($dotNetCoreBuildInfo.status -eq "completed") {
        break
    }
    else {
        $count -= 1
    }
}

$dotNetCoreBuildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $buildId
if (($dotNetCoreBuildInfo.status -ne "completed") -or ($dotNetCoreBuildInfo.result -notmatch "succeeded")) {
    Write-Host "Build failed or timed out, cannot get artifacts..." -ForegroundColor Red
    exit 1
}

Write-Host "##vso[task.setvariable variable=$BuildIdVariableName]$buildId"