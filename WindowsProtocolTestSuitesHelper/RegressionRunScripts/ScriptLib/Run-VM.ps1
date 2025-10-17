#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Run-VM.ps1
## Purpose:        Set the running state of a VM to "Started". i.e Turn on a VM.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$VMName
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Run-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName = $VMName" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script sets the running state of a VM to Started. i.e Turn on a VM."
    Write-host
    Write-host "Example: RunVM.ps1"
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
    return
}

#----------------------------------------------------------------------------
# Find the virtual machine my WMI query.
#----------------------------------------------------------------------------
$query = "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' AND ElementName='" + $VMName + "'"
$VM = Get-WmiObject -namespace "Root\Virtualization" -query $query
if ($VM -eq $null)
{
    Throw "$VMName was not found."
}
Write-Host "Starting VM ($VMName) ..." 

#----------------------------------------------------------------------------
# Turn on the virtual machine.
#----------------------------------------------------------------------------
$ret = $VM.RequestStateChange(2)
.\Wait-VMTask.ps1 $ret.Job

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Run-VM.ps1] FINISHED (NOT VERIFIED)."

exit