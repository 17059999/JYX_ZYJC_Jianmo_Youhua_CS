#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-MSMQ.ps1
## Purpose:        Install windows component MSMQ.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$infFile,
[string]$comFile
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Install-MSMQ.ps1] ..." -foregroundcolor cyan
Write-Host "`$infFile = $infFile"
Write-Host "`$comFile = $comFile"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Accept the certificate issued by CA."
    Write-host "Parm1: INF file to install MSMQ. (Required)"
    Write-host "Parm2: Component file to install MSMQ. (Required)"
    Write-host
    Write-host "Example: Install-MSMQ.ps1  msmq.inf  component.txt"
    Write-host
    Write-host "Note: This script is used in WinXP and WindowsServer2003."
    Write-host "      MSMQ is defaultly installed in Vista and WindowsServer2008."
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
if ($infFile -eq $null -or $infFile -eq "")
{
    Throw "Parameter `$infFile is required."
}
if ($comFile -eq $null -or $comFile -eq "")
{
    Throw "Parameter `$comFile is required."
}

#----------------------------------------------------------------------------
# Installation
#----------------------------------------------------------------------------
$infExist = Get-Item $infFile
$comExist = Get-Item $comFile
if($infExist -eq $null -or $comExist -eq $null)
{
    Throw "Cannot find inf file or com file, installation of MSMQ failed!"
}
cmd.exe /c %systemroot%\system32\sysocmgr.exe /i:$infFile /u:$comFile 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Install-MSMQ.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Install-MSMQ.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Yellow
