#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##############################################################################

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
# Install Windows Features
#----------------------------------------------------------------------------
Write-Info.ps1 "Install Windows Features"

Write-Info "Check FS-SMB installed" Client
$SMBState = Get-WindowsFeature FS-SMB1
if ($SMBState.Installstate -ne "Installed")
{
    Add-WindowsFeature FS-SMB1 -IncludeAllSubFeature -IncludeManagementTools 
}

$osVersion = Get-OSVersionNumber.ps1
if((gwmi win32_computersystem).partofdomain -and (Get-WindowsFeature "AD-Domain-Services").InstallState -eq "Installed")
{
    Write-Info.ps1 "Install RemoteAccess feature for domain controller"
    Import-Module ServerManager

    Write-Info.ps1 "Install RemoteAccess services"
    Add-WindowsFeature RemoteAccess -IncludeAllSubFeature -IncludeManagementTools
}
elseif([double]$osVersion -ge [double]"6.2")
{
    if($env:ComputerName -match "Storage")
    {
        Write-Info.ps1 "Install Windows Feature"
        Add-WindowsFeature File-Services,FS-iSCSITarget-Server
    }
    else
    {
        Write-Info.ps1 "OS is Windows Server 2012 or later."
        Import-Module Servermanager

        Write-Info.ps1 "Install features for FileServer"
        Add-WindowsFeature File-Services,FS-BranchCache,FS-VSS-Agent,BranchCache,RSAT-File-Services

        Write-Info.ps1 "Install DFS feature"
        Add-WindowsFeature FS-DFS-Namespace
        Add-WindowsFeature RSAT-DFS-Mgmt-Con
       
        Write-Info.ps1 "Install features for cluster"
        Add-WindowsFeature Failover-Clustering -IncludeAllSubFeature -IncludeManagementTools

        if($env:ComputerName -match "NODE01")
        {
            Write-Info.ps1 "Install Hyper-V tools to create vhd set file"
            Enable-WindowsOptionalFeature -online -FeatureName Microsoft-Hyper-V -all -NoRestart
            Add-WindowsFeature RSAT-Hyper-V-Tools -IncludeAllSubFeature
        }
    }		
}
else
{
    Write-Info.ps1 "OS is Windows 2008 R2 or lower version."
    Import-Module Servermanager

    Write-Info.ps1 "Install features for cluster"
    ServerManagerCmd.exe -install Failover-Clustering

    Write-Info.ps1 "Install DFS"
    Add-WindowsFeature FS-DFS,RSAT-FSRM-Mgmt
}

#----------------------------------------------------------------------------
# Ending
#----------------------------------------------------------------------------
Write-Info.ps1 "Completed install Windows features."
Stop-Transcript
exit 0