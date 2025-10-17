###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

param(
	[string]$diskPath
)

#--------------------------------------------------------------------------------------------------
# Script to print Windows editions available in an ISO file
#--------------------------------------------------------------------------------------------------
function PrintISOEdition([string]$SourcePath)
{
    # Mount ISO file and get the path to the WIM file.
    if ($null -ne $SourcePath -and ([IO.FileInfo]$SourcePath).Extension -ilike ".ISO")
    {
        $isoPath = (Resolve-Path $SourcePath).Path

        Write-Host "Opening ISO $(Split-Path $isoPath -Leaf)..."
        Mount-DiskImage -ImagePath $isoPath -StorageType ISO
        Get-PSDrive -PSProvider FileSystem | Out-Null #Bugfix to refresh the Drive-List
        # Refresh the DiskImage object so we can get the real information about it.  I assume this is a bug.
        $openIso     = Get-DiskImage -ImagePath $isoPath
        $driveLetter = (Get-Volume -DiskImage $openIso).DriveLetter

        # Check to see if there's a WIM file we can muck about with.
        Write-Host "Looking for $($SourcePath)..."

        if (Test-Path -Path "$($driveLetter):\sources\install.wim") {
            $SourcePath  = "$($driveLetter):\sources\install.wim"
        }
        elseif (Test-Path -Path "$($driveLetter):\sources\install.esd")
        {
            $SourcePath  = "$($driveLetter):\sources\install.esd"
        }
        else
        {
            throw "The specified ISO does not appear to be valid Windows installation media."
        }
    }
    else
    {
        throw "Only ISO file is supported"
    }

    $SourcePath  = (Resolve-Path $SourcePath).Path
    $WindowsImage = Get-WindowsImage -ImagePath $SourcePath
    Write-Host "Available edition names are: "
    $WindowsImage
	Dismount-DiskImage $isoPath
}

PrintISOEdition $diskPath