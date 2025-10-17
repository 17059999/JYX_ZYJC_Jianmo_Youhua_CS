#-----------------------------------------------------------------------------
# Script  : CreateShareFolder
# Usage   : Create a folder and share it to everyone.
# Params  : -ShareFolderPath <string>: The folder path to be shared. Must be 
#           absolute path.
# Remark  : If the folder with the same name already exists, that folder will
#           be shared, and it will not be overwritten.
#-----------------------------------------------------------------------------
Param
(
    [Parameter(Mandatory=$true)]
	[ValidateNotNullOrEmpty()]
    [ValidateScript({[System.IO.Path]::IsPathRooted($_)})]
    [String]$ShareFolderPath
)

try
{ 
    # Create new folder
    if(!(Test-Path -Path $ShareFolderPath))
    {
        New-Item -Path $ShareFolderPath -ItemType Directory -ErrorAction Stop
    }
    # User the folder name as the share name
    $ShareName = [System.IO.Path]::GetFileName($ShareFolderPath)

    # Share the folder to everyone
    $Command = "net.exe share $ShareName=$ShareFolderPath /grant:everyone,FULL"
    cmd /c  $Command 2>&1 | Write-Host
    # If the folder has already been shared, net share will return 2
    if($LASTEXITCODE -eq 2)
    {
        Write-Host "The name has already been shared."
    }
    elseif($LASTEXITCODE -ne 0)
    {
        throw "Error happened in executing $Command. Return value: $LASTEXITCODE"
    }
}
catch
{
    throw "Failed to create share folder. Error happeded: " + $_.Exception.Message
}


