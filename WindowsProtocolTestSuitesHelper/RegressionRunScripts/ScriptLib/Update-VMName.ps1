#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Update-VMName.ps1
## Purpose:        Update VMName
## Version:        1.0 (7 Oct, 2008)
##
##############################################################################

param(
[String]$OldName,
[String]$NewName,
[String]$Server="."
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Update-VMName.ps1] ..." -foregroundcolor cyan
Write-Host "`$OldName           = $OldName"
Write-Host "`$NewName           = $NewName"
Write-Host "`$Server            = $Server"
    
#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Update VMName in hyper-v"
    Write-host
    Write-host "Example 1: Update-VMName.ps1 `"SUT01`" `"SUT02`""
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
if ($OldName -eq $null -or $OldName -eq "")
{
    throw "Parameter $OldName cannot be empty."
}

if ($NewName -eq $null -or $NewName -eq "")
{
    throw "Parameter $NewName cannot be empty."
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

Function Get-VMSettingData
{Param ($VM, $Server=".")
 if ($VM -eq $null) {$VM=$_}
 if ($VM -is [String]) {$VM=(Get-VM -Name $VM -Server $Server) }
 if ($VM -is [Array]) {$VM | ForEach-Object {Get-VMSettingData -VM $_ -Server $Server} }
 if ($VM -is [System.Management.ManagementObject])
  {Get-WmiObject -ComputerName $vm.__Server -NameSpace "root\virtualization" -Query "ASSOCIATORS OF {$VM} Where ResultClass = MsVM_VirtualSystemSettingData"  | where-object {$_.instanceID -eq "Microsoft:$($vm.name)"}}
 $vm=$Null
}

#----------------------------------------------------------------------------
# Main function
#----------------------------------------------------------------------------
$vm = Get-VM -Name $OldName -Server "." 

if( $vm -eq $null )
{
    Throw (" VM $OldName is not exist.")
}

$VSSettingData = Get-VMSettingData $vm
$VSSettingData.ElementName = $NewName
$arguments = @($VM, $VSSettingData.psbase.GetText([System.Management.TextFormat]::WmiDtd20) , $null)
$VSMgtSvc = (Get-WmiObject -computerName $vm.__server -NameSpace  "root\virtualization" -Class "MsVM_virtualSystemManagementService") 
$result = $VSMgtSvc.psbase.invokeMethod("ModifyVirtualSystem",$arguments)  
if ($result -eq 0) {"Update VM $OldName to a new name: $NewName successfully..."} else {"Update VM $OldName to a new name: $NewName failed, result code: $result..."} 
$vm = $null

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Update-VMName.ps1] SUCCEED." -foregroundcolor Green
