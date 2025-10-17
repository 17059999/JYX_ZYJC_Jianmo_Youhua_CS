##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################

# Script is used to assign working directory
# 

param(
[UInt64]$diskSize = 150GB
)

#Get the largest volume which is greater than the appointed disk size
$volumeArr = Get-Volume | where{$_.SizeRemaining -gt $diskSize} | Sort-Object SizeRemaining -Descending

$path = $null
if($volumeArr -ne $null)
{
    $path = $volumeArr[0].DriveLetter + ":" + "\WinteropProtocolTesting"
}


if($path -eq $null)
{
    $path = $env:SystemDrive + "\WinteropProtocolTesting"
}

CMD /C setx NewWorkingDir $path /M
Write-Host $path -ForegroundColor Yellow
exit 0