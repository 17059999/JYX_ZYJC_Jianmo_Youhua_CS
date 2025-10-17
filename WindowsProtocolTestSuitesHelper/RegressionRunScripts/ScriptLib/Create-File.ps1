#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-File.ps1
## Purpose:        Create a new file
## Version:        1.1 (26 Jun, 2008])
##
##############################################################################

param(
[string] $targetFile
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-File.ps1]..." -foregroundcolor cyan
Write-Host "`$targetFile = $targetFile" 

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This scripts will create a new file."
    Write-host
    Write-host "Example: Create-File.ps1 c:\target.txt"
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
if($targetFile.Length -eq [int]0)
{
    Throw "The file'name is empty."
}
else
{
    $ifParameterLegal = $false
    $tempDrivers = $targetFile.Split(':')[0]
    if(($tempDrivers -eq "C")-or ($tempDrivers -eq "c")-or 
       ($tempDrivers -eq "D")-or ($tempDrivers -eq "d")-or 
       ($tempDrivers -eq "E")-or ($tempDrivers -eq "e")-or 
       ($tempDrivers -eq "F")-or ($tempDrivers -eq "f")-or 
       ($tempDrivers -eq "G")-or ($tempDrivers -eq "g")-or 
       ($tempDrivers -eq "H")-or ($tempDrivers -eq "h"))
    {
        $ifParameterLegal = $true
    }
    else
    {
        Throw "You must set a Drive(C,D,E,F,G or H) for your new file."
    }
}

#----------------------------------------------------------------------------
# EXECUTION
#----------------------------------------------------------------------------
if($ifParameterLegal -eq $true)
{
    $ifFileExisting = $false
    [string[]] $fileArray = $targetFile.Split('\')
    for([int]$i = 0; $i -lt $fileArray.Count ; $i = $i+1)
    {
        $ifHaveSubFile = $false
        if($i -eq [int]0)
        {
            [string]$parentPathName =  $fileArray[$i]
        }
        else
        {
            $tempObjFiles = Get-Item ($parentPathName+"\*")
            foreach($tempObjFile in $tempObjFiles)
            {
                if($tempObjFile.Name -eq $fileArray[$i])
                {
                    $ifHaveSubFile = $true
                }
            }
            $parentPathName = $parentPathName + "\" + $fileArray[$i]
            if($ifHaveSubFile -eq $true)
            {
                Write-Host "The Path:"$parentPathName "has already existed."
            }
            else
            {
                [int]$TempCount = $fileArray.Count - 1
                if($i -ne $TempCount)
                {
                    new-item -Type Directory -Path $parentPathName | out-null
                }
                else
                {
                    if($fileArray[$i].Length -gt 0)
                    {
                        new-item -Type File -Path $parentPathName | out-null
                    }
                    Write-Host "Create the Path:"$parentPathName " for creating the target file."
                }
            }
        }
    }
    Write-Host "Succeed to create a new file with the name:" $targetFile "."
}
else
{
    Write-Host "The file's name "$targetFile" has mistake."}

#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Create-File.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit 0