#############################################################################
##
## Microsoft Windows Powershell Sripting
## File:           Release-Lock.ps1
## Purpose:        Release the process lock for the current process
## Version:        1.0 (28 April, 2011)
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows
##
##############################################################################
$SignalFileFullName = "$Env:SystemDrive\allocate.vmlock.signal"
if ( [System.IO.File]::Exists( $SignalFileFullName ) -eq $false )
{
    Throw "release lock failed. the signal file doesn't exist"
}  
cmd /C DEL $env:SYSTEMDRIVE\allocate.vmlock.signal

exit