#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Copy-VM.ps1
## Purpose:        Copy Virtual Machine images from file server.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$srcVMPath, 
[string]$VMDir,
[string]$VMName,
[string]$ProtocolName = ""
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Copy-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$srcVMPath = $srcVMPath" 
Write-Host "`$VMDir = $VMDir" 
Write-Host "`$VMName = $VMName" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script copies Virtual Machine images from file server."
    Write-host
    Write-host "Example: CopyAndImportVM.ps1 \\pt3controller\VM_Base d:\VM W2K8-x86-01"
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
if ($srcVMPath -eq $null -or $srcVMPath -eq "")
{
    Throw "Parameter srcVMPath is required."
}
if ($VMDir -eq $null -or $VMDir -eq "")
{
    Throw "Parameter VMDir is required."
}
if ($VMName -eq $null -or $VMName -eq "")
{
    Throw "Parameter VMName is required."
}

Write-Host "Copying $VMName from $srcVMPath to $VMDir ..." 

#----------------------------------------------------------------------------
# Remove a VM if its name already exists in HyperV manager (if not, the copy will fail because the VM files are being used by HyperV)
#----------------------------------------------------------------------------
$VMSvr = Get-WmiObject -namespace "Root\Virtualization" "Msvm_VirtualSystemManagementService"
$VM = Get-WmiObject -namespace "Root\Virtualization" -query "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' and elementName= '$VMName'"

if($VM -ne $null)
{
    #----------------------------------------------------------------------------
    # Destroy VM
    #----------------------------------------------------------------------------
    Write-Host "The VM $VMName was found already exist in the HyperV manager. Now begin to destroy it..." -foregroundcolor Yellow
    Write-Host "Destorying $VMName ..." 
    .\Destroy-VM.ps1 $VMName
    Write-Host "Now the previous VM of $VMName has been destroyed. It is safe to copy the VM files now." 
}
else
{
    Write-Host "$VMName does not exist in Hyper-V"
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

#----------------------------------------------------------------------------
# Copying by robocopy /MIR
# Copying is skipped if a file is existing in target directory, and keeps the same version with source file.
#----------------------------------------------------------------------------
robocopy /MIR /NFL /NDL "$srcVMPath\$VMName" "$VMDir\$VMName" 2>&1 |Write-Host

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Copy-VM.ps1]..." 
$vhdFile = "$VMDir\$VMName\Virtual Hard Disks\$VMName.vhd"
if ([System.IO.File]::Exists($vhdFile) -ne $true)
{
    #Throw "EXECUTE [Copy-VM.ps1] FAILED. The VHD file cannot be found in the destination folder."
}
else
{
    # Mark vhd file as readonly
    attrib -R "$vhdFile"
    Write-Host "EXECUTE [Copy-VM.ps1] SUCCEED." -foregroundcolor green
}

exit
