#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-VHD.ps1
## Purpose:        Create a new VHD
## Version:        1.0 (6  Apr, 2010])
##
##############################################################################

param(
[string] $VHDPath,
[string] $VHDType = "Dynamic",
[string] $VHDSize = "10GB",
[string] $ParentVHDPath
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-VHD.ps1]..." -foregroundcolor cyan
Write-Host "`$VHDPath        = $VHDPath"
Write-Host "`$VHDType        = $VHDType"
Write-Host "`$VHDSize        = $VHDSize"
Write-Host "`$ParentVHDPath  = $ParentVHDPath"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This scripts will create a new VHD."
    Write-host
    Write-host "Example: Create-VHD.ps1 D:\VM\Win7-x86-01.vhd Dynamic 10GB"
    Write-host "         Create-VHD.ps1 D:\VM\Win7-x86-01.vhd Fixed 10MB"
    Write-host "         Create-VHD.ps1 D:\VM\Win7-x86-01.vhd Different 10GB D:\VM\Parent\Win7-x86-01.vhd"
    Write-host
}

#----------------------------------------------------------------------------
# Show help if required
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    Show-ScriptUsage 
    return
}

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
$VHDSize = $VHDSize.ToUpper()
if(Test-Path $VHDPath)
{
    Throw "The file have been exist."
}
elseif((!$VHDSize.Endswith("GB")) -and (!$VHDSize.Endswith("MB")) -and (!$VHDSize.Endswith("KB")))
{
    Throw "$VHDSize is not correct. "
}
else
{
    $NewVHDPath = $VHDPath.Substring(0,$VHDPath.LastIndexof("\"))
    if(!(Test-Path $NewVHDPath))
    {
        New-Item $NewVHDPath -ItemType directory
    }
    #obtain the Msvm_ImageManagementService class
    $ImageMgtService = get-wmiobject -class "Msvm_ImageManagementService" -namespace "root\virtualization"
    
    if($VHDType -eq "Dynamic")
    {
        $result = $ImageMgtService.CreateDynamicVirtualHardDisk($VHDPath,$VHDSize)
    }
    elseif($VHDType -eq "Fixed")
    {
        $result = $ImageMgtService.CreateFixedVirtualHardDisk($VHDPath,$VHDSize)
    }
    elseif($VHDType -eq "Different")
    {
        if(Test-Path $ParentVHDPath)
        {
            $result = $ImageMgtService.CreateDifferencingVirtualHardDisk($VHDPath,$ParentVHDPath)
        }
        else
        {
            Throw "The file don't exist."
        }
    }
    if($result.ReturnValue -eq 4096)
    {
        # A Job was started, and can be tracked using its Msvm_Job instance
        $job = [wmi]$result.Job
        # Wait for job to finish
        while($job.jobstate -lt "7"){$job.get()}
        # Return the Job's error code
        if($job.ErrorCode -eq 0)
        {
            Write-Host "Create VHD successfully."
        }
        else
        {
            Write-Host "Create VHD failed by $job.ErrorCode."
        }
    }
    else
    {
        Write-Host "Create VHD failed by $result.ReturnValue."
    }
    #----------------------------------------------
    #Completed with No Error (0)
    #Method Parameters Checked - JobStarted (4096)
    #Failed (32768)
    #Access Denied (32769)
    #Not Supported (32770)
    #Status is unknown (32771)
    #Timeout (32772)
    #Invalid parameter (32773)
    #System is in use (32774)
    #Invalid state for this operation (32775)
    #Incorrect data type (32776)
    #System is not available (32777)
    #Out of memory (32778)
    #---------------------------------------------
}