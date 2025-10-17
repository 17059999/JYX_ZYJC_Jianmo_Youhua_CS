#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Delete-AllVM.ps1
## Purpose:        Turns off and removes all existing virtual machine in Hyper-V.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Delete-AllVM.ps1]..." -foregroundcolor cyan
Write-Host "NO PARAM for this script" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script turns off and removes all existing virtual machine in Hyper-V."
    Write-host
    Write-host "Example: DeleteAllVM.ps1"
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

Write-Host "Start removing all VM ..." 

#----------------------------------------------------------------------------
# Get a list of virtual machines, whether it's in running state.
#----------------------------------------------------------------------------
$VMSvr = Get-WmiObject -namespace "Root\Virtualization" "Msvm_VirtualSystemManagementService"
$VMList = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine'"

$isSuccessful = $true
if($VMList -ne $null)
{
    $isSuccessful = $false

    #----------------------------------------------------------------------------
    # Turnoff VM
    #----------------------------------------------------------------------------
    foreach($VM in $VMList)
    {
        $VMName =$VM.ElementName
        Write-Host "Turning off $VMName ..." 
        $ret = $VM.RequestStateChange(3)
        if ($ret.Job -ne $null)
        {
            .\Wait-VMTask.ps1 $ret.Job
        }
        
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
    }

    #----------------------------------------------------------------------------
    #Destroy VM
    #----------------------------------------------------------------------------
    foreach($VM in $VMList)
    {
        $VMName =$VM.ElementName
        Write-Host "Destorying $VMName ..." 
        $ret = $VMSvr.DestroyVirtualSystem($VM)
        .\Wait-VMTask.ps1 $ret.Job
    }

    #----------------------------------------------------------------------------
    # Confirm all VMs are Destoryed/removed from HyperV
    #----------------------------------------------------------------------------
    $timeoutSec = 600
    for ($retryCount = 0; $retryCount -lt $timeoutSec/5; $retryCount++) 
    {
        #----------------------------------------------------------------------------
        # Check if there's still a VM existing.
        #----------------------------------------------------------------------------
        $VMList = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine'"
        if ($VMList -eq $null)
        {
            $isSuccessful = $true
            break
        }
        Start-Sleep -s 5
    }
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Delete-AllVM.ps1]..." 
if ($isSuccessful -ne $true )
{
    Throw "EXECUTE [Delete-AllVM.ps1] FAILED."
}
else
{
    Write-Host "EXECUTE [Delete-AllVM.ps1] SUCCEED." -foregroundcolor green
}

exit