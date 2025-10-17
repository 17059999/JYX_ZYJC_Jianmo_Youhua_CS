###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           ManualRun-MultipleEnv.ps1
## Purpose:        Manual Setup environments for multiple environments for one testsuite, this is used to manually set up event environments 
##                 only for build azure environment
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later.
## Input parameter is 
##      TestSuiteName       :  Test Suite name
##      ConfigFileFolder    :  Environment xml folder of current TestSuite, it's generate by ManualRun-RegressionWithCSV.ps1
##      MsiFolder           :  Folder for storing msi files, each test suite has its own folder in msiFolder for storing its own msi file
##                             E.g. msiFolder\Fileserver\xxx.msi  msiFolder\ADFamily\yyy.msi  msiFolder\Kerberos\zzz.msi
##                             E.g. msiFolder\RDP\Client\mmm.msi  msiFolder\RDP\Server\nnn.msi      
##      TSHelperFolder      :  Helper Repository folder name of test suites
## Process:
##  1. Call GenerateRegressionFolder.ps1 to generate regression folders 
##  2. Call Run-TestSuiteRegression.ps1 to build each environments
##  3. Wait all current testsuite's jobs complete.
###########################################################################################

Param
(
    # The name of the Test Suite, only used to fetch XML configuration file and specify log and vm folder
    #[Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
    [string]$TestSuiteName="FileServer",
    # The folder of the environment XML files, indicating environments xml folder you want to configure
    #[Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
    [string]$ConfigFileFolder="RegressionEnvironments\FileServer",
    # Folder for storing msi files
    [string]$MsiFolder,
    # Azure Subscriptoion Id
    [string]$subscriptionId = "43ceac48-878b-45c7-9a98-4bdca5a11f25",
    # Azure Resource Group for test suite VMs to be created.
    [string]$resourceGroup = "TestSuiteOnAzure",
    # Azure AAD application ID of TSForPowershell
    [string]$applicationId = "ded7315a-2d0b-47b0-8b4f-9d34eb0a6288",
    # Azure certificate thumbprint for TSpowershell, it can be configured under App registrations(Preview)=>Certificates & Secrets
    # Jenkins slave installed private certifciate and upload the public cert to azure
    [string]$thumbPrint = "c13a03e41fe0220fab4b688951fa986e8441385b",
    # Azure AAD Directory(tenant) ID of TSForPowershell
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    # Azure Storage Account Name, this account is used to save Tools, Build Files, Testsuite logs... 
    [string]$storageAccount = "testsuiteshare",
    # Azure Storage Share Name
    [string]$storageShareName = "protocoltestsuiteshare",
    # Azure Resource Group for File Share
    [string]$fileShareResourceGroup = "TestSuiteOnAzure",
     # Manual Regression Type:Azure or Local
    [string]$regressiontype="Azure",
    # local Manual RootSharePath
    [string]$LocalRootSharePath="\\pet-storage-04\PrototestRegressionShare\ProtocolTestSuite",
    # Helper source folder
    [string]$TSHelperFolder="_Helper"
)

$InvocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent
Push-Location "$InvocationPath\..\"
# Test Suite Root Folder under workspace
$HelperRootPath = Get-Location
$TestSuiteRootPath = "$InvocationPath\..\..\"
$jobNamePrefix = "RegressionJob"
$TestsuiteRegressionFolder = "ManualRegression\"+$TestSuiteName + "_RunRegression"
$LogsPath = "$TestSuiteRootPath\$TestsuiteRegressionFolder\TestResults"

Function Write-TestSuiteInfo {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [string]$ForegroundColor = "White",
        [string]$BackgroundColor = "DarkBlue")

    # WinBlue issue: Start-Transcript cannot write the log printed out by Write-Host, as a workaround, use Write-output instead
    # Write-Output does not support color
    if ([Double]$Script:HostOsBuildNumber -eq [Double]"6.3") {
        ((Get-Date).ToString() + ": $Message") | Out-Host
    }
    else {
        Write-Host ((Get-Date).ToString() + ": $Message") -ForegroundColor $ForegroundColor -BackgroundColor $BackgroundColor
    }
}

Function Write-TestSuiteWarning {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [switch]$Exit)

    Write-TestSuiteInfo -Message "[WARNING]: $Message" -ForegroundColor Yellow -BackgroundColor Black
    if ($Exit) {exit 1}
}

function Start-RunJobsOnAzure {
    param (
        [System.Collections.Generic.List[String]]$jobList
    )
    
    $index = 1
    foreach ($job in $jobList) {
        $jobName = $jobNamePrefix + $index.ToString()
        Write-TestSuiteInfo "Start regression job for environment: $jobName"
        $scriptBlock = [Scriptblock]::Create($job)
        Start-Job -ScriptBlock $scriptBlock -Name $jobName | Out-Null
        $index = $index + 1
    }
}

function Wait-RegressionJobs {
    param (
        $jobsCount
    )
    
    # Start to check job status, exit loop when all jobs completed
    $completeCount = 0
    $failedCount = 0
    $timeoutCount = 600; # default timeout is 10 hours

    do {
        Start-Sleep -Seconds 60
        $completeCount = 0
        $failedCount = 0
        $completeCount = (Get-Job -Name "$jobNamePrefix*" | Select-Object -Property Name, State | Where-Object { ($_.State -eq "Completed")} | Measure-Object).Count
        $failedCount = (Get-Job -Name "$jobNamePrefix*" | Select-Object -Property Name, State | Where-Object { ($_.State -eq "Failed")} | Measure-Object).Count

        Write-TestSuiteInfo "Total Jobs: $jobsCount, Completed Jobs Count: $completeCount, Failed Jobs Count: $failedCount"
        $timeoutCount--
    } 
    until((($completeCount + $failedCount) -eq $jobsCount) -Or ($timeoutCount -eq 0))

    $failedJobs = Get-Job -Name "$jobNamePrefix*" | Select-Object -Property Name, State | Where-Object { ($_.State -eq "Failed")}

    Get-Job -Name "$jobNamePrefix*" | ForEach-Object {
        $_ | Receive-Job
    }
    foreach($failedJob in $failedJobs){
        $failedReason = $failedJob.JobStateInfo.Reason
        Write-TestSuiteWarning "Job: $($failedJob.Name) failed, Reason: $failedReason"
    }

    Write-TestSuiteInfo "All jobs completed"
}

function Run-AllExistingEnvironments {
    param (
        [string]$RegressionRootPath
    )
    # Read folder to get all xml
    # Create task list and then call Run-MultipleJobsOnAzure
    $azureJobList = New-Object 'System.Collections.Generic.List[String]'
    Get-ChildItem -Path $ConfigFileFolder -filter "*.xml" | ForEach-Object {
        $fileFullPath = $_.FullName
        $fileName = $_.Name
        Write-TestSuiteWarning "Find environment $fileName, add to job list"
        # copy xml to VSTORMLITEFiles folder
        # Check Environment, if Azure then call Azure script, otherwise call local script
        [Xml]$xmlContent = Get-Content $fileFullPath
        if($regressiontype -match "Azure"){
           
            if(!(Test-Path -Path "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML"))
            {
                New-Item -Path "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML" -ItemType Directory -Force
            }
            Copy-Item $fileFullPath -Destination "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\"
            $scriptBlockStr = "$RegressionRootPath\AzureScripts\Run-TestSuiteRegression.ps1" + `
                ' -TestSuiteName "' + $TestSuiteName + `
                '" -EnvironmentName "' + $fileName + `
                '" -subscriptionId "' + $subscriptionId + `
                '" -resourceGroup "' + $resourceGroup + `
                '" -applicationId "' + $applicationId + `
                '" -thumbPrint "' + $thumbPrint + `
                '" -tenantId "' + $tenantId + `
                '" -storageAccount "' + $storageAccount + `
                '" -fileShareResourceGroup "' + $fileShareResourceGroup + `
                '" -storageShareName "' + $storageShareName + '"'
            $azureJobList.Add($scriptBlockStr)
        }else{
            Write-TestSuiteWarning "Run local manual scripts"
            $ClientAnswerFile=$xmlContent.lab.core.ClientAnswerFile
            $ClientDiskName=$xmlContent.lab.core.ClientDiskName
            $ServerAnswerFile=$xmlContent.lab.core.ServerAnswerFile
            $ServerDiskName=$xmlContent.lab.core.ServerDiskName

            Write-TestSuiteWarning "TestSuiteName:$TestSuiteName"
            Write-TestSuiteWarning "EnvironmentName:$fileName"
            Write-TestSuiteWarning "ClientAnswerFile:$ClientAnswerFile"
            Write-TestSuiteWarning "ClientDiskName:$ClientDiskName"
            Write-TestSuiteWarning "ServerAnswerFile:$ServerAnswerFile"
            Write-TestSuiteWarning "ServerDiskName:$ServerDiskName"
           
            #copy xml to target
            if(!(Test-Path -Path "$HelperRootPath\..\..\WinteropProtocolTesting\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML"))
            {
                New-Item -Path "$HelperRootPath\..\..\WinteropProtocolTesting\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML" -ItemType Directory -Force
            }
            Copy-Item $fileFullPath -Destination "$HelperRootPath\..\..\WinteropProtocolTesting\ProtocolTestSuite\$TestSuiteName\VSTORMLITEFiles\XML\"
            $BuildShareFolder="$HelperRootPath\..\..\WinteropProtocolTesting"
            Write-TestSuiteWarning "BuildShareFolder:$BuildShareFolder"

            $scriptBlockStr="$HelperRootPath\..\RegressionRunScripts\LocalRegression\Run-LocalTestSuiteRegression.ps1" + `
            ' -TestSuiteName "' + $TestSuiteName + ` 
            '" -EnvironmentName "' + $fileName + ` 
            '" -ClientAnswerFile "' + $ClientAnswerFile + ` 
            '" -ClientDiskName "' + $ClientDiskName + ` 
            '" -ServerAnswerFile "' + $ServerAnswerFile + ` 
            '" -ServerDiskName "' + $ServerDiskName + ` 
            '" -BuildShareFolder "' + $BuildShareFolder + '"'
            $azureJobList.Add($scriptBlockStr)
        }
    }

    # Remove all existing regression jobs
    Remove-Job -Name "$jobNamePrefix*" -Force

    # Start run jobs on Azure and local
    Start-RunJobsOnAzure -jobList $azureJobList

    # Wait Jobs complete
    $totalJobs = $azureJobList.Count
    Wait-RegressionJobs -jobsCount $totalJobs

    # All jobs Completed, start to collect regression result
    Collect-TestSuiteResult
}

Function Start-RunJobLog {
    $currentDatetime = Get-Date -format "yyyy-MM-dd_HHmmss"
    $LogFilePath = "$TestSuiteRootPath\TestResults"
    
    if(!(Test-Path $LogFilePath)){
        New-Item -ItemType Directory $LogFilePath
    }

    # Stop the previous transcript
    try {
        Stop-Transcript -ErrorAction SilentlyContinue
    }
    catch [System.InvalidOperationException] {}

    # Start new transcript
    Start-Transcript -Path "$LogFilePath\ManualRun-MultipleEnv.ps1_$currentDatetime.log" -Append -Force
    Write-TestSuiteWarning "TestSuite Root Folder $TestSuiteRootPath"
}

Function Collect-TestSuiteResult{
    # Search TestSuiteResult.html under $LogsPath
    Get-ChildItem -Path $LogsPath -Filter TestSuiteResult.html -Recurse -ErrorAction SilentlyContinue -Force | ForEach-Object {
        $tempFileName = $_.Directory.Name +"_" + $_.Name
        Copy-Item $_.FullName -Destination "$LogsPath\$tempFileName"
    }
}

Function Compelete-RunJobs {
    Write-TestSuiteWarning "ManualRun-MultipleEnv.ps1 completed"
    Stop-Transcript
}


Function Main{
    # Start Transcript
    Start-RunJobLog

    if ($regressiontype -eq "Azure") {
        & "$HelperRootPath\RegressionRunScripts\AzureRegression\GenerateRegressionFolder.ps1" -TestSuiteName $TestSuiteName -TSHelperFolder $TSHelperFolder -TSRegressionFolder $TestsuiteRegressionFolder -MsiFolder $MsiFolder
    }else{
        # 1. if the "WinteropProtocolTesting" folder is not found,we must create it.
        # 2. copy the folders to the "WinteropProtocolTesting" folder
        $WinteropProtocolTesting ="$HelperRootPath\..\..\WinteropProtocolTesting"
        Write-TestSuiteWarning $WinteropProtocolTesting
        if (!(Test-Path -Path $WinteropProtocolTesting)) {
            New-Item -ItemType Directory $WinteropProtocolTesting
        }

        $LocalRegressionSharedFolder ="$HelperRootPath\..\LocalRegressionSharedFolder"
        Write-TestSuiteWarning $LocalRegressionSharedFolder
        if (!(Test-Path -Path $LocalRegressionSharedFolder)) {
            New-Item -ItemType Directory $LocalRegressionSharedFolder
        }
        else{
            Remove-Item "$LocalRegressionSharedFolder\*" -Recurse -Force
        }
        
        if($testSuiteName -match "RDPServer"){
            & "$HelperRootPath\..\RegressionRunScripts\ManualRunScripts\CopyTestSuiteToShare.ps1" -TestSuite RDP -EndpointName Server -TargetTestSuiteName RDPServer -TestSuiteShareFolder "$LocalRegressionSharedFolder\ProtocolTestSuite"
        }elseif ($testSuiteName -match "RDPClient") {
            & "$HelperRootPath\..\RegressionRunScripts\ManualRunScripts\CopyTestSuiteToShare.ps1" -TestSuite RDP -EndpointName Client -TargetTestSuiteName RDPClient -TestSuiteShareFolder "$LocalRegressionSharedFolder\ProtocolTestSuite"
        }else{
            & "$HelperRootPath\..\RegressionRunScripts\ManualRunScripts\CopyTestSuiteToShare.ps1" -TestSuite $testSuiteName -TestSuiteShareFolder "$LocalRegressionSharedFolder\ProtocolTestSuite"
        }
        Write-Host "Copy $MsiFolder to $LocalRegressionSharedFolder\ProtocolTestSuite\"
        Copy-Item  "$MsiFolder\*" -Destination "$LocalRegressionSharedFolder\ProtocolTestSuite\" -Recurse -Force

        Write-Host "Copy TestSuite commonscripts to share folder:$TestSuiteRootPath"

        Copy-Item  "$TestSuiteRootPath\RegressionRunScripts\Common\*" -Destination "$LocalRegressionSharedFolder\ScriptLib\" -Recurse -Force

        & "$HelperRootPath\..\RegressionRunScripts\ManualRunScripts\CopyScriptLibToShare.ps1" -ShareFolderPath $LocalRegressionSharedFolder -UpdateScriptLib
        
        Write-Host "=============  Download ProtocolTestSuite, ScriptLib, VSTORMLITE files from local share  ============"
        Copy-Item "$LocalRegressionSharedFolder\*" "$WinteropProtocolTesting" -Recurse -Force
    }

    Write-TestSuiteWarning "complete copy folders"

    #Read folder to get all xml and then start to run regression for each environment
    Run-AllExistingEnvironments -RegressionRootPath "$TestSuiteRootPath\$TestsuiteRegressionFolder"

    Compelete-RunJobs
}

Main

Pop-Location