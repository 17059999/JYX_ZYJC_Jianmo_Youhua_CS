#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Take-Snapshot.ps1
## Purpose:        Take a snapshot on VM.
## Version:        1.0 (28 Oct, 2008)
##
##############################################################################

param(
[string]$VMName,
[ref]$snapshotID,
[int]$timeoutSec = 600
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Take-Snapshot]..." -foregroundcolor cyan
Write-Host "`$VMName     = $VMName" 
Write-Host "`$timeoutSec = $timeoutSec" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Take a snapshot on VM."
    Write-host "Param1: Virtual Machine name to be taken snapshot. (Required)"
    Write-host "Param2: Snapshot ID. This is a ref parameter used to get the return value. (Required)"
    Write-host "Param3: Timeout seconds. (Optional. Default value: 600)"
    Write-host
    Write-host "Example1: Take-Snapshot  W2K8-x86-01 `$createdSnapshotID "
    Write-host "                Take a snapshot on VM W2K8-x86-01. The returned snapshot ID is assigned to `$createdSnapshotID."
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
if ($VMName -eq $null -or $VMName -eq "")
{
    Throw "Parameter `$VMName is required."
}
if ($timeoutSec -lt 5)
{
    Throw "Parameter `$timeoutSec must greater than 5."
}

Write-Host "Taking snapshot on $VMName ..."
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
# Take a snapshot 
#----------------------------------------------------------------------------
$result = $vsManager.CreateVirtualSystemSnapshot($VM)
$index = $result.Job.LastIndexOf("InstanceID=")
$jobID = $result.Job.Substring($index + 12, $result.Job.Length - $index - 13)

Start-Sleep -Seconds 10
[int]$retryCount = $timeoutSec / 5
$flag = $false

while (($retryCount -ge 0) -and ($flag -eq $false))
{
    $job = get-wmiobject -namespace root\virtualization Msvm_ConcreteJob | where {$_.InstanceID -like $jobID}
    if ($job -eq $null)
    {
        Throw "Cannot create a job to take snapshot."
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

if ($flag -eq $false)
{
    Throw "Take snapshot timeout"
}

#----------------------------------------------------------------------------
# Get the snapshot ID
#----------------------------------------------------------------------------
$snapshots = get-wmiobject -namespace root\virtualization -query "Associators Of {$VM} Where AssocClass=Msvm_ElementSettingData ResultClass=Msvm_VirtualSystemSettingData"
$snapshot  = $null
if ($snapshots.gettype().isArray -eq $false)
{
    $snapshot = $snapshots
}
else
{
    $snapshot = $snapshots[0]
    for($i = 1; $i -lt $snapshots.Length; $i = $i + 1)
    {
        if ($snapshots[$i].CreationTime -gt $snapshot.CreationTime)
        {
            $snapshot = $snapshots[$i]
        }
    }
}
$snapshotID.Value = $snapshot.InstanceID

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Take-Snapshot]..." -foregroundcolor yellow
if ($snapshotID.Value -eq $null)
{
    Throw "EXECUTE [Take-Snapshot] FAILED."
}
else
{
    Write-Host "EXECUTE [Take-Snapshot] SUCCEED." -foregroundcolor green
}

