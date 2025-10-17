# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Queue-DotNetCoreRegressions.ps1
# Purpose:        Queue .NET Core version regression builds.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:               The access token
# $ApiUrl:                    The API url
# $TestSuiteBuildId:          The test suite build id
# $RegressionPipelineNames:   The regression pipeline line name list
# $TargetHelperBranch:        The target helper branch name
# $TargetRepoCsvFile:         The target repo csv file name
# $TestFilter:                The test filter key words
#----------------------------------------------------------------------------

Param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$TestSuiteBuildId,
    [string]$RegressionPipelineNames,
    [string]$TargetHelperBranch,
    [string]$TargetRepoCsvFile,
    [string]$TestFilter  
)

Import-Module .\DevOpsLib.psm1

$Script:regressionBuildIds = New-Object -TypeName System.Collections.Generic.Queue[int] -ArgumentList @()
$Script:regressionPageLinks = @()
$currentPipeline = ""

function Trigger-Regression {
    param (
        [string]$PipelineId
    )

    $parameters = ""

    if($currentPipeline -contains  @("SMBD", "Linux")){
        $parameters += "`"test.OperatingSystem`": `Linux`","
    }

    $parameters += "`"targetRepo.csvFile`": `"$($TargetRepoCsvFile)`","
    $parameters += "`"test.filter`": `"$($TestFilter)`","
    $parameters += "`"test.buildId`": `"$($TestSuiteBuildId)`""

    $buildId = Trigger-Build -ApiUrl $ApiUrl `
        -AccessToken $AccessToken `
        -CurrBuildId $env:BUILD_BUILDID `
        -PipelineId $PipelineId `
        -JobParameters $parameters `
        -TargetRepoBranch $TargetHelperBranch `

    $buildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $buildId
    Write-Host "Triggered build info:"
    Write-Host ($buildInfo | ConvertTo-Json)

    $Script:regressionBuildIds.Enqueue($buildId)
    $Script:regressionPageLinks += $buildInfo._links.web.href
    
    Write-Host "TestSuiteBuildId: $($TestSuiteBuildId)" -ForegroundColor Green
    Write-Host "RegressionBuildId: $($buildId)" -ForegroundColor Green
    Write-Host "==================== Trigger Completed ====================="
}

foreach ($regressionPipelineName in $RegressionPipelineNames.Split(';')) {
    Write-Host "Trigger regression: $regressionPipelineName"
    $currentPipeline = $regressionPipelineName
    $pipelineId = Get-PipelineIdByName -ApiUrl $ApiUrl -AccessToken $AccessToken -PipelineName $regressionPipelineName

    if($currentPipeline -contains  @("SMBD", "Linux")){
        Start-Sleep -Seconds 120
    }

    Trigger-Regression -PipelineId $pipelineId
}

Write-Host "Web page links to the queued regression builds:"
foreach ($link in $Script:regressionPageLinks) {
    Write-Host "    $link"
}

Write-Host "Wait Regression Builds to Complete..."
$downstreamJobsFailed = $false
while ($Script:regressionBuildIds.Count -gt 0) {
    Start-Sleep -Seconds 60

    $buildId = $Script:regressionBuildIds.Dequeue()
    try {
        $buildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $buildId
    }
    catch [System.InvalidOperationException] {
        $Script:regressionBuildIds.Enqueue($buildId)
        continue
    }
    
    if ($buildInfo.status -in @("inProgress", "notStarted")) {
        $Script:regressionBuildIds.Enqueue($buildId)
        continue
    }

    if (($buildInfo.status -ne "completed") -or ($buildInfo.result -notmatch "succeeded")) {
        $downstreamJobsFailed = $true
        continue
    }
}

if ($downstreamJobsFailed) {
    Write-Host "##vso[task.logissue type=error]Downstream jobs did not complete." -ForegroundColor Red
    exit 1
}