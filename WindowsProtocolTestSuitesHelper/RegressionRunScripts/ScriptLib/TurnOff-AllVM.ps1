#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           TurnOff-AllVM.ps1
## Purpose:        Turns off all existing virtual machine in Hyper-V.
## Version:        1.1 (12 Oct, 2009)
##
##############################################################################

#----------------------------------------------------------------------------
# NO PARAM
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [TurnOff-AllVM.ps1]..." -foregroundcolor cyan
Write-Host "NO PARAM for this script" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script turns off all existing virtual machine in Hyper-V."
    Write-host
    Write-host "Example: TurnOff-AllVM.ps1"
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
    }

    $isSuccessful = $true    
}
else
{
    Write-Host "All VMs in Hyper-V are turned off ..."
}

Write-Host "EXECUTE [Turnoff-AllVM.ps1] SUCCEED(Not Verify)." -foregroundcolor green

exit