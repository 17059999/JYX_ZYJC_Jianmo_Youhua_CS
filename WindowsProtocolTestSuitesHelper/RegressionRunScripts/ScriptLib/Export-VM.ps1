#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Export-VM.ps1
## Purpose:        Export a VM into a folder.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

Param(
$VMName       ="Win7-x86-01", 
$VMPath       ="D:\VMTest\",
$VHDSrcPath   ="D:\Win7VHD\Enterprise\6731.1.080613-2011\WIN7-x86-01.vhd"
)

#----------------------------------------------------------------------------
# Print exection information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Export-VM.ps1]..." -foregroundcolor cyan
Write-Host "`$VMName     = $VMName" 
Write-Host "`$VMPath     = $VMPath" 
Write-Host "`$VHDSrcPath = $VHDSrcPath" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Create a VM and mount a VHD to  the vitual matchine"
    Write-host
    Write-host "Example: Create-VM.ps1 WIN7-x86-01 D:\VM"
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

Function Check-ReturnStatus 
{Param ($Result, $task) 

    $returnedValue = $Result.ReturnValue 
    #Return success if the return value is "0"
    if ($returnedValue -eq 0)
    {
        write-host "Task: $task finished." -foregroundcolor Green
    } 
 
    #If the return value is not "0" or "4096" then the operation failed
    elseIf ($returnedValue -ne 4096)
    {
        write-host "Failed to execute task: $task with return value of $returnedValue." -foregroundcolor Red
        throw "Task Failed"
    }
    else
    {  
        #Get the job object
        $job=[WMI]$Result.job
        #Provide updates if the jobstate is "3" (starting) or "4" (running)
        while ($job.JobState -eq 3 -or $job.JobState -eq 4)
        {
            write-host $job.PercentComplete"% completed."  
            start-sleep 1

            #Refresh the job object
            $job=[WMI]$Result.job
        }

        #A jobstate of "7" means success
        if ($job.JobState -eq 7)
        {
            write-host "Task: $task finished."   -foregroundcolor Green
        }
        else
        {
            write-host "Failed to execute task: $task"   -foregroundcolor Red
            write-host "ErrorCode:" $job.ErrorCode   -foregroundcolor Red
            write-host "ErrorDescription" $job.ErrorDescription -foregroundcolor Red
            throw "Task Failed"
        }
    }
}


#----------------------------------------------------------------------------
# Clean folder
#----------------------------------------------------------------------------
$VMFolderOfThisVM = $VMPath+$VMName
if (Test-Path( $VMFolderOfThisVM  ) )
{
    Write-Host "This VM already exists in the VM folder. Delete $VMFolderOfThisVM firstly..." -foregroundcolor Yellow
    cmd /c rd $VMFolderOfThisVM  /s /q
}

#----------------------------------------------------------------------------
# Get the VirtualSystemManagementService object
#----------------------------------------------------------------------------
$HyperVServer = "."
$VSManagementService = gwmi MSVM_VirtualSystemManagementService -namespace "root\virtualization" -computername $HyperVServer
$query = "SELECT * FROM Msvm_ComputerSystem WHERE Caption='Virtual Machine' AND ElementName='" + $VMName + "'"
$VM = Get-WmiObject -namespace "Root\Virtualization" -query $query

#----------------------------------------------------------------------------
# Shutdown VM
#----------------------------------------------------------------------------
Write-Host "Shutdown VM..."
$ShutDownComponent = get-wmiobject -computername $vm.__server -namespace "root\virtualization" -query  "SELECT * FROM Msvm_ShutdownComponent WHERE SystemName='$($vm.name)' "
If ($ShutDownComponent -ne $null) 
{
    $result = $ShutDownComponent.InitiateShutdown($true, "Shutdown system for export") 
    Check-ReturnStatus $result "Creating snapshot for the VM"

    If ($result.returnValue -eq 0) 
    {
        write-host "Shutdown of '$($vm.elementName) ' started."
    }
    else 
    {
        write-host "Attempt to shutdown '$($vm.elementName)' failed with code $($result.returnValue)."
    }
}
else
{
    write-host "Could not get shutdown component for '$($vm.elementName)'."
}

#----------------------------------------------------------------------------
# Set the VHD RO
#----------------------------------------------------------------------------
Write-Host "Set the VHD as ReadOnly: $VHDSrcPath..."
attrib +R $VHDSrcPath

#----------------------------------------------------------------------------
# Export the VM
#----------------------------------------------------------------------------
write-host "Start to export this VM to $VMPath ..."
sleep 30
$result = $VSManagementService.ExportVirtualSystem($VM.__PATH, $True, $VMPath)
Check-ReturnStatus $result "Exporting VM"

#----------------------------------------------------------------------------
# Print verification and exit information
#----------------------------------------------------------------------------
Write-Host "Verifying [Export-VM.ps1]..." -foregroundcolor yellow
Write-Host "EXECUTE [Export-VM.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit