#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:            Add-ServerRole.Ps1
## Purpose:         Add Server role
## Version:         1.0 (July 14th 2008)
##
########################################################################################################################

Param(
[string]$ServerRole,
[string]$LogFile
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Add-ServerRole.Ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
Function Show-ScriptUsage
{    
    Write-host    
    Write-host "Usage:                       `t:This PS1 script isused to add Server role"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First Parameter              `t:$ServerRole : Server Role Name"            
    Write-host "Second Parameter             `t:$LogFile : Install or uninstall log file location "           
    Write-host "Example:"
    Write-host "Add-ServerRole.Ps1 print-server c:\test\testresults\serverlog.txt "
    Write-host 
}

#----------------------------------------------------------------------------
#Verify Help parameters
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    write-host
    show-scriptUsage 
    return
}

#----------------------------------------------------------------------------
#Check for input parameters
#----------------------------------------------------------------------------
If (!$ServerRole)
{
    Write-host "Please specify the Server role!" -foregroundcolor Red
    Show-ScriptUsage
    return
}
If (!$LogFile)
{
    Write-host "Please specify the loglocation for server role !" -foregroundcolor Red
    Show-ScriptUsage
    return
}

#----------------------------------------------------------------------------
#Server ROLE Install
#----------------------------------------------------------------------------
$Flag=$False
$Status = servermanagercmd.exe -install $serverrole -allsubfeatures -resultpath $Logfile

#----------------------------------------------------------------------------
#Install Validation
#----------------------------------------------------------------------------
if ($Status -match "NoChange:")
{
    write-host "Specified Server role is already installed " -foregroundcolor red
    $Flag=$True
}
if ($Status -match "Success: Installation succeeded")
{
   write-host "$serverrole Serverrole Installed Sucessfully" -foregroundcolor green
   $statusflag=$True
}
if ($Statusflag -eq $False)
{
   write-host "Server Role installation failed.Please refer to the log file at $logfile " -foregroundcolor red
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Add-ServerRole.Ps1] SUCCEED." -foregroundcolor Green


