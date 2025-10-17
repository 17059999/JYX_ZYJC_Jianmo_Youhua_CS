#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Config-AutoLogon.ps1
## Purpose:        Create Auto Logon Reg files, copy it to a remote computer's system volume and import it. To make this computer can be loged in automatically with specified credential.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$computerName,
[string]$usr,
[string]$pwd
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Config-AutoLogon.ps1]..." -foregroundcolor cyan
Write-Host "`$computerName        = $computerName" 
Write-Host "`$usr                 = $usr" 
Write-Host "`$pwd                 = $pwd" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script configures a computer to logon automatically by specified credential."
    Write-host
    Write-host "Example: ConfigAutoLogon.ps1 SUT01 username password"
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
Write-Host "Config auto logon with $usr/$pwd on $computerName ..." 

#----------------------------------------------------------------------------
# Create REG file.
#----------------------------------------------------------------------------
$regFile = "$global:testResultDir\AutoLogon.reg"
$usrEscaped = $usr.Replace("\", "\\")
Write-Host "Creating REG file ..." 
.\CreateAutoLogonRegFile.bat $usrEscaped $pwd $regFile

#----------------------------------------------------------------------------
# Get remote computer's system drive share, such as C$/D$
#----------------------------------------------------------------------------
$sysDrive       = .\Get-RemoteSystemDrive.ps1 $computerName $usr $pwd
$sysDriveShare  = $sysDrive.Replace(":", "$")

#----------------------------------------------------------------------------
# Copy BAT and REG file to remote computer.
#----------------------------------------------------------------------------
Write-Host "Copying BAT and REG file to remote computer ..." 

Write-Host "Try to connect to $ComputerName with Net Use..."
net.exe use \\$ComputerName\$sysDriveShare  $pwd /User:$usr 2>&1 | Write-Host
#Write-Host "Return from Net Use: $lastExitCode"

xcopy.exe /Y $regFile \\$computerName\$sysDriveShare  2>&1 | Write-Host
#Write-Host "Return from copying $regFile: $lastExitCode"

xcopy.exe /Y .\ImportAutoLogoRegFile.bat \\$computerName\$sysDriveShare 2>&1 | Write-Host
Write-Host "Return from copying ImportAutoLogoRegFile.bat: $lastExitCode"

net.exe use \\$computerName\$sysDriveShare /delete 2>&1 | Write-Host

#----------------------------------------------------------------------------
# Execute BAT on remote computer.
#----------------------------------------------------------------------------
.\RemoteExecute-Command.ps1 $computerName "$sysDrive\ImportAutoLogoRegFile.bat" $usr $pwd

#----------------------------------------------------------------------------
# Wait, to make sure this computer is shutting down.
#----------------------------------------------------------------------------
Write-Host "Wait, the computer is shutting down..."
[System.Threading.Thread]::Sleep(30000)

#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Config-AutoLogon.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit
