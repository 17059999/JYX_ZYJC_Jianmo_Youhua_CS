#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Start-MIPConfiguration.ps1
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 7 / Windows 2008 R2
##
##############################################################################

Param(
[string]$testDirInVM     = "C:\Test",
[string]$targetScript    = "",
[string]$osType    = "Windows"
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Start-MIPConfiguration.ps1]..." -foregroundcolor cyan
Write-Host "`$testDirInVM    = $testDirInVM"
Write-Host "`$targetScript   = $targetScript"

$testResultsPath="$testDirInVM/TestResults"
if (!(Test-Path -Path $testResultsPath) )
{
    New-Item -Type Directory -Path $testResultsPath -Force
}
$logFile = $testResultsPath + "/Start-MIPConfiguration.ps1.log"
if (!(Test-Path -Path $logFile))
{
    New-Item -Type File -Path $logFile -Force
}
Start-Transcript $logFile -Append
#----------------------------------------------------------------------------
#
#  Function: Replace Pause in the scripts.
#  Parameter:
#           $contentPath:The file's full path
#
#----------------------------------------------------------------------------
Function ReplacePause
(
    [string]$contentPath
)
{
    [bool]$IsReplaced = $False 
    write-host "`$contentPath is $contentPath"
    $scriptsContents = Get-Content $contentPath
    $tempscriptsContents = ""
    foreach($scriptscontent in $scriptsContents)
    {
        #Only Replace "cmd /c pause" and "Pause"
        if($scriptscontent.ToLower().Contains(" pause") -or ($scriptscontent.ToLower().Trim() -eq "pause") )
        {
            $IsReplaced = $True
            $scriptscontent = ""
        }
        $tempscriptsContents = $tempscriptsContents + $scriptscontent + "`r`n"
    }
    #When the content is changed,set the content. 
    if($IsReplaced)
    {
        Set-Content $contentPath $tempscriptsContents
    }
}

Write-Host "Get the scripts path from MSIInstalled.signal file"
$signalFile  = "$env:HOMEDRIVE/MSIInstalled.signal"
if (Test-Path -Path $signalFile)
{
    $TestSuiteScriptsFullPath = Get-Content $signalFile
}
else
{
    Write-Host "MSI has not been installed. please check"
    exit 0
}

Write-Host "Replace pause in the scripts"
$scripts = Get-ChildItem $TestSuiteScriptsFullPath
foreach($script in $scripts)
{
    if (($script.Attributes -ne "Directory") -and ($script.Name.EndsWith(".ps1") -or $script.Name.EndsWith(".bat") -or $script.Name.EndsWith(".cmd")))
    {
        $scriptPath = $TestSuiteScriptsFullPath + "/" + $script.Name
        ReplacePause $scriptPath
    }
}

$BatchFolder = "$TestSuiteScriptsFullPath/../Batch"
if (Test-Path -Path $BatchFolder)
{
    Write-Host "Replace pause in the batch folder"
    $batches = Get-ChildItem $BatchFolder
    foreach($batch in $batches)
    {
        if (($batch.Attributes-ne "Directory") -and ($batch.Name.EndsWith(".bat") -or $batch.Name.EndsWith(".cmd")))
        {
            $batchPath = $BatchFolder + "/" + $batch.Name
            ReplacePause $batchPath
        }
    }
}

Write-Host "Locate to $TestSuiteScriptsFullPath"
pushd $TestSuiteScriptsFullPath
Write-Host "Run $targetScript to continue the configuration"
$detailtargetScript = $TestSuiteScriptsFullPath + "/" + $targetScript
if($osType -ne "Linux"){
    cmd /c powershell $detailtargetScript 2>&1 | Write-Host
}else{
    pwsh $detailtargetScript 2>&1 | Write-Host
}

exit 0