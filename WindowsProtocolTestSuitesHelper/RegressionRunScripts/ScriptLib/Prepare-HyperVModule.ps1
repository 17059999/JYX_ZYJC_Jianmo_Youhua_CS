##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################

# Write out an Info Message to the screen
Function WriteInfo {
	Param ($txt)
	Write-Host (get-date).ToString() : $txt -ForegroundColor Green -BackgroundColor Black
}

# Get script path
$scriptPath = Split-Path $MyInvocation.MyCommand.Definition -parent

# Determine current OS
$script:osversion="Unsupported"
$osbuildnum= "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
if ($osbuildnum -eq "6.1") {$script:osversion="Windows Server 2008 R2"}
if ($osbuildnum -ge "6.2") {$script:osversion="Windows Server 2012"}
if ($osversion -eq "Unsupported") 
{
    WriteInfo "Unsupported OS $osbuildnum, will exit immediately"
    exit 1
} 
else 
{
	    WriteInfo ("Supported Host OS $osbuildnum")
}

if ($osversion -like "Windows Server 2008 R2*")  
{
    # Verify we have the hyperv module to import
    if (Get-Module -ListAvailable hyperv)
    {
	    WriteInfo "Found HyperV v1.0 Powershell module"
    } 
    else 
    {        
	    Robocopy "$scriptPath\..\hyperv" C:\Windows\System32\WindowsPowerShell\v1.0\Modules\hyperv /mir 
    }

    # Import the Hyperv Management Module
    if ( !(Get-Module hyperv) ) 
    {
    	WriteInfo ("Importing HyperV v1.0 Powershell module")
	    Import-Module Hyperv
    }

}

 if ($osversion -like "Windows Server 2012*") 
{
    # Install Hyperv Module if not exist
    Import-Module ServerManager
    $featureName = "RSAT-Hyper-V-Tools"
    $feature = Get-WindowsFeature | where {$_.Name -eq "$featureName"}
    if($feature.Installed -eq $false)
    {
        Add-WindowsFeature -Name $featureName -IncludeAllSubFeature -IncludeManagementTools
        sleep 5
    }
    
    # Import Hyper-V module
    if ( (Get-Module -ListAvailable Hyper-V) )
    {
	    Import-Module Hyper-V
	    WriteInfo "Importing HyperV Powershell module"
    }
    else 
    {
	    WriteInfo "HyperV Powershell module is missing, please check OS Installation."
	    exit 1
    }
}
  
#----------------------------------------------------------------------------
# Complete and exit
#----------------------------------------------------------------------------
exit 0