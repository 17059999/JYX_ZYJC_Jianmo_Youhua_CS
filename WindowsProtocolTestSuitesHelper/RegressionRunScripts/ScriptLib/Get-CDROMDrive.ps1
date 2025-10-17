#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-CDROMDrive.ps1
## Purpose:        Get the CD-ROM Drive letter of a remote or local computer.
## Version:        1.0 (10 Nov, 2008)
##
##############################################################################

param(
[string]$computerName=".",
[string]$userName=$null,
[string]$password=$null,
[int]$driveIndex=1
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-CDROMDrive.ps1] ..." -foregroundcolor cyan
Write-Host "`$computerName = $computerName"
Write-Host "`$userName     = $userName"
Write-Host "`$password     = $password"
Write-Host "`$driveIndex   = $driveIndex"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: This script will get the CD-ROM drive letter of a remote or local computer."
    Write-host
    Write-host "Example: Get-CDROMDrive.ps1 SUT01 administrator Password01!"
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
if ($computerName -eq $null -or $computerName -eq "")
{
    Throw "Parameter computerName is required."
}

#----------------------------------------------------------------------------
# Using global username/password when caller doesnot provide.
#----------------------------------------------------------------------------
if ($userName -eq $null -or $userName -eq "")
{
    $userName = $global:usr
    $password = $global:pwd
}
#----------------------------------------------------------------------------
# Make username prefixed with domain/computername
#----------------------------------------------------------------------------
if ($userName.IndexOf("\") -eq -1)
{
    if ($global:domain  -eq $null -or $global:domain -eq "")
    {
        $userName = "$computerName\$userName"
    }
    else
    {
        $userName = "$global:domain\$userName"
    }
}
#----------------------------------------------------------------------------
# Convert the password to a SecureString
#----------------------------------------------------------------------------
if($password -ne $null)
{
    $securePwd  = New-Object System.Security.SecureString
    for ($i = 0; $i -lt $password.Length; $i++)
    {
        $securePwd.AppendChar($password[$i]);
    }
}
if(($userName -ne $null) -and ($securePwd -ne $null))
{
    $credential = New-Object System.Management.Automation.PSCredential($userName, $securePwd) 
}

#----------------------------------------------------------------------------
# Wait the remote computer is started up
#----------------------------------------------------------------------------
if($computerName -ne ".")
{
    $disconnectCmd = "net.exe use \\$computerName\IPC$ /delete /y      1>>netusesuc.tmp.log 2>>netuseerr.tmp.log"
    $connectCmd    = "net.exe use \\$computerName\IPC$ $password /User:$userName 1>>netusesuc.tmp.log 2>>netuseerr.tmp.log"
    cmd /c $disconnectCmd 
    cmd.exe /c $connectCmd
    if ($lastExitCode -ne 0)
    {
        Write-Host "$computerName is not started yet..."  -foregroundcolor Yellow
        cmd /c $disconnectCmd 
        .\WaitFor-ComputerReady.ps1 $computerName  $userName $password 
    }
    cmd /c $disconnectCmd
}

#----------------------------------------------------------------------------
# Wait the computer RPCServer is online
#----------------------------------------------------------------------------
Write-Host "Try to connect to the RPC server of $computerName ..."
$waitTimeout = 600
$DiskObjs = $null
$retryCount = 0
for (; $retryCount -lt $waitTimeout/2; $retryCount++ ) 
{
    #get local host CDROM Drive
    if($computerName -eq ".")
    {
        $DiskObjs = get-wmiobject win32_LogicalDisk -computer .
    }
    #get remote computer CDROM Drive
    elseif(($credential -ne $null) -and ($credential -ne "") )
    {
        $DiskObjs = get-wmiobject win32_LogicalDisk -computer $computerName -Credential $credential 
    }
    else
    {
        $DiskObjs = get-wmiobject win32_LogicalDisk -computer $computerName
    }
    if($DiskObjs -ne $null)
    {
        break;  
    }    
    $NoNewLineIndicator = $True
    if ( $retryCount % 60 -eq 59 )
    {
       $NoNewLineIndicator = $False
    }
    Write-host "." -NoNewLine:$NoNewLineIndicator -foregroundcolor White
    
    Start-Sleep -s 2  # Sleep for 2 seconds [System.Threading.Thread]::Sleep(2000)
}
if ($DiskObjs -eq $null)
{
    Throw "Connect to remote computer $computerName  failed."
}

Write-host "." -foregroundcolor Green
Write-Host "The RPCServer of $computerName is started now."

#----------------------------------------------------------------------------
# Get the CD-ROM drive letter
#----------------------------------------------------------------------------
$cdRomDrive = $null
[int]$index = 0
foreach($disk in $DiskObjs)
{
    if($disk.Drivetype -eq 5)
    {
        $cdRomDrive = $disk.Name
        $index++
        if($index -eq $driveIndex)
        {
            break
        }
    }
}

if (($cdRomDrive -ne $null) -and ($cdRomDrive -ne "") -and ($index -eq $driveIndex))
{
    Write-Host "The CD-ROM drive for $computerName is $cdRomDrive." -foregroundcolor Green
}
elseif(($cdRomDrive -ne $null) -and ($cdRomDrive -ne ""))
{
    $cdRomDrive=$null
    Write-Host "The Index for CD-ROM drive of $computerName is incorrect." -foregroundcolor Yellow
}
else
{
    Write-Host "Cannot get the CD-ROM drive for $computerName" -foregroundcolor Yellow
}

#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Get-CDROMDrive.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor Green

return $cdRomDrive