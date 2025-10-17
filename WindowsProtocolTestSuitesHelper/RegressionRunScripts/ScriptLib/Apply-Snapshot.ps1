#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Apply-Snapshot.ps1
## Purpose:        Apply a snapshot on VM.
## Version:        1.0 (28 Oct, 2008)
##
##############################################################################

param(
[string]$VMName,
[string]$snapshotID,
[int]$timeoutSec = 600
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Apply-Snapshot]..." -foregroundcolor cyan
Write-Host "`$VMName     = $VMName" 
Write-Host "`$snapshotID = $snapshotID" 
Write-Host "`$timeoutSec = $timeoutSec" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Apply a snapshot on VM."
    Write-host "Param1: Virtual Machine name on which to apply a snapshot. (Required)"
    Write-host "Param2: Snapshot ID to apply. (Required). Snapshot ID can be got from Take-Snapshot.ps1."
    Write-host "Param3: Timeout seconds. (Optional. Default value: 600)"
    Write-host
    Write-host "Example1: Apply-Snapshot  `"W2K8-x86-01`"  `"Microsoft:7CB64D9E-FB14-4B7C-A3E8-8645E38F0860`""
    Write-host "                Apply snapshot `"Microsoft:7CB64D9E-FB14-4B7C-A3E8-8645E38F0860`" on VM `"W2K8-x86-01`"."
    Write-host
}

function Get-JobID([string]$inParam)
{
    if (($inParam -eq $null) -or ($inParam.LastIndexOf("InstanceID=") -eq -1))
    {
        return ""
    }
    $index = $inParam.LastIndexOf("InstanceID=")
    return $inParam.Substring($index + 12, 36)
}

function Change-VMState($machine, [int]$state, [int]$timeout)
{    
    Start-Sleep -Seconds 10
    $result = $machine.RequestStateChange($state)
    if ($result.ReturnValue -ne 4096)
    {
        return $true
    }
    $jobID = Get-JobID $result.Job

    [int]$retryCount = $timeout / 5
    $flag = $false
    
    while (($retryCount -ge 0) -and ($flag -eq $false))
    {
        $job = get-wmiobject -namespace root\virtualization Msvm_ConcreteJob | where {$_.InstanceID -like $jobID}
        if ($job -eq $null)
        {
            Write-Host "Cannot access state of job $jobID." -ForegroundColor Red
            return $false
        }
        
        if ($job.JobState -ne 7)    # Job haven't finish yet
        {            
            $retryCount = $retryCount - 1
            Write-Host "." -NoNewline
            Start-Sleep -Seconds 5
        }
        else    # Job finished
        {            
            $flag = $true
        }
    }
    Write-Host "."
    Start-Sleep -Seconds 5
    
    return $flag
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
if ($VMName -eq $null -or $VMName -eq "")
{
    Throw "Parameter `$VMName is required."
}
if ($snapshotID -eq $null -or $snapshotID -eq "")
{
    Throw "Parameter `$snapshotID is required."
}
if ($timeoutSec -lt 5)
{
    Throw "Parameter `$timeoutSec must greater than 5."
}

#----------------------------------------------------------------------------
# Get VM
#----------------------------------------------------------------------------
$vsManager = get-wmiobject -namespace root\virtualization Msvm_VirtualSystemManagementService

$VM = get-wmiobject -namespace root\virtualization Msvm_ComputerSystem | where {$_.ElementName -like $VMName}
if ($VM -eq $null)
{
    Throw "Specified Virtual Machine do not exist."
}

#----------------------------------------------------------------------------
# Get the snapshot 
#----------------------------------------------------------------------------
$snapshots = get-wmiobject -namespace root\virtualization -query "Associators Of {$VM} Where AssocClass=Msvm_ElementSettingData ResultClass=Msvm_VirtualSystemSettingData"
$snapshot  = $null
foreach($elem in $snapshots)
{
    if ($elem.InstanceID -like "*$snapshotID*")
    {
        $snapshot = $elem
        break
    }
}

if ($snapshot -eq $null)
{
    Throw "Cannot find the snapshot $snapshotID"
}

$snapshotName = $snapshot.ElementName
Write-Host "Snapshot Name: $snapshotName" -ForegroundColor Yellow

#----------------------------------------------------------------------------
# Apply the snapshot
#----------------------------------------------------------------------------
Write-Host "Turning off VM ..."
$ret = Change-VMState $VM  3  $timeoutSec
if ($ret -eq $false)
{
    Throw "Cannot turn off VM"
}

Write-Host "Applying snapshot ..."
$ret = $vsManager.ApplyVirtualSystemSnapshot($VM, $snapshot)
if ($ret.ReturnValue -ne 0)
{
    Throw "Apply snapshot failed."
}

Write-Host "Turning on VM ..."
$ret = Change-VMState $VM  2  $timeoutSec
if ($ret -eq $false)
{
    Throw "Cannot turn on VM"
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Apply-Snapshot] SUCCEED." -foregroundcolor green
