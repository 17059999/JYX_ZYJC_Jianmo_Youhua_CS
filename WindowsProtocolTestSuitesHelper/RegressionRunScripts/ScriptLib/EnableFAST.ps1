#-----------------------------------------------------------------------------
# Script  : EnableFAST
# Usage   : Enable FAST and Claims for this Realm.
# Remark  : If item already exists in the registry, it will be overwritten.
#-----------------------------------------------------------------------------
try
{
	New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system" -Name "KDC" -Force -ErrorAction Stop
	New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system\KDC" -Name "Parameters" -Force -ErrorAction Stop
	New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system\KDC\Parameters" `
		-Name "EnableCbacAndArmor" -PropertyType "DWORD" -Value "1" -Force -ErrorAction Stop
	New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system\KDC\Parameters" `
		-Name "CbacAndArmorLevel" -PropertyType "DWORD" -Value "2" -Force -ErrorAction Stop

	New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system" -Name "kerberos" -Force -ErrorAction Stop
	New-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system\kerberos" -Name "Parameters" -Force -ErrorAction Stop
	New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system\kerberos\Parameters" `
		-Name "EnableCbacAndArmor" -PropertyType "DWORD" -Value "1" -Force -ErrorAction Stop
	New-ItemProperty -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\policies\system\kerberos\Parameters" `
		-Name "Supportedencryptiontypes" -PropertyType "DWORD" -Value "0x7fffffff" -Force -ErrorAction Stop
}
catch
{
	throw "Unable to set FAST and Claims. Error happened: " + $_.Exception.Message
}