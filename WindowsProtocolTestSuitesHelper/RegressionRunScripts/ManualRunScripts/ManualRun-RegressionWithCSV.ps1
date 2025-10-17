###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           ManualRun-RegressionWithCSV.ps1
## Purpose:        Setup environments for a configured excel or csv file
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later.
## Input parameter is 
##      configFile              :  CSV or Excel configureation file path of regression environments
##      filter                  :  Filter string, it used to filter out which environments you want to build in configFile
##      msiFolder               :  Folder for storing msi files, each test suite has its own folder in msiFolder for storing its own msi file
##                                 E.g. msiFolder\Fileserver\xxx.msi  msiFolder\ADFamily\yyy.msi  msiFolder\Kerberos\zzz.msi
##                                 E.g. msiFolder\RDP\Client\mmm.msi  msiFolder\RDP\Server\nnn.msi                                   
##      needRunRegression       :  Whether need run regression, if true, will run regression
##                                 if false, will not run regression, Only generate environment xml files from excel or csv file
##      subscriptionId          :  Azure Subscriptoion Id
##      resourceGroup           :  Azure Resource Group for test suite VMs to be created.
##      applicationId           :  Azure AAD application ID of TSForPowershell
##      thumbPrint              :  Azure certificate thumbprint for TSpowershell, it can be configured under App registrations(Preview)=>Certificates & Secrets
##      tenantId                :  Azure AAD Directory(tenant) ID of TSForPowershell
##      storageAccount          :  Azure Storage Account Name, this account is used to save Tools, Build Files, Testsuite logs... 
##      storageShareName        :  Azure Storage Share Name
##      fileShareResourceGroup  :  Azure Resource Group for File Share
##      tSHelperFolder          :  Helper Repository folder name of test suites
## Process:
##  1. Download ConfigureGenerator tool from azure file share
##  2. Generate environment xml by ConfigureGenerator tool after filtered then copy to each TestSuite
##  3. Call ManualRun-MultipleEnv.ps1 to run regression for each TestSuite
##  4. Wait all TestSuite regression job complete 
###########################################################################################

Param
(
    [Parameter( ValueFromPipeline = $true)]
    [string]$configFile="LocalRegression_FileServer_DotNotCore_WTW.csv",
    [Parameter(ValueFromPipeline = $true)]
    [string]$filter="",
    [Parameter(ValueFromPipeline = $true)]
    [string]$msiFolder="G:\Manual\drop\TestSuites\FileServer\deploy\FileServer-TestSuite-ServerEP.zip",
    [boolean]$needRunRegression=$true,
    [string]$subscriptionId = "43ceac48-878b-45c7-9a98-4bdca5a11f25",
    [string]$resourceGroup = "TestSuiteOnAzure",
    [string]$applicationId = "77b7511b-5d98-4309-a3bd-b5395b7d07be",
    [string]$thumbPrint = "D5A243410B4B8D6550ABFF1CA499C697F39D524A",
    [string]$tenantId = "72f988bf-86f1-41af-91ab-2d7cd011db47",
    [string]$storageAccount = "testsuiteshare",
    [string]$storageShareName = "protocoltestsuiteshare",
    [string]$fileShareResourceGroup = "TestSuiteOnAzure",
    [string]$tSHelperFolder="_Helper"
)

[string]$convertToolRootPath = "ToolShare\ConfigureGenerator"
$InvocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$AzureScriptsPath = "$InvocationPath\..\AzureRegression"
$HelperRootPath = "$InvocationPath\..\..\..\"
$TestSuiteRootPath = "$InvocationPath\..\..\..\..\"
Push-Location "$InvocationPath"
Write-Host "AzureScriptsPath:       $AzureScriptsPath"
Write-Host "TestSuiteRootPath:      $TestSuiteRootPath"
Write-Host "HelperRootPath:         $HelperRootPath"
Write-Host "InvocationPath:         $InvocationPath"

Function Start-LogTranscript {
    $LogFilePath = "$TestSuiteRootPath\TestResults"
    $currentDatetime = Get-Date -format "yyyy-MM-dd_HHmmss"
    if(!(Test-Path $LogFilePath)){
        New-Item -ItemType Directory $LogFilePath
    }

    # Stop the previous transcript
    try {
        Stop-Transcript -ErrorAction SilentlyContinue
    }
    catch [System.InvalidOperationException] {}

    # Start new transcript
    Start-Transcript -Path "$LogFilePath\ManualRun-RegressionUserCSV.ps1_$currentDatetime.log" -Append -Force
}

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

function Check-ConfigureGenerator {
    Write-Host "Check ConfigureGenerator"
    $toolPath = $AzureScriptsPath + "\ConfigureGenerator"
    if(!(Test-Path $toolPath))
    {
        Write-Host "ConfigureGenerator is not existed, please copy those files to $toolPath"
        exit  1
    }

    $templatePath = $AzureScriptsPath + "\ConfigureGenerator\XmlTemplates"
    if(!(Test-Path $templatePath))
    {
        Write-Host "The templates are not existed, please copy those files to $templatePath"
        exit  1
    }

}

function Check-WinteropProtocolTesting {
    Write-Host "Check WinteropProtocolTesting"
    $WinteropProtocolTesting =$HelperRootPath+ "..\..\WinteropProtocolTesting"
    $WinteropProtocolTestingVHD = $WinteropProtocolTesting + "\VHD"
    $WinteropProtocolTestingTools = $WinteropProtocolTesting + "\Tools"
    $WinteropProtocolTestingMedia = $WinteropProtocolTesting + "\Media"
    
    if(!(Test-Path $WinteropProtocolTestingVHD))
    {
        Write-TestSuiteWarning "Please copy VHDs to $WinteropProtocolTestingVHD"
        exit
    }

    if(!(Test-Path $WinteropProtocolTestingTools))
    {
        Write-TestSuiteWarning "Please copy Tools to $WinteropProtocolTestingTools"
        exit
    }

    if(!(Test-Path $WinteropProtocolTestingMedia))
    {
        Write-TestSuiteWarning "Please copy Media to $WinteropProtocolTestingMedia"
        exit
    }

}

# Tool path: RegressionRoot/AzureScripts/
# File structure
# --ConfigureGenerator.exe
# --ConfigureGenerator.exe.config
# --DocumentFormat.OpenXml.dll
# --XmlTemplates
# ----ADFamily.xml
# ----BranchCache.xml
# ----......
function Download-ConfigureGeneratorFromAzureshare{
    Write-Host "Connecting to AzureRm"
    Connect-AzureRmAccount -CertificateThumbprint $thumbPrint -ApplicationId $applicationId -TenantId $tenantId -ErrorAction Stop
    Set-AzureRmCurrentStorageAccount -ResourceGroupName $fileShareResourceGroup -AccountName $storageAccount

    Write-Host "Start download ConfigureGenerator from Azure file share"
    Download-DirectoryFromAzureFileShare
}

function Download-DirectoryFromAzureFileShare {
    param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$directoryName
    )

    $sourcePath = $convertToolRootPath
    $destinationPath = $AzureScriptsPath + "\ConfigureGenerator"
    if($directoryName){
        $sourcePath = $convertToolRootPath + "\" + $directoryName
        $destinationPath = $destinationPath + "\" + $directoryName
    }

    if(!(Test-Path $destinationPath)){
        New-Item -ItemType Directory $destinationPath
    }

    Get-AzureStorageFile -ShareName $storageShareName -Path $sourcePath | Get-AzureStorageFile | ForEach-Object {
        if($directoryName){
            $tempePath = $directoryName + "\" + $_.Name
        }else{
            $tempePath = $_.Name
        }

        if($_.GetType().ToString().EndsWith("CloudFileDirectory")){
            # This is a folder
            Download-DirectoryFromAzureFileShare -directoryName $tempePath
        }else{
            $tempePath = $convertToolRootPath + "\" + $tempePath
            Write-Host "Download file: $tempePath"
            Get-AzureStorageFileContent -ShareName $storageShareName -Path $tempePath -Destination $destinationPath -Force
        }
    }
}

function Generate-EnvrionmentXMLs{
    param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$outputPath
    )

    if(!(Test-Path $configFile)){
    Write-Host "[Error]: No config file" -ForegroundColor Red
    exit
    }

    if(!(Test-Path $outputPath)){
        New-Item -ItemType Directory $outputPath
    }
    $cmd = '& cmd /c "' + $AzureScriptsPath + '\ConfigureGenerator\ConfigureGenerator.exe" "' + $configFile + '" /filter:' + $filter + ' /output:"' + $outputPath + '"'
    Write-Host "Generate xml file from csv file by command: $cmd"
    Invoke-Expression $cmd
}

function Run-Regression{
    param (
        [Parameter(ValueFromPipeline = $True)]
        [System.Collections.Generic.List[String]]$testsuiteList
    )

    $currentRootPath = Get-Location

    Remove-Job -Name * -Force
    $testsuiteList | ForEach-Object {
        $tsName = $_
        $jobName = $tsName
        $configFileFolder = "$InvocationPath\RegressionEnvironments\$tsName"

        # set regressionType value
        $xmlName=Get-ChildItem $configFileFolder -Force "*.xml" -Name | select -First 1
        $xmlPath="$configFileFolder\$xmlName"
        [Xml]$xmlContent = Get-Content $xmlPath
        $regressionType=$xmlContent.lab.core.regressiontype
        Write-Host "RegressionType:$regressionType"

        if($regressionType -eq 'Local')
        {
            Check-WinteropProtocolTesting
        }

        $scriptBlockStr = "$InvocationPath\ManualRun-MultipleEnv.ps1" + `
            ' -TestSuiteName "' + $tsName + `
            '" -ConfigFileFolder "' + $configFileFolder + `
            '" -MsiFolder "' + $msiFolder + `
            '" -subscriptionId "' + $subscriptionId + `
            '" -resourceGroup "' + $resourceGroup + `
            '" -applicationId "' + $applicationId + `
            '" -thumbPrint "' + $thumbPrint + `
            '" -tenantId "' + $tenantId + `
            '" -storageAccount "' + $storageAccount + `
            '" -fileShareResourceGroup "' + $fileShareResourceGroup + `
            '" -tSHelperFolder "' + $tSHelperFolder + `
            '" -regressiontype "' + $regressionType + `
            '" -storageShareName "' + $storageShareName + '"'

        $scriptBlock = [Scriptblock]::Create($scriptBlockStr)
        Write-Host "Start to Run regression for TestSuite: $tsName"
        Start-Job -ScriptBlock $scriptBlock -Name $tsName | Out-Null
        Write-Host "Wait 60 seconds"
        Start-Sleep -Seconds 60
    }

    # wait job complete
    # Start to check job status, exit loop when all jobs completed
    $jobsCount = $testsuiteList.Count
    $completeCount = 0
    $failedCount = 0
    $timeoutCount = 3000; # default timeout is 50 hours

    do {
        Start-Sleep -Seconds 60
        $completeCount = 0
        $failedCount = 0
        $completeCount = (Get-Job | Select-Object -Property Name, State | Where-Object { $_.State -eq "Completed"} | Measure-Object).Count
        $failedCount = (Get-Job | Select-Object -Property Name, State | Where-Object { $_.State -eq "Failed" } | Measure-Object).Count

        Write-Host "Total Jobs: $jobsCount, Completed Jobs Count: $completeCount, Failed Jobs Count: $failedCount"
        $timeoutCount--
    }
    until((($completeCount + $failedCount) -eq $jobsCount) -Or ($timeoutCount -eq 0))

    $failedJobs = Get-Job | Select-Object -Property Name, State | Where-Object { $_.State -eq "Failed" }

    Get-Job | Receive-Job

    foreach($failedJob in $failedJobs){
        $failedReason = $failedJob.JobStateInfo.Reason
        Write-Warning "Job: $($failedJob.Name) failed, Reason: $failedReason"
    }
    $env:END_TIME = (Get-Date).ToString()
    Write-Host "All jobs completed"

    Pop-Location
}

function Main{
    Start-LogTranscript   

    # Step 1: Check ConfigureGenerator tool
    Check-ConfigureGenerator

    # Step 2: Call ConfigureGenerator.exe, convert csv/excel to xmls
    $outPutConfigFiles = "$InvocationPath\RegressionEnvironments"

    # Clean Up existed config files
    if(Test-Path $outPutConfigFiles)
    {
        Remove-Item $outPutConfigFiles -Recurse
    }
    
    Generate-EnvrionmentXMLs -outputPath $outPutConfigFiles

    if(!$needRunRegression)
    {
        exit
    }

    # get test suite names
    [System.Collections.Generic.List[String]]$testsuiteList = New-Object 'System.Collections.Generic.List[String]'
    Get-ChildItem -Path $outPutConfigFiles | Where-Object { $_.PSIsContainer } | ForEach-Object {
        Write-Host "Find TestSuite $($_.Name)"
        $testsuiteList.Add($_.Name)
    }

    $TestSuiteDestPath = Join-Path "$TestSuiteRootPath" -ChildPath "TestSuites" | Split-Path -parent 
    Write-Host "Dest path of Copy: $TestSuiteDestPath"
    Copy-Item -Path "$HelperRootPath\TestSuites" -Dest $TestSuiteDestPath -Recurse -Force

    Push-Location "$InvocationPath\..\..\"

    # Call ManualRun-MultipleEnv.ps1
    Run-Regression -testsuiteList $testsuiteList

    Write-Host "ManualRun-RegressionUserCSV.ps1 execute completed"
    Stop-Transcript
}

Main

Pop-Location
