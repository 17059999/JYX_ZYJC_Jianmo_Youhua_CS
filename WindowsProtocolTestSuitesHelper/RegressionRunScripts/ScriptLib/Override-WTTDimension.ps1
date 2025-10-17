#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##
## Microsoft Windows Powershell Scripting
## File:           Override-WTTDimension.ps1
## Purpose:        Override WTT\OSBinRoot and WTT\BuildLabString if they were set incorrectly.
## Requirements:   Windows Powershell 3.0
## Supported OS:   Windows Server 2012
##
##############################################################################

param([string]$logPath="C:\WinteropProtocolTesting\TestResults")

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
if (!(Test-Path -Path $logPath))
{
    CMD /C MKDIR $logPath 
}
Stop-Transcript -ErrorAction Continue | Out-Null
Start-Transcript -Path "$logPath\Override-WTTDimension.ps1.log" -Append -Force

#----------------------------------------------------------------------------
# Override WTT\BuildLabString from WTT\OSBinRoot
#----------------------------------------------------------------------------
$existingOsBinRoot = WTTCMD /ConfigReg /Query /Value:WTT\OSBinRoot
Write-Host "Existing WTT\OSBinRoot: $existingOsBinRoot"

# Winblue will use winblue_refresh and Win8 will use win8_ldr
if(($existingOsBinRoot -ne $null) -and ($existingOsBinRoot -notlike "*winblue*"))
{
    Write-Host "Use existing WTT\OSBinRoot value to build WTT\BuildLabString"
    $osBinRoot = $existingOsBinRoot
}
else
{
    Write-Host "Getting WTT\OSBinRoot value from unattend.xml"
    #----------------------------------------------------------------------------
    # Get unattend.xml
    #----------------------------------------------------------------------------
    $unattendFile = "$env:SystemDrive\`$AsiTemp`$\unattend.xml"
    if(!(Test-Path $unattendFile))
    {
        Write-Host "Cannot find answerfile $unattendFile" -ForegroundColor Red
        exit 1
    }

    #----------------------------------------------------------------------------
    # Get value for WTT\OSBinRoot
    #----------------------------------------------------------------------------
    [xml]$unattendContent = Get-content -LiteralPath $unattendFile
    $setupComponent = $unattendContent.unattend.settings.component | where {$_.name -eq "Microsoft-Windows-Setup"}

    # Get setup path, e.g. \\ntdev.corp.microsoft.com\MainLab_FinalMilestone\winblue_refresh\9600.16468.131113-1345\amd64fre\media\server_en-us\sources\install.wim
    $setupPath = $setupComponent.ImageInstall.osimage.InstallFrom.Path
    if($setupPath -eq $null -or $setupPath -eq "")
    {
        Write-Host "Cannot find OS image setup path."
        exit 1
    }
    else
    {
        Write-Host "OS image setup path: $setupPath"
    }

    # Get WTT\OSBinRoot from setup path
    # Build Path, e.g. \\ntdev.corp.microsoft.com\MainLab_FinalMilestone\winblue_refresh\9600.16468.131113-1345\amd64fre
    $buildPath = $setupPath.Substring(0,$setupPath.IndexOf("media"))
    # BuildType, e.g. amd64fre
    $buildType = Split-Path $buildPath -Leaf 
    # OSBinRoot, e.g. \\ntdev.corp.microsoft.com\MainLab_FinalMilestone\winblue_refresh\9600.16468.131113-1345\amd64fre\bin
    $osBinRoot = $buildPath + "bin" 
    
    Write-Host "Expected WTT\OsBinRoot: $osBinRoot"
    if($osBinRoot -ne $null)
    {
        # Override WTT\OSBinRoot
        Write-Host "Override WTT\OSBinRoot"
        Write-Host "Remove existing WTT\OSBuildRoot"
        CMD /C WTTCMD /ConfigReg /Remove /Value:WTT\OSBinRoot

        Write-Host "Add expected WTT\OSBuildRoot"
        CMD /C WTTCMD /ConfigReg /Add /Value:WTT\OSBinRoot /Data:"$osBinRoot"
    }
    else
    {
        Write-Host "Cannot get WTT\OSBinRoot from unattend.xml."
        exit 1
    }  
}

# Assemble WTT\BuildLabString from OSBinRoot
$arrPath = $osBinRoot.Split("\")
# Build Type, e.g. amd64fre
$buildType = $arrPath[$arrPath.Count -2] 
# Build Folder, e.g. 9600.16468.131113-1345
$buildFolder = $arrPath[$arrPath.Count -3]
# OS Lab, e.g. winblue_refresh
$osLab = $arrPath[$arrPath.Count -4]
$arrBuildInfo = $buildFolder.Split(".")
# BuildLabString, e.g. 9600.16468.amd64fre.winblue_refresh.131113-1345
$buildLabString = $arrBuildInfo[0] + "." + $arrBuildInfo[1] + "." + $buildType + "." + $osLab + "." + $arrBuildInfo[2]
# Override WTT\BuildLabString
$existingBuildLabString = WTTCMD /ConfigReg /Query /Value:WTT\BuildLabString
Write-Host ""
Write-Host "Existing WTT\BuildLabString: $existingBuildLabString"
Write-Host "Expected WTT\BuildLabString: $buildLabString"
if($existingBuildLabString -ne $buildLabString -and $buildLabString -ne $null)
{
    Write-Host "Override WTT\BuildLabString"
    Write-Host "Remove existing WTT\BuildLabString"
    CMD /C WTTCMD /ConfigReg /Remove /Value:WTT\BuildLabString

    Write-Host "Add expected WTT\BuildLabString"
    CMD /C WTTCMD /ConfigReg /Add /Value:WTT\BuildLabString /Data:"$buildLabString"
}
else
{
    Write-Host "Existing WTT\BuildLabString $existingBuildLabString is expected, no need to override."
}

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Stop-Transcript -ErrorAction Continue
exit 0