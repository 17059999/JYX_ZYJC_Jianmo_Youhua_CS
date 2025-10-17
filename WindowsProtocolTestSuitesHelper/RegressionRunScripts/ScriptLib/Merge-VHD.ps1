#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Merge-VHD.ps1
## Purpose:        Create a new file
## Version:        1.0 (25  Mar, 2010])
##
##############################################################################

param(
[string] $VMPath
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Merge-VHD.ps1]..." -foregroundcolor cyan
Write-Host "`$VMPath      = $VMPath"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This scripts will create a new file."
    Write-host
    Write-host "Example: Merge-VHD.ps1 D:\VM\Win7-x86-01"
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
#This methon requires the file set in driver C or D.
if(Test-Path $VMPath)
{
    [string]$mergePath = Get-ChildItem $VMPath -recurse *.avhd | %{$_.FullName}
    [string]$parentPath = Get-ChildItem $VMPath -recurse *.vhd | %{$_.FullName}
    .\Set-FileAttribute $parentPath "ReadOnly" $false
    [string[]]$mergePathArray= $mergePath.Split(" ")
    foreach($_ in $mergePathArray)
    {
        Write-Host "Merge $_ into $parentPath"
        #obtain the Msvm_ImageManagementService class
        $ImageMgtService = get-wmiobject -class "Msvm_ImageManagementService" -namespace "root\virtualization"
        
        # Create the fixed VHD
        $result = $ImageMgtService.MergeVirtualHardDisk($_,$parentPath)
        
        if($result.ReturnValue -eq 4096)
        {
            # A Job was started, and can be tracked using its Msvm_Job instance
            $job = [wmi]$result.Job
            # Wait for job to finish
            while($job.jobstate -lt "7"){$job.get()}
            # Return the Job's error code
            return $job.ErrorCode
            if($job.ErrorCode -eq 0)
            {
                Write-Host "Merge VHD successfully."
            }
            else
            {
                Write-Host "Merge VHD failed by $job.ErrorCode."
            }
        }
        if($result.ReturnValue -eq 0)
        {
            Write-Host "Merge VHD successfully."
        }
        elseif($result.ReturnValue -eq 32768)
        {
            Write-Host "Merge Failed!"
        }
        elseif($result.ReturnValue -eq 32769)
        {
            Write-Host "Access Denied!"
        }
        elseif($result.ReturnValue -eq 32772)
        {
            Write-Host "Timeout!"
        }
        elseif($result.ReturnValue -eq 32773)
        {
            Write-Host "Invalid parameter!"
        }
        elseif($result.ReturnValue -eq 32774)
        {
            Write-Host "System is in use!"
        }
        elseif($result.ReturnValue -eq 32779)
        {
            Write-Host "File Not Found!"
        }
        else
        {
            return $result.ReturnValue
        }
        #Failed (32768)
        #Access Denied(32769)
        #Not Supported (32770)
        #Status is unknown (32771)
        #Timeout (32772)
        #Invalid parameter (32773)
        #System is in use (32774)
        #Invalid state for this operation (32775)
        #Incorrect data type (32776)
        #System is not available (32777)
        #Out of memory (32778)
        #File Not Found (32779)
        # Otherwise, the method completed
        #return $result.ReturnValue
    } 
}
else
{
    Throw "The Folder do not exist."
}