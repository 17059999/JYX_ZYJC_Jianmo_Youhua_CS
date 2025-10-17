# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

# Print the parameters passed in
Write-Host "Profile: $env:Profile"
Write-Host "Selected: $env:Selected"
Write-Host "Filter: $env:Filter"
Write-Host "Config: $env:Config"
Write-Host "ReportFormat: $env:ReportFormat"

if (($env:Profile -eq "") -or (-not (Test-Path "/data/fileserver/$($env:Profile)"))) {
    Write-Host "The profile `"$env:Profile`" does not exist in the current directory on the host."
    exit -1
}

Set-Location "/opt/ptmcli"

# Construct the log path and the PTMCli command
$logExt = "log"
if ($env:ReportFormat -match "Plain") {
    $logExt = "log"
}
elseif ($env:ReportFormat -match "JSON") {
    $logExt = "json"
}
elseif ($env:ReportFormat -match "XUnit") {
    $logExt = "xml"
}

$profileName = [System.IO.Path]::GetFileNameWithoutExtension($env:Profile)
$resultPath = "/data/fileserver/$($profileName)_$((Get-Date).ToString("yyyyMMddhhmmss"))"

mkdir $resultPath

$logPath = "$resultPath/report.$logExt"

$command = "dotnet ./PTMCli.dll --profile `"/data/fileserver/$($env:Profile)`" --testsuite `"/opt/fileserver`" --format `"$env:ReportFormat`" --report `"$logPath`""
if ($env:Selected -match "true") {
    $command += " --selected"
}
if ($env:Filter -ne "") {
    $command += " --filter `"$env:Filter`""
}
if ($env:Config -ne "") {
    $command += " --config $env:Config"
}

Write-Host "Invoking the following command: $command"
Invoke-Expression $command

if (Test-Path "/opt/fileserver/HtmlTestResults") {
    Copy-Item "/opt/fileserver/HtmlTestResults" $resultPath -Recurse -Force
}