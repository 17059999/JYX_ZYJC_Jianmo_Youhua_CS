#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-RemoteDNS.ps1
## Purpose:        Set DNS server address of the remote computer
## Version:        1.0 (6 June, 2011)
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows 2008 Server, Windows 2003 Server, Windows Vista, Windows XP
##
##############################################################################

param(
[string]$newDNS,
[string]$computerName,
[string]$userName,
[string]$password,
[string]$interface
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-RemoteDNS.ps1] ..." -foregroundcolor cyan
Write-Host "`$newDNS          = $newDNS"
Write-Host "`$computerName    = $computerName"
Write-Host "`$userName        = $userName"
Write-Host "`$password        = $password"
Write-Host "`$interface       = $interface"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Set DNS address of the remote computer."
    Write-host
    Write-host "Example 1: Set-RemoteDNS.ps1 `"192.168.0.201`" ENDPOINT01 contoso\administrator Password01!"
    Write-host "Example 2: Set-RemoteDNS.ps1 `"192.168.0.201`" ENDPOINT01 contoso\administrator Password01! `"Local Area Connection 1`""
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
# Function: Get-NetworkInterfaceNames
# Usage   : Get network interface names from registry
#----------------------------------------------------------------------------
function Get-NetworkInterfaceNames(
[string]$machineName
)
{
    if(!$machineName)
    {
        throw "Machine name cannot be empty."
    }
    
    $key="SYSTEM\CurrentControlSet\Control\Network"
    $keytype=[Microsoft.Win32.RegistryHive]::LocalMachine
    $remotebase=[Microsoft.Win32.RegistryKey]::OpenRemoteBaseKey($keytype,$machineName)
    $regkey=$remotebase.OpenSubKey($key)
    $subkeys = $regkey.GetSubkeyNames()
    foreach($subkey in $subkeys)
    {
        $entry = $regkey.OpenSubKey($subkey)
        $defaultName = $entry.GetValue($null)
        if($defaultName -and $defaultName.Contains("Adapters"))
        {
            $regkey = $entry
            $subkeys = $entry.GetSubkeyNames()
            break
        }
    }
    
    $interfaceNames = New-Object System.Collections.ArrayList
    foreach($subkey in $subkeys)
    {
        $entry = $regkey.OpenSubKey("$subkey\Connection")
        if($entry)
        {
            $interfaceName = $entry.GetValue("Name")
            $showIcon = $entry.GetValue("ShowIcon")
            if($showIcon -eq "0" -and $interfaceName)
            {
                $interfaceNames.Add($interfaceName)             
            }
        }
    }
    
    return $interfaceNames
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($newDNS -eq $null -or $newDNS -eq "")
{
    throw "Parameter newIP cannot be empty."
}

if ($computerName -eq $null -or $computerName -eq "")
{
    $computerName = $env:COMPUTERNAME
}

#----------------------------------------------------------------------------
# Using global username/password when caller doesnot provide.
#----------------------------------------------------------------------------
if ($userName -eq $null -or $userName -eq "")
{
    $userName = $global:usr
    $password = $global:pwd
}

#----------------------------------------------------------------------------
# Make username prefixed with domain/computername
#----------------------------------------------------------------------------
if ($userName.IndexOf("\") -eq -1)
{
    if ($global:domain  -eq $null -or $global:domain -eq "")
    {
        $userName = "$computerName\$userName"
    }
    else
    {
        $userName = "$global:domain\$userName"
    }
}

#----------------------------------------------------------------------------
# Convert the password to a SecureString
#----------------------------------------------------------------------------
$securePwd  = New-Object System.Security.SecureString
for ($i = 0; $i -lt $password.Length; $i++)
{
    $securePwd.AppendChar($password[$i]);
}
$credential = New-Object System.Management.Automation.PSCredential($userName, $securePwd) 

#----------------------------------------------------------------------------
# Set the DNS server address
#----------------------------------------------------------------------------
$osVersion = .\Get-OSVersion.ps1 $computerName $userName $password
NET.EXE USE \\$computerName /delete /y > null
NET.EXE USE \\$computerName /u:$userName $password
if($osVersion -eq "XP" -or $osVersion -eq "W2K3")
{
    #Determine IP version
    if($newDNS.Contains("."))
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ip set dnsservers `"$interface`" static $newDNS" $userName $password
        }
        else
        {
            $interfaceNames = Get-NetworkInterfaceNames $computerName
            $interfaceName = ""
            foreach($name in $interfaceNames)
            {
                if($name.ToString().Contains("Local Area") -and $name -gt $interfaceName)
                {
                    $interfaceName = $name
                }
            }
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ip set dnsservers `"$name`" static $newDNS" $userName $password
        }
    }
    else
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set dnsservers `"$interface`" $newDNS" $userName $password
        }
        else
        {
            $interfaceNames = Get-NetworkInterfaceNames $computerName
            $interfaceName = ""
            foreach($name in $interfaceNames)
            {
                if($name.ToString().Contains("Local Area") -and $name -gt $interfaceName)
                {
                    $interfaceName = $name
                }
            }
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set dnsservers `"$name`" $newDNS" $userName $password
        }
    }
}
else
{
    #Determine IP version
    if($newDNS.Contains("."))
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv4 set dnsservers `"$interface`" static $newDNS" $userName $password
        }
        else
        {
            $adapters = Get-WmiObject "Win32_NetworkAdapterConfiguration" -computer $computerName -Credential $credential |Where-Object -FilterScript{$_.IPEnabled -eq $true}
            foreach($adapter in $adapters)
            {
                $index = $adapter.InterfaceIndex
                .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ip set dnsservers $index static $newDNS" $userName $password
                break
            }
        }
    }
    else
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set dnsservers `"$interface`" $newDNS" $userName $password
        }
        else
        {
            $adapters = Get-WmiObject "Win32_NetworkAdapterConfiguration" -computer $computerName -Credential $credential |Where-Object -FilterScript{$_.IPEnabled -eq $true}
            foreach($adapter in $adapters)
            {
                $index = $adapter.InterfaceIndex
                .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set dnsservers $index $newDNS" $userName $password
                break
            }
        }
    }
}

Write-Host "Wait for the remote process to be completed..."
[System.Threading.Thread]::Sleep(25000)
if($newDNS.Contains(" "))
{
    $newDNS = $newDNS.Substring(0, $newDNS.IndexOf(" "))
}

#Commented by v-yulong. No need to restart computer after changing IP
#Write-Host "Reboot $computerName..."
#.\RemoteExecute-Command.ps1 $newIP "ShutDown.exe -r -f -t 1" $userName $password
#[System.Threading.Thread]::Sleep(30000)
#.\WaitFor-ComputerReady.ps1 $newIP $userName $password

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-RemoteDNS.ps1] SUCCEED." -foregroundcolor Green
