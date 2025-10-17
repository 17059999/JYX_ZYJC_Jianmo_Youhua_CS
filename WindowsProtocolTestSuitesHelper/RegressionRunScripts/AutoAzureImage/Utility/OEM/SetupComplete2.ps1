###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

$adminAccount = Get-WmiObject Win32_UserAccount -filter "LocalAccount=True" | ? {$_.SID -Like "S-1-5-21-*-500"}
if($adminAccount.Disabled)
{
  Write-Host "Admin account was disabled. Enabling the Admin account."
  $adminAccount.Disabled = $false
  $adminAccount.Put()
}