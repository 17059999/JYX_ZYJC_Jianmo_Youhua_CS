# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

# This method is used to trigger RDP client to close all RDP connection to server for clean up.

$protocolName="RDP"
$endPointPath = "$env:SystemDrive\MicrosoftProtocolTests\$protocolName\Client-Endpoint" 
$version = Get-ChildItem $endPointPath | where {$_.Attributes -eq "Directory" -and $_.Name -match "\d+\.\d+\.\d+\.\d+"} | Sort-Object Name -descending | Select-Object -first 1
$shpath = "$endPointPath\$version\bin\NonWindowsSUTImplementation"

$protocolConfigPath = $env:SystemDrive + "\temp"
[XML]$vmConfig = Get-Content "$protocolConfigPath\protocol.xml"
$sutSettting = $vmConfig.lab.servers.vm | where {$_.role -eq "SUT"}
$SUT_IP=$sutSettting.IP

cmd /c pscp -pw Password01! $shpath\stoprdp.sh administrator@"$SUT_IP":/home/administrator
cmd /c plink -l administrator -pw Password01! $SUT_IP chmod +x /home/administrator/stoprdp.sh  
cmd /c plink -l administrator -pw Password01! $SUT_IP /home/administrator/stoprdp.sh

return 0