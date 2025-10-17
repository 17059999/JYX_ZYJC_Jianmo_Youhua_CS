###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Run-TestCase.ps1
## Purpose:        Run test case for specified test suite
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later.
## Input parameter is
##      TestSuiteName           :  Test Suite name
##      configFile              :  Environment xml full path
##      EnvironmentName         :  Environment xml Name
##      subscriptionId          :  Azure Subscriptoion Id
##      storageShareName        :  Azure Storage Share Name
##      fileShareResourceGroup  :  Azure Resource Group for File Share
##      storageAccount          :  Azure Storage Account Name, this account is used to save Tools and Build Files
##      resultStorageAccount    :  Azure Storage Account Name, this account is used to save Testsuite logs
##      toolLibPath             :  Test suite tools path
##      logFilePath             :  test case execute log path
###########################################################################################

Param(
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string] $TestSuiteName,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$configFile,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$EnvironmentName,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$subscriptionId,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$storageShareName,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$fileShareResourceGroup,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$storageAccount,
    [Parameter(ValueFromPipeline = $True)]
    [string]$resultStorageAccount,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$toolLibPath,
    [string]$logFilePath,
    [Parameter(ValueFromPipeline = $True)]
    [string]$TargetedCategoryName = "",
    [Parameter(ValueFromPipeline = $True)]
    [string]$TargetedTestName = "",
    [Parameter(ValueFromPipeline = $True)]
    [string]$runTests = "true"

)

Write-Host "runTests : $runTests"
$Global:StartTime = [System.dateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss")

$scriptPath             = Split-Path $MyInvocation.MyCommand.Definition -parent
$TestSuiteRootPath      = "$scriptPath\..\.."
$TSHelperFolder         = "$scriptPath\.."
$testSuitePath          = "$TestSuiteName"
$RegressionRootPath     = "$scriptPath\.."

Write-Host "TestSuiteName : $TestSuiteName"
Write-Host "configFile : $configFile"
Write-Host "EnvironmentName : $EnvironmentName"
Write-Host "subscriptionId : $subscriptionId"
Write-Host "scriptPath : $scriptPath"

if($TestSuiteName.Equals("RDPServer")){
    $testSuitePath = "RDP\Server"
}
if($TestSuiteName.Equals("RDPClient")){
    $testSuitePath = "RDP\Client"
}

Push-Location $scriptPath

Function Write-TestSuiteInfo {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [string]$ForegroundColor = "White",
        [string]$BackgroundColor = "DarkBlue")

    
    Write-Host ((Get-Date).ToString() + ": $Message") -ForegroundColor $ForegroundColor -BackgroundColor $BackgroundColor
}

#------------------------------------------------------------------------------------------
# Read and parse XML configuration file
# $Setup will be used as a global variable to storage the configuration information
#------------------------------------------------------------------------------------------
Function Read-TestSuiteXml {

    Write-TestSuiteInfo "Read and parse the XML configuration file."

    Write-TestSuiteStep "Check if the XML configuration file exist or not."
    # If $XmlFileFullPath is not found, prompt a list of choices for user to choose
    if (!(Test-Path -Path $configFile)) {
        Write-TestSuiteError "$configFile file not found."
    }
    else {
        Write-TestSuiteInfo "$configFile file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $configFile
    if ($null -eq $Script:Setup) {
        Write-TestSuiteError "$configFile file is not a valid xml configuration file." -Exit
    }
}

#------------------------------------------------------------------------------------------
# Create Execute Protocol Test task and wait test result
#------------------------------------------------------------------------------------------
Function Run-TestCase {
    $Script:Parameters = $null

    $ParametersNode = $Script:Setup.lab.SelectSingleNode("Parameters")
    if(($null -ne $ParametersNode) -and ($ParametersNode.HasChildNodes))
    {
        $Script:Parameters = @{}
        foreach($arg in $ParametersNode.ChildNodes)
        {
            # Node type is Element and it has no child
            if(($arg.NodeType -eq [Xml.XmlNodeType]::Element) -and (-not $arg.HasChildNodes))
            {
                $Parameters.Add($arg.Name, $arg.Value)
            }
        }
    }

    # change to execute-protocoltest path
    Push-Location "$RegressionRootPath\ProtocolTestSuite\$TestSuiteName\Scripts"
    $currentPath = Get-Location
    Write-TestSuiteInfo "Current Folder: $currentPath"

    $CmdLine = '.\Execute-ProtocolTest.ps1 -ProtocolName $TestSuiteName -WorkingDirOnHost $RegressionRootPath -TestResultDirOnHost $logFilePath -EnvironmentName $EnvironmentName -CategoryName "$TargetedCategoryName" -TestName "$TargetedTestName" -runTests "$runTests"'
    $scriptinfo = Get-Command ".\Execute-ProtocolTest.ps1"
    if($null -ne $Parameters)
    {
        foreach($key in $Parameters.Keys)
        {
            if($null -ne $scriptinfo.Parameters[$key])
            {
                $value = $Parameters[$key]
                $CmdLine += " -$key $value"
            }
            else
            {
                Write-TestSuiteWarning "$key is not a valid parameter name for Execute-ProtocolTest.ps1, please check it in lab.Parameters of XML configuration!"
            }
        }
    }
    Write-TestSuiteWarning "Running: $CmdLine"
    Invoke-Expression $CmdLine
    Pop-Location
}

#------------------------------------------------------------------------------------------
# Parse trx result and copy result to azure file share
#------------------------------------------------------------------------------------------
Function Process-TestCaseResult{

    Write-TestSuiteStep "Copy test result to Azure FileShare"
    # Create TestSuite Folder
    $envrionmentFolder = (Get-Item $logFilePath).Name
    $testsuiteFolder = (Get-Item $logFilePath).Parent.Name
    $destinationFolder = "azure-$($testsuiteFolder.ToLower().Replace("_","-"))"
    Write-Host "============================================================"
    Write-Host "envrionmentFolder : $envrionmentFolder"
    Write-Host "testsuiteFolder : $testsuiteFolder"
    Write-Host "storageShareName : $storageShareName"
    Write-Host "storageAccount : $storageAccount"
    Write-Host "resultStorageAccount : $resultStorageAccount"
    Write-Host "============================================================"

    $storageAccountKeys = Get-AzStorageAccountKey -ResourceGroupName $fileShareResourceGroup -Name $resultStorageAccount
    if($storageAccountKeys.Count -gt 0){
        
        $context = New-AzStorageContext -StorageAccountName $resultStorageAccount -StorageAccountKey $storageAccountKeys[0].Value

        Write-Host "Create share container : $destinationFolder"
        New-AzStorageContainer -Name $destinationFolder -Context $context
        
        Get-ChildItem -Path "$logFilePath\..\..\$testsuiteFolder" -File -Recurse | Set-AzStorageBlobContent -Container $destinationFolder -Context $context
    }
    else{
        Write-Error "Cannot find StorageAccountKey!" 
    }
    
    Write-TestSuiteStep "Parse test result and create corresponding json files"

    Write-TestSuiteInfo "Log file Path: $logFilePath"
    if(Test-Path -Path $logFilePath)
    {
        $outFileName = [System.IO.Path]::GetFileNameWithoutExtension($configFile)
        $jsonPath = [string]::Format("{0}\{1}", $logFilePath, "$outFileName.json")

        $trxList = Get-ChildItem $logFilePath -Filter "*.trx"
        if($null -ne $trxList -and $trxList.Count -gt 0)
        {
            # Create regression result info to json file
            & "$RegressionRootPath\ScriptLib\Generate-RegressionInfo.ps1" -logFilePath $logFilePath -configFile $configFile -EnvironmentName $EnvironmentName -blobContainerName $destinationFolder
        }else{
            Write-TestSuiteWarning "No trx file found in $logFilePath"
        }

        # archiveArtifacts used hard code path, but $jsonPath here is a dynamic generated path, copy to $RegressionRootPath\TestResults so that jenkins file can get this json
        if($runTests -eq "true")
        {
             Write-Host "Copy json result from $jsonPath to $RegressionRootPath\TestResults"    
        
      
    
            Push-Location $RegressionRootPath\TestResults
            #Remove-Item *.json
            Remove-Item *.trx
        
            Copy-Item $jsonPath -Destination "$RegressionRootPath\TestResults" -Force
            $trxList | Copy-Item -Destination "$RegressionRootPath\TestResults" -Force
            if((Test-Path $RegressionRootPath\TestResults\$outFileName.json) -and ((Get-ChildItem $RegressionRootPath\TestResults*.trx).Count -gt 0)) {
                Write-Host "Copy completed"
            }
            else {
                Write-Host "Copy failed"
            }
        }
       
    }
}

Function Main{

    Read-TestSuiteXml

    $driver = $Script:Setup.lab.servers.vm | where {$_.role -eq "DriverComputer"}
    if($driver -ne $null) {
        Run-TestCase

        Process-TestCaseResult
    }    
}

Write-TestSuiteInfo "TestSuiteRootPath  : $TestSuiteRootPath"
Write-TestSuiteInfo "TSHelperFolder     : $TSHelperFolder"
Write-TestSuiteInfo "scriptPath         : $scriptPath"
Write-TestSuiteInfo "testSuitePath      : $testSuitePath"
Write-TestSuiteInfo "logFilePath        : $logFilePath"


Set-AzCurrentStorageAccount -ResourceGroupName $fileShareResourceGroup -Name $storageAccount



Main

Pop-Location