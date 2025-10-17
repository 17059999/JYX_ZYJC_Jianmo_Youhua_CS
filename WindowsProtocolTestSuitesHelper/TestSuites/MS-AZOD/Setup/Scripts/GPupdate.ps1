﻿#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
####################################################################################
##
## 	Microsoft Windows Powershell Scripting
##  File:		GPupdate.ps1
##	Purpose:	forcely run gpupdate command
##	Version: 	1.1 (18 Jan, 2012)
####################################################################################

#----------------------------------------------------------------------------
# Get working directory and log file path
#----------------------------------------------------------------------------
$workingDir=$MyInvocation.MyCommand.path
$workingDir =Split-Path $workingDir
$runningScriptName=$MyInvocation.MyCommand.Name
$logFile="$workingDir\$runningScriptName.log"

#----------------------------------------------------------------------------
#Createthe log file
#----------------------------------------------------------------------------
echo "-----------------$runningScriptName Log----------------------" > $logFile
echo "Executing [$runningScriptName.ps1]."  >> $logFile

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    echo "----------------$runningScriptName Log----------------------" > $logFile
    echo "Usage: This script will open the cmd.exe with a domain user account."   >> $logFile
    echo "Example: $runningScriptName.ps1 username password domainname"  >> $logFile	    
}
#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}
cmd /c gpupdate /force >> $logFile
#----------------------------------------------------------------------------
# print out the log
#----------------------------------------------------------------------------

echo "-----------------$runningScriptName Log Done----------------------" >> $logFile


