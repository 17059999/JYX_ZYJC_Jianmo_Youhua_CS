#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-Queue.ps1
## Purpose:        Create a message queue
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$pathName,
[string]$label
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-Queue.ps1] ..." -foregroundcolor cyan
Write-Host "`$pathName = $pathName"
Write-Host "`$label = $label"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Create a queue."
    Write-host "Parm1: Path of the Queue. (Required)"
    Write-host "Parm2: Queue's Lable name. (Required)"
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
if ($pathName -eq $null -or $pathName -eq "")
{
    Throw "Parameter `$pathName is required."
}
if ($label -eq $null -or $label -eq "")
{
    Throw "Parameter `$label is required."
}

#----------------------------------------------------------------------------
# Create the queue
#----------------------------------------------------------------------------
$dirFound = $False
#$objDirSet = Get-WmiObject Win32_Directory
#foreach($objDir in $objDirSet)
#{
#    if($objDir.Name -eq $PathName)
#    {
#        $dirFound = $True
#        break
#    }
#}

#if($dirFound -eq $True)
#{
    $queue = new-object -comobject MSMQ.MSMQQueueInfo
    $queue.PathName = $pathName
    #The MSMQQueueInfo.PathName property tells Message Queuing where to store the messages of the queue, whether the queue is public or private, and the name of the queue.

    $queue.Label = $label
    $queue.Create()
    #function create has no return value
    Write-Host "Queue $label has been successfully created!" -foregroundcolor Green
#}
#else
#{
#    Write-Host "Directory doen not exist, queue can not be Created!" -foregroundcolor Red
#}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Create-Queue.ps1] ..." -foregroundcolor Yellow
Write-Host "EXECUTE [Create-Queue.ps1] SUCCEED." -foregroundcolor Green
