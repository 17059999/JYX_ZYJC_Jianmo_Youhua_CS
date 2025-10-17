#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Set-ComputerName.ps1
## Purpose:        Rename the computer name
## Version:        1.0 (6 Oct, 2008)
##
##############################################################################

param(
[string]$newComputerName,
[string]$computerName,
[string]$userName,
[string]$password,
[string]$domain,
[boolean]$isInDomain = $false
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-ComputerName.ps1] ..." -foregroundcolor cyan
Write-Host "`$newComputerName = $newComputerName"
Write-Host "`$computerName    = $computerName"
Write-Host "`$userName        = $userName"
Write-Host "`$password        = $password"
Write-Host "`$domain          = $domain"
Write-Host "`$isInDomain      = $isInDomain"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Rename the computer name."
    Write-host
    Write-host "Example 1: Set-ComputerName.ps1 NewName SUT01 administrator Password01! Contoso.com"
    Write-host "Example 2: Set-ComputerName.ps1 NewName"
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
if ($newComputerName -eq $null -or $newComputerName -eq "")
{
    throw "Parameter newComputerName cannot be empty."
}

if ($computerName -eq $null -or $computerName -eq "")
{
    $computerName = $env:COMPUTERNAME
}

if ($domain -eq $null -or $domain -eq "")
{
    if ($global:domain  -eq $null -or $global:domain -eq "")
    {
        $domain = "contoso.com"
    }
    else
    {
        $domain = $global:domain
    }
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
    if (!$isInDomain)
    {
        $userName = "$computerName\$userName"
    }
    elseif($domain)
    {
        $userName = "$domain\$userName"
    }
    else
    {
        $userName = "$global:domain\$userName"
    }
}

#----------------------------------------------------------------------------
# Set the computer name
#----------------------------------------------------------------------------
if ($computerName -eq $env:COMPUTERNAME)
{
    Write-Host "Rename computer name from $computerName to $newComputerName..."
    $osObj = Get-WmiObject Win32_ComputerSystem
    $retValue = $osObj.Rename($newComputerName)
    if($retValue.ReturnValue -ne 0)
    {
        $value = $retValue.ReturnValue
        throw "Failed to rename the computer: $value"
    }
    else
    {
        return $newComputerName
    }
}
elseif ($computerName -eq $newComputerName)
{
    Write-Host "No changes made, as the new computer name is the same as the old one."
}
else
{
    #----------------------------------------------------------------------------
    # Create credential for accessing VM
    #----------------------------------------------------------------------------
    $Securepassword = ConvertTo-SecureString -string $password -asplaintext -force
    $cred = New-Object -TypeName System.Management.Automation.PSCredential -argumentlist $userName,$Securepassword
    $osObj=  Get-WmiObject -Namespace "root\cimv2" -Class Win32_ComputerSystem -Credential $cred -Impersonation 3 -ComputerName $computerName
    $retValue = $osObj.Rename($newComputerName)
    if($retValue.ReturnValue -ne 0)
    {
        $value = $retValue.ReturnValue
        throw "Failed to rename the computer: $value"
    }
    else
    {
        Write-Host "Reboot the computer..."
        if ($userName.IndexOf("\") -eq 1)
        {
            if (!$isInDomain)
            {
                $userName = "$newComputerName\$userName"
            }
            elseif($domain)
            {
                $userName = "$domain\$userName"
            }
            else
            {
                $userName = "$global:domain\$userName"
            }
        }
        #cmd /c "ShutDown.exe /m \\$computerName /r /f /t 0"
        if($computerName -contains "::")
        {
            $computerName=$computerName.Replace(":","-") + ".ipv6-literal.net"
        }
        .\RemoteExecute-Command.ps1 $computerName "cmd /c `"shutdown.exe /r /f /t 0`"" $userName $password
        sleep 30
        .\WaitFor-ComputerReady.ps1 $computerName $userName $password
        return $newComputerName
    }
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-ComputerName.ps1] SUCCEED." -foregroundcolor Green
