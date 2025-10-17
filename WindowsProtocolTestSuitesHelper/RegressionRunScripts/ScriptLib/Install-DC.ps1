#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-DC.ps1
## Purpose:        Install domain controller on the server machine.
## Version:        1.1 (26 June, 2008)
##           
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Install-DC.ps1] ..." -foregroundcolor cyan


#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage:             `t:This script installs domain controller on the server machine"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First Parameter          `t:Usage: use -? or -help to get script usage."
    Write-host
    Write-host "Prerequisite:            `t:Please make the necessary changes in DCInstallAnswerFile.txt file"
    Write-host
    Write-host "Example:"
    Write-host "Install_DC.ps1"
    Write-host
}

#----------------------------------------------------------------------------
#Verify Required parameters
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    write-host 
    show-scriptusage 
    return
}

#----------------------------------------------------------------------------
#Function: ExecuteLocalCommand
#
#Usage   : Execute command on cmd console
#----------------------------------------------------------------------------
function ExecuteLocalCommand([string] $command)
{
    cmd.exe /c $command 2>&1 | Write-Host
}


#--------------------------------------------------------------------------------------------
#Setting the impersonation and authentication level.
#--------------------------------------------------------------------------------------------
$objWMIService = new-object -comobject WbemScripting.SWbemLocator
$objWMIService.Security_.ImpersonationLevel = 3
$objWMIService.Security_.AuthenticationLevel = 6

#--------------------------------------------------------------------------------------------
#Copy answer file to C:
#--------------------------------------------------------------------------------------------
$SystemDrive = [System.Environment]::GetEnvironmentVariable("systemdrive")
copy-item Install-DC-DCInstallAnswerFile.txt  $SystemDrive\DCInstallAnswerFile.txt

#--------------------------------------------------------------------------------------------
#install DC
#--------------------------------------------------------------------------------------------
echo "Start install DC..."
ExecuteLocalCommand("dcpromo.exe /answer:C:\DCInstallAnswerFile.txt")


Write-Host "EXECUTE [Install-DC.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

return 0
