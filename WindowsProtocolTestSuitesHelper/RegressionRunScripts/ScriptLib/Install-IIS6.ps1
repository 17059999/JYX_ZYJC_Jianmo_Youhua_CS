##############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-IIS6.ps1
## Purpose:        Install IIS6
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$DLLPath = $null, 
[string]$AnswerFile = $null
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Install-IIS6.ps1] ..." -foregroundcolor cyan
Write-Host "`$DLLPath    = $DLLPath"
Write-Host "`$AnswerFile = $AnswerFile"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Install IIS6 on Windows 2003/Windows XP"
    Write-host
    Write-host "Example: Install-IIS6.ps1 D: AnswerFile.txt"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($DLLPath -eq $null -or $DLLPath -eq "")
{
    Throw "Parameter DLLPath is required."
}

if ($AnswerFile -eq $null -or $AnswerFile -eq "")
{
    Throw "Parameter AnswerFile is required."
}

#----------------------------------------------------------------------------
# Check if the computer is XP or 2k3
#----------------------------------------------------------------------------
$OS = Get-WmiObject -Class win32_OperatingSystem
if(($OS.Version -ne "5.1.2600") -and ($OS.Version -ne "5.2.3790"))
{
    Write-Host "Only Windows XP and WinServer 2003 are supported by far"
    return
}

Write-Host "Check OS succeed"

#----------------------------------------------------------------------------
# Some flag settings
#----------------------------------------------------------------------------
$NewPathValid = $False
$OldPathValid = $False
$AnswerFileValid = $False

#----------------------------------------------------------------------------
# Store the old setup path
#----------------------------------------------------------------------------
$OldSPath = Get-ItemProperty -path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Setup -name SourcePath

#----------------------------------------------------------------------------
# Check if both old and new setup paths are valid
#----------------------------------------------------------------------------
if($DLLPath -ne $null -and $DLLPath -ne "")
{
    if(Test-Path $DLLPath)
    {
        $NewPathValid = $True
    }
}
if(Test-Path $oldSPath.SourcePath)
{
    $OldPathValid = $True
}

if(!$NewPathValid -and !$OldPathValid)
{
    Write-Host "No valid path is provided, installation fail!"
    return
}

Write-Host "Check Setup Registry succeed"

#----------------------------------------------------------------------------
# Check if valid answer file is provided. If not, create a default answer file
#----------------------------------------------------------------------------
if($AnswerFile -ne $null -and $AnswerFile -ne "")
{
    if(Test-Path $AnswerFile)
    {
        $AnswerFileValid = $True
    }
}

if(!$AnswerFileValid)
{
    $AnswerFile = ".\iis_is.txt"
$AnswerFileContent = "[Components]
iis_common = on
iis_inetmgr = on
iis_www = on
iis_ftp = off
iis_htmla = off
aspnet = on
iis_asp = on
"
    $AnswerFileContent | Out-File $AnswerFile
}

Write-Host "Check Answer file succeed"

#----------------------------------------------------------------------------
# If new setup path is provided, use the new one
#----------------------------------------------------------------------------
if($NewPathValid)
{
    Set-ItemProperty -path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Setup -name ServicePackSourcePath -value $DLLPath
    Set-ItemProperty -path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Setup -name SourcePath -value $DLLPath
}

#----------------------------------------------------------------------------
# Execute command to install IIS
#----------------------------------------------------------------------------
cmd.exe /c "sysocmgr /i:%systemdrive%\windows\inf\sysoc.inf /u:$AnswerFile" 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Reset status
#----------------------------------------------------------------------------
if($NewPathValid)
{
    Set-ItemProperty -path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Setup -name ServicePackSourcePath -value $OldSPSPath.ServicePackSourcePath
    Set-ItemProperty -path HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Setup -name SourcePath -value $OldSPath.SourcePath
}
if(!$AnswerFileValid)
{
    Remove-Item $AnswerFile
}

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Install-IIS6.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit