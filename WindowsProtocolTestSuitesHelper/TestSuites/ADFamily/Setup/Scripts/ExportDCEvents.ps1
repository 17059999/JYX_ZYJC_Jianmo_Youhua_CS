#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

Param([string]$logPath = "c:\temp\")
$file = $logPath + "DsEvents.evtx"
If (Test-Path $file){
	Remove-Item $file
}
wevtutil epl "Directory Service" $file

$file = $logPath + "SystemEvents.evtx"
If (Test-Path $file){
	Remove-Item $file
}
wevtutil epl "System" $file

$file = $logPath + "DfsrEvents.evtx"
If (Test-Path $file){
	Remove-Item $file
}
wevtutil epl "DFS Replication" $file

$file = $logPath + "DnsEvents.evtx"
If (Test-Path $file){
	Remove-Item $file
}
wevtutil epl "DNS Server" $file
