# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Collect-RegressionResult.ps1
# Purpose:        Collect the regression test result.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $TestSuite:              The Test Suite name
# $EnvType:                The environment name: Azure or Local
# $ApiUrl:                 The API url
# $AccessToken:            The access token
# $SubscriptionId:         The Azure subscriptions id
# $SourceBuildId:          The source build id
#----------------------------------------------------------------------------

param
(
    [string]$TestSuiteName,
    [string]$EnvType,
    [string]$ApiUrl,
    [string]$AccessToken,
    [string]$SubscriptionId,
    [string]$SourceBuildId,
    [string]$ResultStorageAccount = "",
    [string]$FileShareResourceGroup = "",
    [string]$runTests = "true",
    [string]$checkVMs = "true"
)

$invocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$testSuiteRootPath = Get-Location
$containerUri = "https://ms.portal.azure.com/#blade/Microsoft_Azure_Storage/ContainerMenuBlade/overview/storageAccountId/%2Fsubscriptions%2F$subscriptionId%2FresourceGroups%2F$FileShareResourceGroup%2Fproviders%2FMicrosoft.Storage%2FstorageAccounts%2F$ResultStorageAccount/path/{0}/etag/"
$reportTimeZoneInfo = [System.TimeZoneInfo]::FindSystemTimeZoneById("China Standard Time")
# The flag indicate whether it is triggered from a remote project (GitHub) or not
$isRemoteApiUrlValid = [System.Uri]::IsWellFormedUriString($env:REMOTE_APIURL, [System.UriKind]::Absolute)

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

Import-Module .\DevOpsLib.PSM1

function Generate-TestCaseRunTable {
    param(
        [string]$TestSuite,                       # The Test Suite name
        [string]$EnvType,                         # The environment name: Azure or Local
        [string]$Platform,                        # The platform: 20H1, RS5, etc.
        [string]$ResourceGroup,                   # The Azure Resource Group
        [string]$TotalNum,                        # The total case number
        [string]$PassedNum,                       # The passed case number
        [string]$FailedNum,                       # The failed case number
        [string]$InconclusiveNum,                 # The inconclusive case number
        [string]$Status,                          # The job status
        [string]$Machine,                         # The host machine name
        [string]$ResultLogPath,                   # The test result log url
        [string]$ContainerName,                   # The Azure storage container name that store the test result
        [string]$JobUrl,                          # The pipeline regression url
        [string]$StartTime,                       # The regression start time
        [string]$EndTime,                         # The regression end time
        [string]$ErrorMessage,                    # The regression error message
        [string]$JsonInfoName                     # The json file name that record the test result
    )

    if (![string]::IsNullOrEmpty($ContainerName)) {
        $ResultLogPath = $containerUri -f $ContainerName
    }
    $Link = $JsonInfoName.Replace(".json", "")

    $localStartTime = [System.TimeZoneInfo]::ConvertTimeFromUtc($StartTime, $reportTimeZoneInfo).ToString("MM/dd/yyyy HH:mm:ss")
    $localEndTime = [System.TimeZoneInfo]::ConvertTimeFromUtc($EndTime, $reportTimeZoneInfo).ToString("MM/dd/yyyy HH:mm:ss")

    $testResults = "<tr class='normal'>"
    $testResults += "<td>$TestSuite</td>"
    $testResults += "<td>$EnvType</td>"
    $testResults += "<td>$Platform</td>"
    $testResults += "<td>$localStartTime</td>"
    $testResults += "<td>$localEndTime</td>"
    $testResults += "<td>$TotalNum</td>"
    $testResults += "<td><font class='font-green'>$PassedNum</font></td>"
    $testResults += "<td><font class='font-red'>$FailedNum</font></td>"
    $testResults += "<td><font class='font-orange'>$InconclusiveNum</font></td>"
    $testResults += "<td>$Status</td>"
    if ($EnvType -eq "Azure") {
        $testResults += "<td>"
        $Machine.Split(",") | `
            ForEach-Object {
            $machinePath = "https://ms.portal.azure.com/#@microsoft.onmicrosoft.com/resource/subscriptions/$SubscriptionId/resourceGroups/$ResourceGroup/providers/Microsoft.Compute/virtualMachines/$_/overview"
            $testResults += "<a href=`"$machinePath`">$_</a>,"
        }
        if ($testResults[$testResults.Length - 1] -eq ",") {
            $testResults = $testResults.Substring(0, $testResults.Length - 1)
        }
        $testResults += "</td>"
    }
    else {
        $testResults += "<td>$Machine</td>"
    }
    $testResults += "<td><a href=`"$ResultLogPath`">resultLog</td>"
    $testResults += "<td><a href=`"$JobUrl`">$Link</td>"
    $testResults += "<td><font class='font-red'>$ErrorMessage</font></td>"
    $testResults += "</tr>"
    return $testResults
}

function Generate-Report {
    param (
        [boolean]$IsTriggeredFromGithub,
        [string]$LogFilePath
    )

    $result = @{ }
    $result.buildResult = 'Success'
    $reportBody = ''
    $checkBody = ''
    Write-Host "logFilePath: $LogFilePath"


    Get-ChildItem $LogFilePath | Where-Object {
        $_.PSIsContainer
    } | ForEach-Object {
        Get-ChildItem $_.FullName -Filter "*.json" | Where-Object {
        -not $_.Name.Contains("check")
        }
    } | ForEach-Object {
        $jsonInfo = (Get-Content $_.fullname | ConvertFrom-Json)

        if($runTests -eq "true")
        {
            if ($jsonInfo.result -match "Failed") {
                $result.buildResult = "Failed"
            }

            if (-not ($jsonInfo.status -match "completed")) {
                Write-Host "Incomplete JSON result file found: $($_.Name)"
                $Script:testFailed = $true
            }

            $legalStatuses = @("Completed", "Not Run", "Timed Out")
            $envStatus = if ($jsonInfo.status -in $legalStatuses) {
                $jsonInfo.status
            }
            else {
                "Unknown"
            }

            $resourceGroup = if ($jsonInfo.envType -eq "Azure") {
                $jsonInfo.resourceGroup
            }
            else {
                ''
            }
        }
        $reportBody += Generate-TestCaseRunTable -TestSuite $jsonInfo.testSuiteName `
            -EnvType $jsonInfo.envType `
            -Platform $jsonInfo.platform `
            -ResourceGroup $resourceGroup `
            -TotalNum $jsonInfo.totalNum `
            -PassedNum $jsonInfo.passedNum `
            -FailedNum $jsonInfo.failedNum `
            -InconclusiveNum $jsonInfo.inconclusiveNum `
            -Status $envStatus `
            -Machine $jsonInfo.machine `
            -ResultLogPath $jsonInfo.resultLogPath `
            -ContainerName $jsonInfo.containerName `
            -JobUrl $jsonInfo.jobUrl `
            -StartTime $jsonInfo.startTime `
            -EndTime $jsonInfo.endTime `
            -ErrorMessage $jsonInfo.errorMessage `
            -JsonInfoName $_.name
    }

    Write-Host "checkVMs : $checkVMs"
    if($checkVMs -eq "true"){

   
        Get-ChildItem $LogFilePath | Where-Object {
        $_.PSIsContainer
        } | ForEach-Object {
        Get-ChildItem $_.FullName -Filter "*.json" | Where-Object {
            $_.Name.Contains("check")
        } | ForEach-Object {
            
            $jsonInfo = (Get-Content $_.fullname | ConvertFrom-Json)
            $checkBody += "<tr class='normal'>"
            $checkBody += "<td>"
            $checkBody += $jsonInfo.machine
            $checkBody += "</td><td>"
            $checkBody += $jsonInfo.ipinfo
            $checkBody += "</td><td>"
            $checkBody += $jsonInfo.diskUsage
            $checkBody += "</td><td>"
            $checkBody += $jsonInfo.quicEnabled
            $checkBody += "</td><td>"
            $checkBody += $jsonInfo.testsuiteExisted
            $checkBody += "</td><td>"
            $checkBody += $jsonInfo.ptmExisted
            $checkBody += "</td></tr>"
            }
        }
    }
    $targetBuildInfo = if ($IsTriggeredFromGithub) {
        Get-BuildInfoByBuildId -ApiUrl $env:REMOTE_APIURL -AccessToken $env:REMOTE_ACCESSTOKEN -BuildId $SourceBuildId
    }
    else {
        Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $SourceBuildId
    }
    $sourceBranch = $targetBuildInfo.sourceBranch
    $targetBuildPipelineName = $targetBuildInfo.definition.name

    if ($targetBuildPipelineName -Like '*CodeSign') {
        # Scheduled build does not have "parameters" property
        if ($targetBuildInfo.reason -eq "schedule") {
            $targetBuildPipelineId = $targetBuildInfo.definition.id
            [string]$targetBuildPipelineRevision = $targetBuildInfo.definition.revision
            $sourceBranch = Get-PipelineVariableDefaultValueByName -ApiUrl $ApiUrl `
                -AccessToken $AccessToken `
                -PipelineId $targetBuildPipelineId `
                -PipelineRevision $targetBuildPipelineRevision `
                -VariableName "build.testSuitesBranch"
        }
        else {
            $parameters = $targetBuildInfo.parameters | ConvertFrom-Json
            $sourceBranch = $parameters.'build.testSuitesBranch'
        }
    }

    $currentBuildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $env:BUILD_BUILDID
    $buildLink = $currentBuildInfo._links.web.href
    $triggeredBy = $currentBuildInfo.requestedFor.displayName
    $helperBranch = $currentBuildInfo.sourceBranch

    $prUrl = ""

    $jobStartTime = ([DateTime]$currentBuildInfo.startTime).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss")
    $localJobStartTime = [System.TimeZoneInfo]::ConvertTimeFromUtc($jobStartTime, $reportTimeZoneInfo)
    $localJobEndTime = [System.TimeZoneInfo]::ConvertTimeFromUtc([System.DateTime]::UtcNow, $reportTimeZoneInfo)

    if ($IsTriggeredFromGithub) {
        $Script:reportEnvType = "GitHub"

        if ($targetBuildInfo.reason -eq 'pullRequest') {
            $triggeredBy = $targetBuildInfo.triggerInfo.'pr.sender.name'
            $parameters = $targetBuildInfo.parameters | ConvertFrom-Json
            $sourceBranch = $parameters.'system.pullRequest.sourceBranch'
            $prNumber = $parameters.'system.pullRequest.pullRequestNumber'
            [string]$gitUrl = $parameters.'system.pullRequest.sourceRepositoryUri'
            $gitHubUrl = $gitUrl.TrimEnd('.git')
            $prUrl = "$gitHubUrl/pull/$prNumber"
        }
        else {
            $triggeredBy = $targetBuildInfo.requestedFor.displayName
        }

        if ($sourceBranch.StartsWith('refs/heads/')) {
            $sourceBranch = $sourceBranch.Substring('refs/heads/'.Length)
        }

        if ($helperBranch.StartsWith('refs/heads/')) {
            $helperBranch = $helperBranch.Substring('refs/heads/'.Length)
        }

        Write-Host "Generate html file for Github user"
        $content = Get-Content ".\RegressionResultTemplate.html";
        $content = $content `
            -replace "{{TESTSUITE_NAME}}", $TestSuiteName `
            -replace "{{START_TIME}}", $localJobStartTime.ToString("MM/dd/yyyy HH:mm:ss") `
            -replace "{{END_TIME}}", $localJobEndTime.ToString("MM/dd/yyyy HH:mm:ss") `
            -replace "{{BUILD_URL}}", $buildLink `
            -replace "{{TRIGGERED_BY}}", $triggeredBy `
            -replace "{{TESTSUITE_BRANCH}}", $sourceBranch `
            -replace "{{HELPER_BRANCH}}", $helperBranch `
            -replace "{{PULL_REQUEST}}", $prUrl `
            -replace "{{REPORT_BODY}}", $reportBody

        $outFilePath = "$InvocationPath..\AzureRegression\..\TestResults\GitHubReport.html"
    }
    else {
        $Script:reportEnvType = $jsonInfo.envType

        if ($targetBuildInfo.reason -eq 'pullRequest') {
            $triggeredBy = $targetBuildInfo.requestedFor.displayName
            $parameters = $targetBuildInfo.parameters | ConvertFrom-Json
            $sourceBranch = $parameters.'system.pullRequest.sourceBranch'
            [string]$gitUrl = $parameters.'system.pullRequest.sourceRepositoryUri'
            $prNumber = $parameters.'system.pullRequest.pullRequestId'
            $prUrl = "$gitUrl/pullrequest/$prNumber"
        }

        if ($sourceBranch.StartsWith('refs/heads/')) {
            $sourceBranch = $sourceBranch.Substring('refs/heads/'.Length)
        }

        if ($helperBranch.StartsWith('refs/heads/')) {
            $helperBranch = $helperBranch.Substring('refs/heads/'.Length)
        }

        Write-Host "Generate html file for regression report"
        $content = Get-Content ".\RegressionResultTemplate.html";
        $content = $content `
            -replace "{{TESTSUITE_NAME}}", $TestSuiteName `
            -replace "{{START_TIME}}", $localJobStartTime.ToString("MM/dd/yyyy HH:mm:ss") `
            -replace "{{END_TIME}}", $localJobEndTime.ToString("MM/dd/yyyy HH:mm:ss") `
            -replace "{{BUILD_URL}}", $buildLink `
            -replace "{{TRIGGERED_BY}}", $triggeredBy `
            -replace "{{TESTSUITE_BRANCH}}", $sourceBranch `
            -replace "{{HELPER_BRANCH}}", $helperBranch `
            -replace "{{PULL_REQUEST}}", $prUrl `
            -replace "{{REPORT_BODY}}", $reportBody
            #-replace "{{CHECK_BODY}}", $checkBody

        if($checkVMs -eq "true"){
            $checkcontent = Get-Content ".\RegressionCheckResultTemplate.html";
            $checkcontent = $checkcontent `
                -replace "{{CHECK_BODY}}", $checkBody
        }

        #$relativePath = Join-Path $InvocationPath "..\TestResults"

        #$reportPath = [System.IO.Path]::GetFullPath($relativePath)
        $outFilePath = "$InvocationPath\Report.html"
    }
    
    Write-Host "outFilePath: $outFilePath"
    
    #$resultPath = Join-Path $InvocationPath "..\TestResults"
    #$TestPath = [System.IO.Path]::GetFullPath($resultPath)
    #Write-Host "TestPath: $TestPath"
    #if (!(Test-Path "$TestPath")){
    #    mkdir "$TestPath"
    #}
    
    $content | Out-File $outFilePath -Encoding utf8 -Force
    
    $checkcontent | Out-File -FilePath $outFilePath -Encoding utf8 -Append

    $result.outFilePath = $outFilePath
    return $result
}

function Collect-RegressionResult {
    $currentBuildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $env:BUILD_BUILDID
    $pipelineName = $currentBuildInfo.definition.name
    $buildNumber = $currentBuildInfo.buildNumber
    $triggeredBy = $currentBuildInfo.requestedFor.displayName

    if ($currentBuildInfo.reason -eq 'buildCompletion') {
        $SourceBuildId = $currentBuildInfo.triggeredByBuild.id
    }

    $targetBuildInfo = if ($isRemoteApiUrlValid) {
        Get-BuildInfoByBuildId -ApiUrl $env:REMOTE_APIURL -AccessToken $env:REMOTE_ACCESSTOKEN -BuildId $SourceBuildId
    }
    else {
        Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $SourceBuildId
    }
    $targetBuildPipelineName = $targetBuildInfo.definition.name

    if ($isRemoteApiUrlValid) {
        if ($targetBuildInfo.reason -eq "pullRequest") {
            $triggeredBy = $targetBuildInfo.triggerInfo.'pr.sender.name'
        }
        else {
            $triggeredBy = $targetBuildInfo.requestedFor.displayName
        }
    }

    Write-Host "TestSuiteRootPath: $testSuiteRootPath"
    #$jsonPath = Join-Path $testSuiteRootPath "..\TestResults"
    #$testJsonPath = [System.IO.Path]::GetFullPath($jsonPath)
    $logFilePath = "$testSuiteRootPath\TestResults"
    $result = Generate-Report -IsTriggeredFromGithub $isRemoteApiUrlValid -LogFilePath $logFilePath
    $mailSubject = if ($targetBuildPipelineName -Like '*CodeSign') {
        "[CodeSign]-"
    }
    else {
        ""
    }
    $mailSubject += "["
    $mailSubject += if ([string]::IsNullOrEmpty($Script:reportEnvType)) { "" } else { "$($Script:reportEnvType) " }
    $mailSubject += "Regression Result]"
    $mailSubject += " $pipelineName#$buildNumber triggered by [$triggeredBy] - $($result.buildResult)"
    Write-Host "MailSubject: $mailSubject"
    $mailBody = Get-Content $result.outFilePath -Raw
    $senderPassword = ConvertTo-SecureString $env:SMTP_SENDERPASSWORD -AsPlainText -Force

    & ".\Send-AzMail.ps1" `
        -SmtpHost $env:SMTP_SMTPHOST `
        -SmtpPort $env:SMTP_SMTPPORT `
        -SenderUsername $env:SMTP_SENDERUSERNAME `
        -SenderPassword $senderPassword `
        -SendTo $env:SENDTOTEST `
        -MailSubject $mailSubject `
        -MailBody    $mailBody
}

function Main {
    Collect-RegressionResult
}

Main

Pop-Location

if ($testFailed) {
    Write-Host "##vso[task.logissue type=error]Downstream jobs did not complete."
    exit 1
}