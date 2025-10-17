# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

# This script is used to trigger client to initiate a RDP connection from RDP client, 
# and client should use RDP standard security protocol.

# Start a background job to initiate RDP on Linux SUT 
Start-Job -scriptblock {
    $protocolName="RDP"
    $endPointPath = "$env:SystemDrive\MicrosoftProtocolTests\$protocolName\Client-Endpoint" 
    $version = Get-ChildItem $endPointPath | where {$_.Attributes -eq "Directory" -and $_.Name -match "\d+\.\d+\.\d+\.\d+"} | Sort-Object Name -descending | Select-Object -first 1
    $shpath = "$endPointPath\$version\bin\NonWindowsSUTImplementation"

    $protocolConfigPath = $env:SystemDrive + "\temp"
    [XML]$vmConfig = Get-Content "$protocolConfigPath\protocol.xml"
    $driverComputerSettting = $vmConfig.lab.servers.vm | where {$_.role -eq "DriverComputer"}
    $sutSettting = $vmConfig.lab.servers.vm | where {$_.role -eq "SUT"}
    $SUT_IP=$sutSettting.ip
    $DriverComputer_IP = $driverComputerSettting.ip
    $DriverComputer_User = $vmConfig.lab.core.username
    $DriverComputer_Passwd = $vmConfig.lab.core.password

    cmd /c pscp -pw Password01! $shpath\runrdp.sh administrator@"$SUT_IP":/home/administrator
    cmd /c plink -l administrator -pw Password01! $SUT_IP chmod +x /home/administrator/runrdp.sh
    cmd /c plink -l administrator -pw Password01! $SUT_IP /home/administrator/runrdp.sh $DriverComputer_User $DriverComputer_Passwd $DriverComputer_IP
}

# Wait for xfreerdp is started on Linux SUT
sleep 30

return 0