###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################
param(
[string]$redundantVMPath, 
[string]$redundantVMName
)


#--------------------------------------------------------------------------------------------------
# Remove all created resources after a run
#--------------------------------------------------------------------------------------------------
function Cleanup([string]$vmPath, [string]$vmName)
{
	#DeleteVM and resources folder
	if(Get-VM -Name $vmName -ErrorAction SilentlyContinue)
	{
		Stop-VM -Name $vmName -Force
		Remove-VM -Name $vmName -Force
		Write-Warning "Cleanup: VM $vmName has been removed"
	}
	if(Test-Path $vmPath)
	{
		Remove-Item $vmPath -Recurse -Force
		Write-Warning "Cleanup: directory $vmPath has been deleted"
	}
}

Cleanup $redundantVMPath $redundantVMName