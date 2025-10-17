#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Remove-VM.ps1
## Purpose:        Turns off and removes an existing virtual machine in Hyper-V.
## Version:        1.0 (10 Nov, 2008)
##
##############################################################################

param(
[string]$VMName
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Remove-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script turns off and removes an existing virtual machine in Hyper-V."
    Write-host
    Write-host "Example: Remove-VM.ps1 W2k3-x86-01"
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
    Throw "Parameter VMName is required."
}

Write-Host "Start removing an VM ..." 
#----------------------------------------------------------------------------
# Get a list of virtual machines, whether it's in running state.
#----------------------------------------------------------------------------
$VMSvr = Get-WmiObject -namespace "Root\Virtualization" "Msvm_VirtualSystemManagementService"
$VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' and elementName= '$VMName'"

$isSuccessful = $true
if($VM -ne $null)
{
    $isSuccessful = $false
	
    #----------------------------------------------------------------------------
    # Turnoff VM
    #----------------------------------------------------------------------------   
    Write-Host "Turning off $VMName ..." 
    $ret = $VM.RequestStateChange(3)
    if ($ret.Job -ne $null)
    {
        .\Wait-VMTask.ps1 $ret.Job
    }   

    #----------------------------------------------------------------------------
    # Destroy VM
    #----------------------------------------------------------------------------   
    Write-Host "Destorying $VMName ..." 
    $ret = $VMSvr.DestroyVirtualSystem($VM)
    .\Wait-VMTask.ps1 $ret.Job    

    #----------------------------------------------------------------------------
    # Confirm all VMs are shutdown
    #----------------------------------------------------------------------------
    $timeoutSec = 600
    for ($retryCount = 0; $retryCount -lt $timeoutSec/5; $retryCount++) 
    {
        #----------------------------------------------------------------------------
        # Check if there's still a VM existing.
        #----------------------------------------------------------------------------
        $VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine'and elementName= '$VMName'"
        if ($VM -eq $null)
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
Write-Host "Verifying [Remove-VM.ps1]..." 
if ($isSuccessful -ne $true )
{
    Throw "EXECUTE [Remove-VM.ps1] FAILED."
}
else
{
    Write-Host "EXECUTE [Remove-VM.ps1] SUCCEED." -foregroundcolor green
}

exit