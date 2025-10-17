#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Turnoff-VM.ps1
## Purpose:        Turns off an existing virtual machine in Hyper-V.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$VMName
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Turnoff-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 
 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script turns off an existing virtual machine in Hyper-V."
    Write-host
    Write-host "Example: Turnoff-VM.ps1 W2k3-x86-01"
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

Write-Host "Start turnoff an VM ..." 
#----------------------------------------------------------------------------
# Get a list of virtual machines, whether it's in running state.
#----------------------------------------------------------------------------
$VMSvr = Get-WmiObject -namespace "Root\Virtualization" "Msvm_VirtualSystemManagementService"
$VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' and elementName= '$VMName'"

$isSuccessful = $true
if($VM -ne $null)
{
    #----------------------------------------------------------------------------
    # Turnoff VM
    #----------------------------------------------------------------------------   
    Write-Host "Turning off $VMName ..." 
    $ret = $VM.RequestStateChange(3)
    if ($ret.Job -ne $null)
    {
        .\Wait-VMTask.ps1 $ret.Job
    }     
}
#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Turnoff-VM.ps1] SUCCEED(Not Verify)." -foregroundcolor green

exit