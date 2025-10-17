################################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
################################################################################################

#--------------------------------------------------------------------------------------------------
# Script to Remove resource created for previous Auto Create Azure Image runs on the machine
#--------------------------------------------------------------------------------------------------

param(
[string]$basePath
)

#verify script is running as administrator else elevate
$invocationDefinition = $MyInvocation.MyCommand.Definition
$scriptPath = split-path -parent $invocationDefinition

$user = New-Object Security.Principal.WindowsPrincipal $([Security.Principal.WindowsIdentity]::GetCurrent())
$userName = $user.Identities.Name
$userIsAdmin = $user.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)

if ($userIsAdmin -eq $false)
{
        Write-Host "Attempting to elevate user $userName"
        Start-Process powershell.exe -Verb RunAs -ArgumentList ('-noprofile -noexit -file "{0}" -elevated' -f ($invocationDefinition))
        exit
}
else
{
    Write-Host "Executing script as Administrator for user $userName"
}

Set-Location $scriptPath
$csvFileName = "AutoAzureImage.csv"
function Main($basePath, $csvFileName)
{
    #verify base path
    if(-not ($null -eq $basePath -or $basePath -eq "") -and (Test-Path $basePath\$csvFileName))
    {
        Write-Host "Base directory path: $basePath"
    }
    else
    {
        throw "Invalid Base Path. Can't find CSV at $basePath"
    }

    #Read CSV
    Write-Host "Reading CSV"
    $userSettings = Import-CSV -Path $basePath/$csvFileName
    $Global:basePath = $basePath


    #cleanup each image entry
    foreach($setting in $userSettings)
    {
        $vmName = $setting.VMName
        $vmPath = "$basePath\autovm\$vmName"
        Write-Host "Cleaning up $vmName ..."
        . $basePath/Cleanup.ps1 $vmPath $vmName
    }
}

Main $basePath $csvFileName