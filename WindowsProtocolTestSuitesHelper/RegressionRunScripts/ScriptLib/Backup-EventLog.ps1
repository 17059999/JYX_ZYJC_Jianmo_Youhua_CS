#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Backup-EventLog.Ps1
## Purpose:        Backup specified eventlogs to a file.
## Version:        1.0 
##
########################################################################################################################

Param(
$eventLogName,
$logLocation
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Backup-EventLog.ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
  
function Show-ScriptUsage
{    
    Write-host    
    Write-host "Usage:          This PS1 script will backup specified eventlog to a file"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "Parameter one:  eventLogName: Target Eventlog name"
    Write-host "Parameter two:  logLocation : Location to back up the target eventlog"
    Write-host "Example:"
    Write-host "Eventlog_Backup.Ps1 Application c:\eventlogbackup\application.txt"
    Write-host
}

#----------------------------------------------------------------------------
#Verify Help parameters
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    write-host 
    write-host
    show-scriptusage 
    return
}

#----------------------------------------------------------------------------
#Check for input parameters
#-----------------------------------------------------------------------------
if(!$eventLogName)
{
    Write-host
    Write-host "Please specify the Eventlog name!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

if(!$logLocation)
{
    Write-host
    Write-host "Please specify the target Eventlog backup file location!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

#----------------------------------------------------------------------------
#Check for valid Input parameters 
#-----------------------------------------------------------------------------
$Event= GET-WMIOBJECT -QUERY "SELECT * FROM WIN32_NTEventLogFile WHERE LOGFILENAME='$eventLogName'"

If($Event.logfilename -eq $eventLogName)
{ 
    $IfParameterValid = $False
    $TempDrivers = $logLocation.Split(':')[0]
    $Drive = Get-psdrive

    Foreach ($Drv in $Drive)
    {
        if ($drv.name -eq $TempDrivers)
        {
            $IfParametervalid = $True
        }
    }
}
else
{
    Write-host "Please specify the valid eventLogName" -foregroundcolor Red
}

#----------------------------------------------------------------------------
#If parameters are valid ,backup the eventlog to Check for valid Input parameters 
#-----------------------------------------------------------------------------
If($IfParameterValid -eq $True)
{
    Get-eventlog $eventLogName | out-file $logLocation
    Write-host "Sucessfully copied $eventLogName EventLog to the $logLocation" -foregroundcolor green
}

if($IfParametervalid -eq $False)
{
    Write-host "Please specify the valid log location path" -foregroundcolor Red
}

#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Backup-EventLog.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit 0






