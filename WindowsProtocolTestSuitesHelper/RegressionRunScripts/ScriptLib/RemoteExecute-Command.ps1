#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           RemoteExecute-Command.ps1
## Purpose:        Execute a command on a remote computer via WMI.
## Version:        1.1 (26 June, 2008)
##
##############################################################################

param(
[string]$computerName, 
[string]$cmdLine, 
[string]$usr, 
[string]$pwd
)

# Write Call Stack
if($function:EnterCallStack -ne $null)
{
	EnterCallStack "RemoteExecute-Command.ps1"
}

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [RemoteExecute-Command.ps1]." -foregroundcolor cyan
Write-Host "`$computerName = $computerName"
Write-Host "`$cmdLine      = $cmdLine"
Write-Host "`$usr          = $usr"
Write-Host "`$pwd          = $pwd"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script execute a command on remote computer."
    Write-host
    Write-host "Example: RemoteExecute-Command.ps1 SUT01 'netsh... username password'"
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
if ($cmdLine -eq $null -or $cmdLine -eq "")
{
    Throw "Parameter cmdLine is required."
}

#----------------------------------------------------------------------------
# Using global username/password when caller doesnot provide.
#----------------------------------------------------------------------------
if ($usr -eq $null -or $usr -eq "")
{
    $usr = $global:usr
    $pwd = $global:pwd
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
# Try to connect to the remote computer
#----------------------------------------------------------------------------
$script:objCon = $null
for ($index = 0; $index -lt 15; $index++)
{
    trap{continue} 
    &{
        trap{Write-Host $_ ;break}

        $objSWemLocator = New-Object -ComObject WbemScripting.SWbemLocator
        $script:objCon = $objSWemLocator.ConnectServer($computerName, "root\CIMV2", $usr, $pwd, "", "", 128)

        $script:objCon.Security_.ImpersonationLevel = 3
        $script:objCon.Security_.AuthenticationLevel = 6

        if ($script:objCon -ne $null)
        {
            break
        }
    }
    Write-Host "."
    Start-Sleep -s 10
}

if ($script:objCon -eq $null)
{
    Throw "Connect to remote computer failed."
}

#----------------------------------------------------------------------------
# Remote execute command
#----------------------------------------------------------------------------
$oProc = $objCon.Get("Win32_Process")
$mthd = $oProc.Methods_.Item("Create")
$oInParams = $mthd.InParameters.SpawnInstance_()
$oInParams.Properties_.Item("CommandLine").Value = $cmdLine
$oOutParams = $objCon.ExecMethod("Win32_Process", "Create", $oInParams)
$returnFromExecRemote = $oOutParams.Properties_.Item("ReturnValue").Value 

#----------------------------------------------------------------------------
# Verifying Execute Result
#----------------------------------------------------------------------------
Write-Host "Verifying the return code from remote execution..." 
if ($returnFromExecRemote -ne 0)
{
    Throw "Execute [RemoteExecute-Command.ps1] failed. Return code is: $returnFromExecRemote."
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [RemoteExecute-Command.ps1] SUCCEED." -foregroundcolor green

# Write Call Stack
if($function:ExitCallStack -ne $null)
{
	ExitCallStack "RemoteExecute-Command.ps1"
}

exit 0
