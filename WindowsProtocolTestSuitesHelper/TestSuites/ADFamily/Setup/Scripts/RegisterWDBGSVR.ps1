#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

$private:regRunPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run" 
$private:regKeyName = "WDBGSVR"

# If the key has already been set, remove it
if (((Get-ItemProperty $regRunPath).$regKeyName) -ne $null)
{
	Remove-ItemProperty -Path $regRunPath -Name $regKeyName
}

try
{
    Set-ItemProperty -Path $regRunPath -Name $regKeyName `
                        -Value "c:\temp\scripts\WindowsDebugServer.exe" `
                        -Force -ErrorAction Stop
}
catch
{
    throw "Unable to restart. Error happened: $_.Exception.Message"
}
