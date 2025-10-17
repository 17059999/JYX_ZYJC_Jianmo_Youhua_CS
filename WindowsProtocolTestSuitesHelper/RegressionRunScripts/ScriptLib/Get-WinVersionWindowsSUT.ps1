# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
    [string]$ComputerName,
    [string]$UserName,
    [string]$UserPassword
)

$winVersion = "Windows Server 2000"
$secureUserPassword = ConvertTo-SecureString $UserPassword -AsPlainText  -Force
$userCred = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList @($UserName, $secureUserPassword)

Write-Host "Try to connect to computer $ComputerName ..."
$waitTimeout = 600
$sysInfo = $null
$retryCount = 0
for (; $retryCount -lt $waitTimeout / 2; $retryCount++ ) {
    if($ComputerName){
        $sysInfo = Get-WmiObject Win32_OperatingSystem -ComputerName $ComputerName -Credential $userCred
    } else {
        $sysInfo = Get-WmiObject Win32_OperatingSystem
    }
    
    if ($null -ne $sysInfo) {
        break;  
    }
    
    $noNewLineIndicator = $true
    if ($retryCount % 60 -eq 59) {
        $noNewLineIndicator = $False
    }
    Write-Host "." -NoNewLine:$noNewLineIndicator -ForegroundColor White
    
    Start-Sleep -Seconds 2
}

if ($null -eq $sysInfo) {
    Write-Host "Connect to computer $ComputerName failed."
}

if ($null -ne $sysInfo) {
    $winVersion = $sysInfo.Name.split("|")[0]

    Write-Host "Set `$winVersion to $winVersion"
}
else {
    Write-Host "Cannot detect the Windows Version."
}

return $winVersion