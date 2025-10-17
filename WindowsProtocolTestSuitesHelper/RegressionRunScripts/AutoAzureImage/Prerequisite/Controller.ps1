###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

#--------------------------------------------------------------------------------------------------
# Script used by pipeline to check that an agent has prerequisite artifacts
#--------------------------------------------------------------------------------------------------

#Verify script is running as administrator else elevate
$invocationDefinition = $MyInvocation.MyCommand.Definition
$scriptPath = split-path -parent $invocationDefinition

$user = New-Object Security.Principal.WindowsPrincipal $([Security.Principal.WindowsIdentity]::GetCurrent())
$userName = $user.Identities.Name
$userIsAdmin = $user.IsInRole([Security.Principal.WindowsBuiltinRole]::Administrator)

if ($userIsAdmin -eq $false)
{
        Write-Host "Attemting to elevate user $userName"
        Start-Process powershell.exe -Verb RunAs -ArgumentList ('-noprofile -noexit -file "{0}" -elevated' -f ($invocationDefinition))
        exit
}
else
{
    Write-Host "Executing script as Administrator for user $userName"
}

Set-Location $scriptPath

function Main
{
    #Ensure Hyper-V is enabled
    $hyperV = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V
    if($hyperV.State -ne "Enabled")
    {    
        Write-Host "Hyper-V is not enabled on this machine. Attempting to enable Hyper-V"
        try
        {
            Install-WindowsFeature -Name Hyper-V -IncludeManagementTools -Restart
        }
        catch
        {
            Write-Host "Could not install Hyper-V feature on this machine..."$_
            Exit $LASTEXITCODE
        }
        Write-Host "Hyper-V has been installed now. Restarting machine"
    }
    else
    {
        Write-Host "Hyper-V already installed on this machine."
    }

    #Ensure Hyper-V Powershell is enabled
    $hyperVPowershell = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-PowerShell
    $hyperVManagementPowershell = Get-WindowsOptionalFeature -Online -FeatureName Microsoft-Hyper-V-Management-PowerShell
    if($hyperVPowershell.State -ne "Enabled" -and $hyperVManagementPowershell.State -ne "Enabled")
    {    
        Write-Host "Hyper-V-Powershell is not enabled on this machine. Attempting to enable Hyper-V-Powershell"
        try
        {
            Install-WindowsFeature -Name Hyper-V-PowerShell
        }
        catch
        {
            Write-Host "Could not install Hyper-V-PowerShell feature on this machine..."$_
            Exit $LASTEXITCODE
        }
        Write-Host "Hyper-V-PowerShell has been installed now"
    }
    else
    {
        Write-Host "Hyper-V-PowerShell already installed on this machine."
    }

    #Ensure Hyper-V module is enabled
    if (-not(Get-Module -ListAvailable Hyper-V))
    {
        Import-Module Hyper-V
        if (Get-Module -ListAvailable Hyper-V)
        {
            Write-Host "Hyper-V Module is now imported on this machine."
        }
        else
        {
            throw "Could not import Hyper-V module on this machine..."
        }
    } 
    else
    {
        Write-Host "Hyper-V Module already exists on this machine."
    }

    #Ensure AzureRm module is installed
    $azureRM = Get-InstalledModule -Name "AzureRM"
    if($null -eq $azureRM -or $azureRM -eq "")
    {
        Install-Module -Name AzureRM -Scope CurrentUser -Repository PSGallery -Confirm:$False -Force -AllowClobber

        $azureRM = Get-InstalledModule -Name "AzureRM"
        if($null -eq $azureRM -or $azureRM -eq "")
        {
            throw "Could not install AzureRM on machine"
        }
        else
        {
            Write-Host "AzureRM module is now installed on this machine"
        }
    }
    else
    {
        Write-Host "AzureRM module is already installed on this machine"
    }

    #Check for Azure connection
    $azureRmContext = Get-AzureRmContext
    if($null -eq $azureRmContext -or $azureRmContext -eq "")
    {
        throw "No Azure account connection on this machine"
    }
    else
    {
        Write-Host "Azure connection available on this machine"
    }
}

Main