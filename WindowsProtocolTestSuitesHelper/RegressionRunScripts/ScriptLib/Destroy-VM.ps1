#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Destroy-VM.ps1
## Purpose:        Turns off an existing virtual machine in Hyper-V.
## Version:        1.1 (12 Oct, 2009)
##
##############################################################################

param(
[string]$VMName
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Destroy-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script delete an existing virtual machine in Hyper-V."
    Write-host
    Write-host "Example: Destroy-VM.ps1 W2k3-x86-01"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($Args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}
#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($VMName -eq $null -or $VMName -eq "")
{
    Throw "Parameter VMName is required."
}

Write-Host "Start deleting an VM ..." 
#----------------------------------------------------------------------------
# Get VM and destroy VM
#----------------------------------------------------------------------------
$VMSvr = Get-WmiObject -namespace "Root\Virtualization" "Msvm_VirtualSystemManagementService"
$VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' and elementName= '$VMName'"

$isSuccessful = $true
if($VM -ne $null)
{
    #----------------------------------------------------------------------------
    # Move snapshots to a temp place to make the deleting faster
    #----------------------------------------------------------------------------
    $snapshots = Get-WmiObject -Namespace root\virtualization Msvm_VirtualSystemSettingData -Filter "SystemName = `'$($VM.Name)`' and SettingType = 5"
    if ($snapshots -eq $null)
    {
        Write-Host "No Snapshot is found on VM `"$VMName`"" -ForegroundColor Yellow
        continue
    }

    # Get Snapshot location
    $snapshotPath = $null
    foreach ($snapshot in $snapshots)
    {
        $snapshotID = $snapshot.InstanceID
        $settingData = Get-WmiObject -Namespace root\virtualization Msvm_ResourceAllocationSettingData | where {($_.ResourceSubType -like "*Microsoft Virtual Hard Disk*") -and ($_.Parent -like "*$($VM.Name)*")}
        $index = $($settingData.Connection).LastIndexOf("\")
        if ($index -ne -1)
        {
            $snapshotPath = $($settingData.Connection).Substring(0, $index)
            Write-Host "Snapshot Path found: $snapshotPath"
            break
        }
    }

    if ($snapshotPath -eq $null)
    {
        Write-Host "Cannot get snapshot path" -ForegroundColor Yellow
    }
    else
    {
        $snapshotFiles = [System.IO.Directory]::GetFiles($snapshotPath, "*.avhd", [System.IO.SearchOption]::AllDirectories)
        Write-Host "The following snspshots found:"
        $snapshotFiles 
        $tempSnapshotPath = $snapshotPath + "\..\TemSnapShots\"
        if ([System.IO.Directory]::Exists($tempSnapshotPath) -eq $false)
        {
            Write-Host "Create a temp snapshot folder..."
            md $tempSnapshotPath 
        }

        foreach ($snapshotFile in $snapshotFiles)
        {
            $snapshotFileName = $snapshotFile.SubString($snapshotFile.LastIndexOf("\")+1)
            Write-Host "Moving this snapshot: $snapshotFileName "
            Write-Host "To a new temp folder: $tempSnapshotPath "

            $snapshotNewFullPath = $tempSnapshotPath + "\" + $snapshotFileName 
            Move-Item -Path $snapshotFile -Destination $snapshotNewFullPath -Force
        }
    }
    #----------------------------------------------------------------------------
    # Destroy VM
    #----------------------------------------------------------------------------
    Write-Host "Destorying $VMName ..." 
    $ret = $VMSvr.DestroyVirtualSystem($VM)
    .\Wait-VMTask.ps1 $ret.Job    
                
    #----------------------------------------------------------------------------
    # Confirm VM is Destoryed/removed from HyperV
    #----------------------------------------------------------------------------
    $timeoutSec = 600
    for ($retryCount = 0; $retryCount -lt $timeoutSec/5; $retryCount++) 
    {
        #----------------------------------------------------------------------------
        # Check if the VM is still existing.
        #----------------------------------------------------------------------------
        $VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' and elementName= '$VMName'"
        if ($VM -eq $null)
        {
            $isSuccessful = $true
            break
        }
        Start-Sleep -s 5
    }
}
else
{
    Write-Host "$VMName does not exist in Hyper-V" 
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Destroy-VM.ps1]..." 
if ($isSuccessful -ne $true )
{
    Throw "EXECUTE [Destroy-VM.ps1] FAILED."
}
else
{
    Write-Host "EXECUTE [Destroy-VM.ps1] SUCCEED." -foregroundcolor green
}

exit