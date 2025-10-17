# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Queue-Regression.ps1
# Purpose:        According to TestSuiteName to queue the regression environment build.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $AccessToken:               The access token
# $ApiUrl:                    The API url
# $BuildDefinitionName:       The build definition name
# $ConfigFileFolder:          The config file folder path
# $Waiting:                   The flag to identify whether the pipeline job is completed
#----------------------------------------------------------------------------

Param(
    [string]$AccessToken,
    [string]$ApiUrl,
    [string]$BuildDefinitionName,
    [string]$ConfigFileFolder,
    [string]$Waiting,
    [string]$RunTests = "true",
    [string]$enableRestorePoint = "false"
) 
$rootPath = Get-Location

$Script:completedJobIds = New-Object System.Collections.Generic.HashSet[int] -ArgumentList @()
$Script:timedOutJobIds = New-Object System.Collections.Generic.HashSet[int] -ArgumentList @()
Import-Module .\DevOpsLib.psm1

#------------------------------------------------------------------------------------------
# Determine the result of the task by comparing the last result of the query with the QueueID
#------------------------------------------------------------------------------------------
function Check-BuildStatus {
    param (
        [Parameter(Mandatory = $TRUE)]
        [int]$TaskId,
        [Parameter(Mandatory = $TRUE)]
        [string]$Status 
    )   

    try {
        $buildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $TaskId
        Write-Host "The status of build $TaskId is: $($buildInfo.status)"
        if ($buildInfo.status -match $Status) {
            return $true
        }

        return $false
    }
    catch {
        $errorMessage = $_.Exception.Message
        Write-Host "Warning: The request failed, Error Message: $errorMessage";
        return $false
    }
}

function Check-BuildCompletion {
    param (
        [Parameter(Mandatory = $true)]
        [int]$TaskId
    )   

    return Check-BuildStatus -TaskId $TaskId -Status "completed"
}

function Check-BuildIsInProgress {
    param (
        [Parameter(Mandatory = $true)]
        [int]$TaskId
    )   

    return Check-BuildStatus -TaskId $TaskId -Status "inProgress"
}

function TriggerByConfiguredEnvironments {    
    $pipelineId = Get-PipelineIdByName -ApiUrl $ApiUrl -AccessToken $AccessToken -PipelineName $BuildDefinitionName
    Write-Host "PerEnv Pipeline: Get-PipelineId By Name: $BuildDefinitionName( $pipelineId )"

    $sourcePipelineId = Get-PipelineIdByName -ApiUrl $ApiUrl -AccessToken $AccessToken -PipelineName $env:BUILD_DEFINITIONNAME
    Write-Host "Regression Pipeline: Get-PipelineId By Name: $env:BUILD_DEFINITIONNAME( $sourcePipelineId )"

    $currentBuildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $env:BUILD_BUILDID
    $sourceBuildId = if ($currentBuildInfo.reason -eq 'buildCompletion') {
        $currentBuildInfo.triggeredByBuild.id
    }
    else {
        $env:SPECIFIC_BUILDID
    }

    $sourceBuildInfo = if ([System.Uri]::IsWellFormedUriString($env:REMOTE_APIURL, [System.UriKind]::Absolute)) {
        Get-BuildInfoByBuildId -ApiUrl $env:REMOTE_APIURL -AccessToken $env:REMOTE_ACCESSTOKEN -BuildId $sourceBuildId
    }
    else {
        Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $sourceBuildId
    }
    $sourceBranch = $sourceBuildInfo.sourceBranch

    # Read folder to get all xml
    # Create task list and then call Run-MultipleJobsOnAzure
    $jobList = New-Object System.Collections.Generic.List[object] -ArgumentList @()

    Get-ChildItem -Path $ConfigFileFolder -filter "*.xml" -Recurse | ForEach-Object {
        $fileFullPath = $_.FullName
        $fileName = $_.Name
        $TestSuiteName = Split-Path (Split-Path $fileFullPath -Parent) -Leaf
        Write-Host "$TestSuiteName`: Found environment $fileName, add it to the job list..."        

        # Check Environment, if Azure then call Azure script, otherwise call local script
        $job = @{ }

        [Xml]$xmlContent = Get-Content $fileFullPath
        $regressionType = $xmlContent.lab.core.regressiontype
        [int]$parsingResult = 0
        [int]$timeoutInMinutes = if ([int]::TryParse($xmlContent.lab.core.timeoutinminutes, [ref]$parsingResult)) {
            $parsingResult
        }
        else {
            # Assign a default timeout of 8 hours if the timeout is not set in XML
            60 * 8
        }
        	
        $jobParameters = ""
        $jobParameters += "`"test.testSuiteName`": `"$TestSuiteName`","
        $jobParameters += "`"test.sourceBranch`": `"$sourceBranch`","
        $jobParameters += "`"test.pipelineId`": `"$sourcePipelineId`","
        $jobParameters += "`"test.buildId`": `"$env:BUILD_BUILDID`","        
        $jobParameters += "`"test.environmentName`": `"$fileName`","
        $jobParameters += "`"test.runTests`": `"$RunTests`","
        $jobParameters += "`"enableRestorePoint`": `"$enableRestorePoint`""
        
        [int]$jobId = Trigger-Build -ApiUrl $ApiUrl -AccessToken $AccessToken -CurrBuildId $env:BUILD_BUILDID -PipelineId $pipelineId -JobParameters $jobParameters
        $job.jobId = $jobId
        $job.jobName = $BuildDefinitionName
        $job.environmentName = $fileName
        $job.regressionType = $regressionType
        $job.startTime = [System.DateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss")

        if ($job.regressionType -eq "Azure") {
            $job.resourceGroup = $xmlContent.lab.vmsetting.resourceGroup
        }

        $job.timeoutInMinutes = $timeoutInMinutes        
        $jobList.Add($job)

        # Start sleep 10 seconds then trigger next job
        Start-Sleep -Seconds 10
    }
    return $jobList
}

function Wait-Builds {
    param (
        $WaitJobList
    )
    Write-Host "##[group]Start waiting downstream jobs complete, timeout is the maximum of all job timeouts."
    [int]$timeoutCount = ($WaitJobList | ForEach-Object { $_.timeoutInMinutes } | Measure-Object -Maximum).Maximum
    Write-Host "The timeout is $timeoutCount minutes."
    #Write-Host ($WaitJobList | Format-List | Out-String)
    $waitJobDict = New-Object 'System.Collections.Generic.Dictionary[int, int]' -ArgumentList @()
    $WaitJobList | ForEach-Object { $waitJobDict.Add($_.jobId, $timeoutCount) }

    # Start to check job status, exit loop when all jobs completed
    $completedCount = 0
    $timedOutCount = 0
    $jobsCount = ($WaitJobList | Measure-Object).Count
    
    do {
        Start-Sleep -Seconds 60
        
        foreach ($job in $WaitJobList) {
            $jobId = $job.jobId
            if ((-not ($Script:completedJobIds.Contains($jobId))) -and (-not ($Script:timedOutJobIds.Contains($jobId)))) {
                
                $jobTimeoutCount = $waitJobDict[$jobId]

                if ($jobTimeoutCount -le 0) {
                    $job.endTime = [System.DateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss")
                    Write-Host "Build $jobId timed out."
                    Write-Host "JobName: $($job.jobName)"
                    Write-Host "StartTime: $($job.startTime)"
                    Write-Host "EndTime: $($job.endTime)"
                    
                    $timedOutCount += 1
                    $Script:timedOutJobIds.Add($jobId)
                    Cancel-BuildByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $jobId

                    $waitJobDict.Remove($jobId)
                    continue
                }
                
                $jobCompleted = Check-BuildCompletion -TaskId $jobId
                if ($jobCompleted) {
                    $job.endTime = [System.DateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss")
                    Write-Host "Build $jobId completed."
                    Write-Host "JobName: $($job.jobName)"
                    Write-Host "StartTime: $($job.startTime)"
                    Write-Host "EndTime: $($job.endTime)"

                    $completedCount += 1
                    $Script:completedJobIds.Add($jobId)

                    $waitJobDict.Remove($jobId)
                }
                else {
                    $jobInProgress = Check-BuildIsInProgress -TaskId $jobId
                    if ($jobInProgress) {
                        $waitJobDict[$jobId] = $jobTimeoutCount - 1
                    }
                }
            }
        }

        Write-Host "Total Jobs Count: $jobsCount, Completed Jobs Count: $completedCount, Timed Out Jobs Count: $timedOutCount"
    }
    until(($completedCount + $timedOutCount) -eq $jobsCount)

    # Wait Publish Artifacts 
    Start-Sleep -Seconds 60
    Write-Host "All jobs completed"
    Write-Host "##[endgroup]"
}

function GetArtifactsAndGenerateReport {
    param (
        [string]$JobName,
        [string]$JobId,
        [string]$JobStartTime,
        [string]$JobEndTime,
        [string]$EnvironmentName,
        [string]$RegressionType,
        [string]$ResourceGroup
    )

    $fileName = $EnvironmentName.Replace(".xml", "")

    Write-Host "Start getting results"
    Write-Host "jobName: $JobName"
    Write-Host "jobId: $JobId"
    Write-Host "EnvironmentName: $EnvironmentName"

    $hostMachineName = ""
    
    $buildInfo = Get-BuildInfoByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $JobId
    $buildJobUrl = $buildInfo._links.web.href

    try {
        $buildStartTime = ([DateTime]$buildInfo.startTime).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss")
        $buildEndTime = ([DateTime]$buildInfo.finishTime).ToUniversalTime().ToString("MM/dd/yyyy HH:mm:ss")
    }
    catch {
        $buildStartTime = $JobStartTime
        $buildEndTime = $JobEndTime
    }

    try {
        $localFilePath = "$rootPath\TestResults\$($JobId)"
        GetArtifactsByBuildId -ApiUrl $ApiUrl -AccessToken $AccessToken -BuildId $JobId -ArtifactName drop -DownloadPath $localFilePath

        $artifactFilesPath = "$localFilePath\drop"
        Get-ChildItem -Recurse $artifactFilesPath | Foreach-Object {
            $newPath = "$localFilePath\$($_.Name)"
            Move-Item $_.FullName -Destination $newPath -Force
        }
        Remove-Item $artifactFilesPath -Recurse -Force
        
        $hostNameFilePath = "$localFilePath\HostName.txt"
        if (Test-Path $hostNameFilePath) {
            $hostMachineName = Get-Content $hostNameFilePath | Out-String

            Write-Host "Host Name: $hostMachineName"
        }
        
        $jsonFilePath = "$localFilePath\$fileName.json"

        if (Test-Path $jsonFilePath) {
            $resultInfo = (Get-Content $jsonFilePath) | ConvertFrom-Json
            $resultInfo.startTime = $buildStartTime
            $resultInfo.endTime = $buildEndTime
            $resultInfo.jobUrl = $buildJobUrl
            
            if ($RegressionType -eq "Azure") {
                $resultInfo | Add-Member -Name "resourceGroup" -Value $ResourceGroup -MemberType NoteProperty
            }
            
            $resultInfo | ConvertTo-Json | Out-File $jsonFilePath -Encoding utf8 -Force
        }
        else {
            throw "JSON result file was not found."
        }

        Write-Host "Get results completed"
    }
    catch {
        $errorMessage = $_.Exception.Message
        Write-Host "Error: $errorMessage"
        $jobResDir = "$rootPath\TestResults\$($JobId)"
        if (-not (Test-Path $jobResDir)) {
            New-Item -ItemType Directory -Path $jobResDir
        }

        $configFileFullName = (Get-ChildItem -Path $ConfigFileFolder -filter $EnvironmentName -Recurse | Select-Object -Last 1).FullName

        [xml]$configContent = Get-Content "$configFileFullName"
        $testName = $configContent.lab.core.TestSuiteName
        if ($EnvironmentName -match "_") {
            $platform = $EnvironmentName.Substring($EnvironmentName.LastIndexOf("_") + 1).Replace(".xml", "")
        }

        if ($RegressionType -eq "Azure") {            
            $machineList = $configContent.lab.servers.vm
            $machineList | ForEach-Object {
                $machine += $_.name + ","
            }
            $hostMachineName = $machine.Substring(0, $machine.length - 1)
        }

        $jsonFilePath = "$jobResDir\$fileName.json"
        
        $resultInfo = @{
            testSuiteName   = $testName
            platform        = $platform
            envType         = $RegressionType
            startTime       = $buildStartTime
            endTime         = $buildEndTime
            totalNum        = "0"
            passedNum       = "0"
            failedNum       = "0"
            inconclusiveNum = "0"
            status          = if ($Script:timedOutJobIds.Contains($JobId)) { "Timed Out" } else { "Not Run" }
            machine         = $hostMachineName
            resultLogPath   = "n/a"
            jobUrl          = $buildJobUrl
            result          = "Failed"
        }

        if ($RegressionType -eq "Azure") {
            $resultInfo.resourceGroup = $ResourceGroup
        }

        $resultInfo | ConvertTo-Json | Out-File $jsonFilePath -Encoding utf8
    }
}

function Collect-TestSuiteResults {
    param (
        $JobList
    )

    if (!(Test-Path "$rootPath\TestResults")) {
        mkdir "$rootPath\TestResults"
    }
    $JobList | ForEach-Object {
        GetArtifactsAndGenerateReport -JobName $_.jobName `
            -JobId $_.jobId `
            -JobStartTime $_.startTime `
            -JobEndTime $_.endTime `
            -EnvironmentName $_.environmentName `
            -RegressionType $_.regressionType `
            -ResourceGroup $_.resourceGroup
    }
}

Function Main {
    $regressionJobList = TriggerByConfiguredEnvironments

    if ($Waiting -eq 'true') {
        Wait-Builds -WaitJobList $regressionJobList

        # All jobs Completed, start to collect regression results
        Collect-TestSuiteResults -JobList $regressionJobList
    }
}

Main

