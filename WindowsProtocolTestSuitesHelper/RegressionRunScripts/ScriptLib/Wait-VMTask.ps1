#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Wait-VMTask.ps1
## Purpose:        This script waits a Hyper-V task done.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$taskPath
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Wait-VMTask.ps1]..." -foregroundcolor cyan
Write-Host "`$taskPath = $taskPath" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script waits a Hyper-V task done."
    Write-host
    Write-host "Example: WaitVMTask.ps1 taskPath"
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
if ($taskPath -eq $null -or $taskPath -eq "")
{
    Throw "Parameter taskPath is required."
}

#----------------------------------------------------------------------------
# Function: Task-IsCompleted
# Usage   : Check if the job is completed.
#----------------------------------------------------------------------------
function Task-IsCompleted()
{
    $taskPath = $taskPath.Replace("\", "\\")
    $query = "SELECT * FROM Msvm_ConcreteJob WHERE __PATH='" + $taskPath + "'"
    $job = Get-WmiObject -query $query -namespace "Root\Virtualization"

    $jobState = $job.JobState
    $isCompleted = $false
    if (($jobState -eq 7) -or ($jobState -eq 8) -or ($jobState -eq 9) -or ($jobState -eq 10) -or ($jobState -eq $null))
    {
        $isCompleted = $true
    }

    write-host $job.PercentComplete"% completed (Job State: $jobState)."  
    if (($job.PercentComplete -eq 100) -and ($jobState -eq 32768))
    {
        $isCompleted = $true
        write-host "Job completed with 100% but has errors, please see Server manager, Roles, HyperV, Events for details"  -foregroundcolor YELLOW  
    }

    return $isCompleted
}

Write-Host "Wait until the following task complete ..." 
Write-Host $taskPath

#----------------------------------------------------------------------------
# Check and Wait
#----------------------------------------------------------------------------
While ((Task-IsCompleted) -eq $false)
{
    [System.Threading.Thread]::Sleep(3000)
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Wait-VMTask.ps1] SUCCEED."

exit
