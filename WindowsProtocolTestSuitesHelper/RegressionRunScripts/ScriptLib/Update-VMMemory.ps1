#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Update-VMMemory.ps1
## Purpose:        Update VMMemory
## Version:        1.0 (7 Oct, 2008)
##
##############################################################################
        
param(
[String]$VMName,
[double]$MemorySize = 900,
[String]$Server="."
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Update-VMMemory.ps1] ..." -foregroundcolor cyan
Write-Host "`$VMName           = $VMName"
Write-Host "`$MemorySize       = $MemorySize"
Write-Host "`$Server           = $Server"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Update VMMemory in hyper-v"
    Write-host
    Write-host "Example 1: Update-VMMemory.ps1 `"w2k8-x64-01`" 1000"
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
    throw "Parameter $VMName cannot be empty."
}

if ($MemorySize -eq $null -or $MemorySize -eq "")
{
    throw "Parameter $MemorySize cannot be empty."
}

#----------------------------------------------------------------------------
# Define function
#----------------------------------------------------------------------------
Function Get-VM 
{Param ([String]$Name="%", [String]$Server=".", [Switch]$suspended, [switch]$running, [Switch]$stopped) 
 $Name=$Name.replace("*","%")
 $WQL="Select * From MsVM_ComputerSystem Where ElementName Like '$Name' AND Caption Like 'Virtual%' "
 if ($running -or $stopped -or $suspended) {
    [String]$state = ""
    if ($running)  {$State +="or enabledState=" +  $VMState["running"]  }
    if ($Stopped)  {$State +="or enabledState=" +  $VMState["Stopped"]  }
    if ($suspended){$State +="or enabledState=" +  $VMState["suspended"]}
    $WQL += "AND (" + $state.substring(3) +")" }
 Get-WmiObject -computername $Server -NameSpace "root\virtualization" -Query $WQL
}

Function Get-VMMemorySettingData
{Param ($VM, $Server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMMemorySettingData -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject])
  {Get-WmiObject -ComputerName $VM.__Server -NameSpace "root\virtualization" -Query "select * from Msvm_MemorySettingData where instanceId Like 'Microsoft:$($VM.name)%' "  }
 $VM=$Null
}

#----------------------------------------------------------------------------
# Main function
#----------------------------------------------------------------------------
$vm = Get-VM -Name $VMName -Server "." 

if( $vm -eq $null )
{
    Throw (" VM $VMName is not exist.")
}

$VMMemorySettingData = Get-VMMemorySettingData $vm
$VMMemorySettingData.Limit = $MemorySize
$VMMemorySettingData.Reservation = $MemorySize
$VMMemorySettingData.VirtualQuantity = $MemorySize
$arguments = @($VM, @($VMMemorySettingData.psbase.GetText([System.Management.TextFormat]::WmiDtd20)) , $null)
$VSMgtSvc = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService") 
$result = $VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystemResources",$arguments)  
 if ($result -eq 0) {"Update memory for VM $VMName to $MemorySize successfully..."} else {"Failed to update memory for VM $VMName, result code: $result..."} 
 $vm=$null
 
#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Update-VMMemory.ps1] SUCCEED." -foregroundcolor Green
