#############################################################################
## Copyright (c) Microsoft. All rights reserved.
## Licensed under the MIT license. See LICENSE file in the project root for full license information.
##
#############################################################################

#----------------------------------------------------------------------------
# Azure assigns a default gateway to the first (primary) network interface attached to the virtual machine.
# Azure does not assign a default gateway to additional (secondary) network interfaces attached to a virtual machine.
# Therefore, you are unable to communicate with resources outside the subnet that a secondary network interface is in, by default. 
# Secondary network interfaces can, however, communicate with resources outside their subnet, though the steps to enable communication are different for different operating systems.
# Script is used to add route in the route table.
#----------------------------------------------------------------------------

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$env:Path += ";$scriptPath;$scriptPath\Scripts"

#----------------------------------------------------------------------------
# Start loging using start-transcript cmdlet
#----------------------------------------------------------------------------
[string]$logFile = $MyInvocation.MyCommand.Path + ".log"
Start-Transcript -Path "$logFile" -Append -Force

#----------------------------------------------------------------------------
# Update Gateway For Multi NICs
#----------------------------------------------------------------------------
Write-Info.ps1 "Update Gateway For Multi NICs"
$DefaultGateWay = (Get-NetRoute | Where-Object { $_.DestinationPrefix -eq '0.0.0.0/0' }).NextHop
$Nics = Get-NetIPConfiguration
foreach ($nic in $Nics) {
    if ($null -eq $nic.IPv4DefaultGateway) {
        Write-Info "New-NetRoute InterfaceIndex:" $nic.InterfaceIndex "DefaultGateWay:" $DefaultGateWay
        New-NetRoute -InterfaceIndex $nic.InterfaceIndex -NextHop $DefaultGateWay -DestinationPrefix "0.0.0.0/0"
    }
}

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Write-Info.ps1 "Completed update gateway for multi NICs."
Stop-Transcript
exit 0