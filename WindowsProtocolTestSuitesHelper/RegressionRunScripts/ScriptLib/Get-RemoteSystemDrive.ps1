#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-RemoteSystemDrive.ps1
## Purpose:        Get the system drive letter of a remote or local computer.
## Version:        1.0 (18 May, 2008)
##
##############################################################################

param(
[string]$computerName,
[string]$userName,
[string]$password
)

# Write Call Stack
if($function:EnterCallStack -ne $null)
{
	EnterCallStack "Get-RemoteSystemDrive.ps1"
}

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-RemoteSystemDrive.ps1] ..." -foregroundcolor cyan
Write-Host "`$computerName = $computerName"
Write-Host "`$userName     = $userName"
Write-Host "`$password     = $password"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script will get the system drive letter of a remote or local computer."
    Write-host
    Write-host "Example: Get-RemoteSystemDrive.ps1 SUT01"
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
if ($computerName -eq $null -or $computerName -eq "")
{
    Throw "Parameter computerName is required."
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
#if ($usr.IndexOf("\") -eq -1)
#{
#    if ($global:domain  -eq $null -or $global:domain -eq "")
#    {
#       $usr = "$computerName\$usr"
#    }
#    else
#    {
#        $usr = "$global:domain\$usr"
#   }
#}
#[v-xich]: Remove this, we don't use domain account as default.

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
# Wait the computer is started up
#----------------------------------------------------------------------------
$disconnectCmd = "net.exe use \\$computerName\IPC$ /delete /y                1>>$global:testResultDir\Get-RemoteSystemDrive.ps1.std.log 2>>$global:testResultDir\Get-RemoteSystemDrive.ps1.err.log"
$connectCmd    = "net.exe use \\$computerName\IPC$ $password /User:$userName 1>>$global:testResultDir\Get-RemoteSystemDrive.ps1.std.log 2>>$global:testResultDir\Get-RemoteSystemDrive.ps1.err.log"
cmd /c $disconnectCmd
cmd /c $connectCmd
if ($lastExitCode -ne 0)
{
    Write-Host "$computerName is not started yet..."  -foregroundcolor Yellow
    cmd /c $disconnectCmd
    .\WaitFor-ComputerReady.ps1 $computerName  $userName $password 
}
cmd /c $disconnectCmd

#----------------------------------------------------------------------------
# Wait the computer RPCServer is online
#----------------------------------------------------------------------------
Write-Host "Try to connect to the RPC server of $computerName ..."
$waitTimeout = 600
$osObj = $null
$retryCount = 0
for (; $retryCount -lt $waitTimeout/2; $retryCount++ ) 
{
    trap{continue}
    &{
        trap{Write-Host $_;break}

        $script:osObj = get-wmiobject win32_operatingsystem -computer $computerName -Credential $script:credential 
        if($script:osObj -ne $null)
        {
            break;
        }
    }
    
    $NoNewLineIndicator = $True
    if ( $retryCount % 60 -eq 59 )
    {
       $NoNewLineIndicator = $False
    }
    Write-host "." -NoNewLine:$NoNewLineIndicator -foregroundcolor White
    
    Start-Sleep -s 2  # Sleep for 2 seconds [System.Threading.Thread]::Sleep(2000)
}
if ($osObj -eq $null)
{
    Throw "Connect to remote computer $computerName  failed."
}

Write-host "." -foregroundcolor Green
Write-Host "The RPCServer of $computerName is started now."

#----------------------------------------------------------------------------
# Get the system drive letter
#----------------------------------------------------------------------------
$systemDrive = $osObj.SystemDrive;
if (($systemDrive -ne $null) -and ($systemDrive -ne ""))
{
    Write-Host "The system drive for $computerName is $systemDrive."
}
else
{
    throw "Cannot get the system drive for $computerName"
}

#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Get-RemoteSystemDrive.ps1] FINISHED (NOT VERIFIED)."
# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "Get-RemoteSystemDrive.ps1"
}

return $systemDrive