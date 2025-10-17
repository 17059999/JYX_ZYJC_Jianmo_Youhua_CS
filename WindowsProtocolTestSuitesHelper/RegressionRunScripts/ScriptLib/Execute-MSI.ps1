##############################################################################
## 
## Microsoft Windows Powershell Scripting
## File:           Execute-MSI.PS1
## Purpose:        Install/unstall microsoft installer package file (.MSI file)
## Version:        1.1 (26 June 2008)
##
##############################################################################

Param(
[string]$Srclocation,
[string]$install
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Execute-MSI.ps1]..." -foregroundcolor cyan
Write-Host "`$Srclocation = $Srclocation" 
Write-Host "`$install = $install" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This scripts will install/unstall microsoft installer package file (.MSI file)"
    Write-host
    Write-host "Example Install: Execute-MSI.ps1 c:\setup.msi Install"
    Write-host "Example Unstall: Execute-MSI.ps1 c:\setup.msi Unstall"
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
#Verify required parameters
#----------------------------------------------------------------------------
if ($Srclocation -eq $null -or $Srclocation -eq "")
{
    Throw "Enter the Source location of .MSI File !"
    return
}    

if ($install -eq $null -or ($install -ne "Install" -and $install -ne "Unstall"))
{
    $install="Install"
}

#----------------------------------------------------------------------------
# EXECUTION (Installation of MSI file in passive mode, optional restart)
#----------------------------------------------------------------------------
if($install -eq "Install")
{
    msiexec /i $srclocation /PASSIVE /PROMPTRESTART
}
else
{
    msiexec /x $srclocation /PASSIVE /PROMPTRESTART
}

#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Execute-MSI.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit 0