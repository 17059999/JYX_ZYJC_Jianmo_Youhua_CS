#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Import-VM.ps1
## Purpose:        Import a virtual machine into Hyper-V.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$VMDir,
[string]$VMName,
[string]$ProtocolName = ""
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Import-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$VMDir        = $VMDir" 
Write-Host "`$VMName       = $VMName" 
Write-Host "`$ProtocolName = $ProtocolName"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script imports a virtual machine into Hyper-V."
    Write-host
    Write-host "Example: `$VMName = ImportVM.ps1 D:\VM W2K8-x86-01 MS-AZOD"
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
if ($VMDir -eq $null -or $VMDir -eq "")
{
    Throw "Parameter VMDir is required."
}
if ($VMName -eq $null -or $VMName -eq "")
{
    Throw "Parameter VMName is required."
}
if ($ProtocolName -ne "")
{
    #----------------------------------------------------------------------------
    # Remove a VM if its name already exists in HyperV manager (if not, the start vm will fail because the VM files are being used by HyperV)
    #----------------------------------------------------------------------------
    $VMSvr = Get-WmiObject -namespace "Root\Virtualization" "Msvm_VirtualSystemManagementService"
    $VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' and elementName= '$ProtocolName-$VMName'"
    
    if($VM -ne $null)
    {
        #----------------------------------------------------------------------------
        # Destroy VM
        #----------------------------------------------------------------------------
        Write-Host "The VM $ProtocolName-$VMName was found already exist in the HyperV manager. Wait 30 seconds to destroy it..." -foregroundcolor Red
        sleep 30
        Write-Host "Destorying $ProtocolName-$VMName ..." 
        .\Destroy-VM.ps1 "$ProtocolName-$VMName"
        Write-Host "Now the previous VM of $VMName has been destroyed. It is safe to copy the VM files now." 
    }
}

$VMFullName = "$VMDir\$VMName"
Write-Host "Importing $VMFullName into Hyper-V ..." 

#----------------------------------------------------------------------------
# Import VM by calling WMI.
#----------------------------------------------------------------------------
$VSysManager = Get-WmiObject "Msvm_VirtualSystemManagementService" -namespace "Root\Virtualization"
$ret = $VSysManager.ImportVirtualSystem($VMFullName, $true)

#----------------------------------------------------------------------------
# Wait until the import operation is done.
#----------------------------------------------------------------------------
.\Wait-VMTask.ps1 $ret.Job

#----------------------------------------------------------------------------
# Rename VM
#----------------------------------------------------------------------------
if ($ProtocolName -ne "")
{
    .\Rename-VM.ps1 $VMName "$ProtocolName-$VMName"
    return "$ProtocolName-$VMName"
}
else
{
    return $VMName
}

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Import-VM.ps1] FINISHED (NOT VERIFIED)."

exit