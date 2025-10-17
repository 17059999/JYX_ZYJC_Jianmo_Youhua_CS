#-----------------------------------------------------------------------------
# Script  : CreateShortcut
# Usage   : Create a shortcut at the given location and points to a given
#           target path.
# Params  : -ShortcutPath <string>: Where the shortcut will be located.
#           -TargetPath <string>: Where the shortcut points to. Must be 
#           absolute path.
#           [-Arguments <string>]: Arguments that passed to the target program.
#-----------------------------------------------------------------------------
Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$ShortcutPath,

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [ValidateScript({
        if(![System.IO.Path]::IsPathRooted($_))
        {
            throw "Argument TargetPath should be absolute path"
        }
        return $true
    })]
    [string]$TargetPath,

    [parameter(Mandatory=$false)]
    [ValidateNotNullOrEmpty()]
    [string]$Arguments
)

try
{
    $WshShell = New-Object -ComObject WScript.Shell
    $Shortcut = $WshShell.CreateShortcut($ShortcutPath)
    if($Shortcut -eq $null)
    {
        throw "Failed to create shortcut at the given path"
    }

    $Shortcut.TargetPath = $TargetPath
    if($Arguments -ne $null)
    {
        $Shortcut.Arguments = $Arguments
    }
    $Shortcut.Save()
}
catch
{
    throw "Error happened while executing " + $MyInvocation.MyCommand.Name `
        + ": " + $_.Exception.Message
}