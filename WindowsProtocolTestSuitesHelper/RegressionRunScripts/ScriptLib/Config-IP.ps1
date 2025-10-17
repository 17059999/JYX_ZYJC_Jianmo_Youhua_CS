#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-IP.ps1
## Purpose:        Config the IP on VM (disable IPv4 or disable IPv6)
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$IPVersion,
[string]$testResultsPath
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
$logFile = $testResultsPath + "\Config-IP.ps1.log"
Start-Transcript $logFile -force

Write-Host "EXECUTING [Config-IP.ps1] ..." -foregroundcolor cyan
Write-Host "`$IPVersion        = $IPVersion"
Write-Host "`$testResultsPath  = $testResultsPath"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "This script will config the IP on VM"
    Write-host
    Write-host "Example: Config-IP.ps1 IPv6"
    Write-host
}

#----------------------------------------------------------------------------
# Function: Disable-IPv4
# Usage   : Disable IPv4 on the machine
#----------------------------------------------------------------------------
function Disable-IPv4
{
    #This script is going to disable IPv4 through adding a registry key and restart computer
    #no parameter is needed
    
    #First we query the IP configuration to see if IPv4 is enabled
    
    Write-Host "Detect IPv4."
    
    $objNetConfig = Get-WmiObject Win32_NetworkAdapterConfiguration
    
    $IPv4Enabled = $False
    
    #Get the enabled network adapter, then list its IP to see if it contains IPv4
    #If it does, then IPv4 is enabled
    
    foreach ($IPConfig in $objNetConfig)
    {
        if ($IPConfig.IPEnabled)
        {
            foreach ($IP in $IPConfig.IPAddress)
            {
                if ($IP.ToString().Contains("."))
                {
                    $IPv4Enabled = $True
                    break
                }
            }
        }
    }
    
    if (!$IPv4Enabled)
    {
        Write-Host "IPv4 is not enabled, no need to disable it"
        return
    }
    
    #if IPv4 is enabled, check if Registry key "DisabledComponents" exists
    #if not, create one
    #if does, set its value to 0xFF
    
    Write-Host "IPv4 is enabled. Disable IPv4 now..."
    
    $ItemFound = $False
    $RestartNeeded = $False
    
    $RegKeySet = Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters
    $RegKeyMemberSet = $RegKeySet | Get-Member
    foreach ($Reg in $RegKeyMemberSet)
    {
        if ($reg.Name -eq "DisabledComponents")
        {
            $ItemFound = $True
            if ($RegKeySet.DiabledComponents -ne 0xFF)
            {
                Write-Host "Modify the registry to disable IPv4."
                Set-ItemProperty -path HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters -name DisabledComponents -value 0xFFFFFFFF
                $RestartNeeded = $True
            }
        }
    }
    if (!$ItemFound)
    {
        Write-Host "Add a new registry to disable IPv4."
        $obj = New-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip\Parameters -Name DisabledComponents -PropertyType DWORD -Value 0xFFFFFFFF
        $RestartNeeded = $True
    }
    
    #if Registry key "DisabledComponents" is created or modified, we need to restart computer
    if ($RestartNeeded)
    {
        Write-Host "This change will take effect after restarting." 
    }
}

#----------------------------------------------------------------------------
# Function: Disable-IPv6
# Usage   : Disable IPv6 on the machine
#----------------------------------------------------------------------------
function Disable-IPv6
{
    #This script is going to disable IPv6 through adding a registry key and restart computer
    #no parameter is needed
    
    #First we query the IP configuration to see if IPv6 is enabled
    
    Write-Host "Detect IPv6."
    
    $objNetConfig = Get-WmiObject Win32_NetworkAdapterConfiguration
    
    $IPv6Enabled = $False
    
    #Get the enabled network adapter, then list its IP to see if it contains IPv6
    #If it does, then IPv6 is enabled
    
    foreach ($IPConfig in $objNetConfig)
    {
        if ($IPConfig.IPEnabled)
        {
            foreach ($IP in $IPConfig.IPAddress)
            {
                if ($IP.ToString().Contains(":"))
                {
                    $IPv6Enabled = $True
                    break
                }
            }
        }
    }
    
    if (!$IPv6Enabled)
    {
        Write-Host "IPv6 is not enabled, no need to disable it."
        return
    }
    else
    {
        Write-Host "Start to disable IPv6..."
    }
    
    
    #if IPv6 is enabled, check if Registry key "DisabledComponents" exists
    #if not, create one
    #if does, set its value to 0xFF
    
    Write-Host "IPv6 is enabled. Disable IPv6 now..."
    
    $ItemFound = $False
    $RestartNeeded = $False
    
    $RegKeySet = Get-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters
    $RegKeyMemberSet = $RegKeySet | Get-Member
    foreach ($Reg in $RegKeyMemberSet)
    {
        if ($reg.Name -eq "DisabledComponents")
        {
            $ItemFound = $True
            if ($RegKeySet.DiabledComponents -ne 0xFF)
            {
                Write-Host "Modify the registry to disable IPv6."
                Set-ItemProperty -path HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters -name DisabledComponents -value 0xFFFFFFFF
                $RestartNeeded = $True
            }
        }
    }
    if (!$ItemFound)
    {
        Write-Host "Add a new registry to disable IPv6."
        $obj = New-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Services\Tcpip6\Parameters -Name DisabledComponents -PropertyType DWORD -Value 0xFFFFFFFF
        $RestartNeeded = $True
    }
    
    #if Registry key "DisabledComponents" is created or modified, we need to restart computer
    if ($RestartNeeded)
    {
        Write-Host "This change will take effect after restarting."
    }
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
if ($IPVersion -eq $null -or $IPVersion -eq "")
{
    Throw "Parameter IPVersion is required."
}

#----------------------------------------------------------------------------
# Setting IPv4/IPv6
#----------------------------------------------------------------------------
Write-Host "Current IP information:"    
ipconfig.exe 2>&1 | Write-Host
Write-Host "Set IP Version according to: $IPVersion."      
if ($IPVersion -eq "IPv4")
{
    # TODO: DISABLE IPv6
    Write-Host "Begin to disable IPv6..."                  
    Disable-IPv6
}
elseif ($IPVersion -eq "IPv6")
{
    # TODO: DISABLE IPv4
    Write-Host "Begin to disable IPv4..."                  
    Disable-IPv4
}
elseif ($IPVersion -eq "IPv4IPv6")
{
    Write-Host "Support IPv4 and IPv6 both, do nothing here."   
    # Do nothing (should enable IPv4 and Ipv6)
}
else
{
    Write-Host "Not Supported IP Version: $IPVersion"      
    Write-Host "Error in ConfigIPAndDomain.ps1" > $Env:SystemDrive\config.error.signal
    Throw "Not supported IP version " + $IPVersion
}
Write-Host "Will check IPConfig after restarted."

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Config-IP.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow
Stop-Transcript

exit