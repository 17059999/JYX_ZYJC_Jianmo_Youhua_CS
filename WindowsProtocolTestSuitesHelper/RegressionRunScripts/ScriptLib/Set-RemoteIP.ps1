#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-RemoteIP.ps1
## Purpose:        Set IP address of the remote computer
## Version:        1.0 (7 Oct, 2008)
##
##############################################################################

param(
[string]$newIP,
[string]$computerName,
[string]$userName,
[string]$password,
[string]$interface
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-RemoteIP.ps1] ..." -foregroundcolor cyan
Write-Host "`$newIP           = $newIP"
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
    Write-host "Usage: Set IP address of the remote computer."
    Write-host
    Write-host "Example 1: Set-RemoteIP.ps1 `"192.168.0.110 255.255.255.0`" ENDPOINT01 contoso\administrator Password01!"
    Write-host "Example 2: Set-RemoteIP.ps1 `"192.168.0.110 255.255.255.0`" ENDPOINT01 contoso\administrator Password01! `"Local Area Connection 1`""
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
if ($newIP -eq $null -or $newIP -eq "")
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
# Set the IP address
#----------------------------------------------------------------------------
$osVersion = .\Get-OSVersion.ps1 $computerName $userName $password
NET.EXE USE \\$computerName /delete /y > null
NET.EXE USE \\$computerName /u:$userName $password
if($osVersion -eq "XP" -or $osVersion -eq "W2K3")
{
    #Determine IP version
    if($newIP.Contains("."))
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ip set address `"$interface`" static $newIP" $userName $password
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
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ip set address `"$name`" static $newIP" $userName $password
        }
    }
    else
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set address `"$interface`" $newIP" $userName $password
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
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set address `"$name`" $newIP" $userName $password
        }
    }
}
else
{
    #Determine IP version
    if($newIP.Contains("."))
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv4 set address `"$interface`" static $newIP" $userName $password
        }
        else
        {
            $adapters = Get-WmiObject "Win32_NetworkAdapterConfiguration" -computer $computerName -Credential $credential |Where-Object -FilterScript{$_.IPEnabled -eq $true}
            foreach($adapter in $adapters)
            {
                $index = $adapter.InterfaceIndex
                .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ip set address $index static $newIP" $userName $password
                break
            }
        }
    }
    else
    {
        if($interface)
        {
            .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set address `"$interface`" $newIP" $userName $password
        }
        else
        {
            $adapters = Get-WmiObject "Win32_NetworkAdapterConfiguration" -computer $computerName -Credential $credential |Where-Object -FilterScript{$_.IPEnabled -eq $true}
            foreach($adapter in $adapters)
            {
                $index = $adapter.InterfaceIndex
                .\RemoteExecute-Command.ps1 $computerName "cmd /c netsh interface ipv6 set address $index $newIP" $userName $password
                break
            }
        }
    }
}

Write-Host "Wait for the remote process to be completed..."
[System.Threading.Thread]::Sleep(25000)
if($newIP.Contains(" "))
{
    $newIP = $newIP.Substring(0, $newIP.IndexOf(" "))
}
Write-Host "Reboot $computerName..."
.\RemoteExecute-Command.ps1 $newIP "ShutDown.exe -r -f -t 1" $userName $password
[System.Threading.Thread]::Sleep(30000)
.\WaitFor-ComputerReady.ps1 $newIP $userName $password

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-RemoteIP.ps1] SUCCEED." -foregroundcolor Green
