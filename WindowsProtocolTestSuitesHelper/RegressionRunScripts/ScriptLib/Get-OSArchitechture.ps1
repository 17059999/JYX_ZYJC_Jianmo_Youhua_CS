#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-OSArchitecture.ps1
## Purpose:        Get the OS Architecture, return x86 or x64.
## Version:        1.1 (4 Aug, 2008)
##
##############################################################################

param(
[string]$computerName,
[string]$userName,
[string]$password
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-OSArchitecture.ps1] ..." -foregroundcolor cyan
Write-Host "`$computerName = $computerName"
Write-Host "`$userName     = $userName"
Write-Host "`$password     = $password"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Get the OS Architecture."
    Write-Host "       Return value will be one of the following:"
    Write-Host "       `"x86`", `"x64`", or `$NULL if fail to get OS architecture."
    Write-host
    Write-host "Example: `$osArch = .\Get-OSArchitecture.ps1 SUT01 Contoso.com\administrator Password01!"
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
    $computerName = $env:COMPUTERNAME
}

#----------------------------------------------------------------------------
# Using global username/password when caller doesnot provide.
#----------------------------------------------------------------------------
if ($computerName -ne $env:COMPUTERNAME)
{
    if ($userName -eq $null -or $userName -eq "")
    {
        $userName = $global:usr
        $password = $global:pwd
    }
}

#----------------------------------------------------------------------------
# Make username prefixed with domain/computername
#----------------------------------------------------------------------------
if ($computerName -ne $env:COMPUTERNAME)
{
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
}

#----------------------------------------------------------------------------
# Convert the password to a SecureString
#----------------------------------------------------------------------------
if ($computerName -ne $env:COMPUTERNAME)
{
    $securePwd  = New-Object System.Security.SecureString
    for ($i = 0; $i -lt $password.Length; $i++)
    {
        $securePwd.AppendChar($password[$i]);
    }
    $credential = New-Object System.Management.Automation.PSCredential($userName, $securePwd) 
}

#----------------------------------------------------------------------------
# Wait the computer is started up
#----------------------------------------------------------------------------
if ($computerName -ne $env:COMPUTERNAME)
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
Write-Host "Try to connect to computer $computerName ..."
$waitTimeout = 600
$osObj = $null
$retryCount = 0
for (; $retryCount -lt $waitTimeout/2; $retryCount++ ) 
{
    if($computerName -ieq $env:COMPUTERNAME)
    {
        $osObj = get-wmiobject win32_operatingsystem
    }
    else
    {
        $osObj = get-wmiobject win32_operatingsystem -computer $computerName -Credential $credential 
    }
    if($osObj -ne $null)
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
if ($osObj -eq $null)
{
    Throw "Connect to computer $computerName failed."
}

Write-host "." -foregroundcolor Green
Write-Host "Connection to computer $computerName created."

#----------------------------------------------------------------------------
# Starting to get OS architecture
#----------------------------------------------------------------------------
$gotFlag = $false
$returnValue = ""
if ($osObj -eq $null)
{
    Throw "Error: Cannot get WMI Object."
}

$archiStr = $osObj.OSArchitecture 
if ($archiStr -ne $null -and $archiStr -ne "") #Decide by Win32_OperatingSystem.OSArchitecture. This attribute don't exist in W2K3 and before.
{
    if ($archiStr.Contains("32"))
    {
        $returnValue = "x86"
        $gotFlag = $true
    }
    elseif ($archiStr.Contains("64"))
    {
        $returnValue = "x64"
        $gotFlag = $true
    }
}

if ($gotFlag -eq $false) #Decide by Win32_ComputerSystem.SystemType
{
    $csObj = $null
    if($computerName -ieq $env:COMPUTERNAME)
    {
        $csObj = get-wmiobject Win32_ComputerSystem 
    }
    else
    {
        $csObj = get-wmiobject Win32_ComputerSystem -computer $computerName -Credential $credential 
    }
    if ($csObj -eq $null)
    {
        Throw "Error: Cannot get WMI Object."
    }

    $stStr = $csObj.SystemType
    if ($stStr -ne $null -and $stStr -ne "")
    {
        if ($stStr.ToUpper().Contains("X86") -or $stStr.Contains("32"))
        {
            $returnValue = "x86"
            $gotFlag = $true
        }
        elseif ($stStr.ToUpper().Contains("X64") -or $stStr.Contains("64"))
        {
            $returnValue = "x64"
            $gotFlag = $true
        }
    }
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Get-OSArchitecture.ps1] ..." -foregroundcolor Yellow
if ($gotFlag -eq $false)
{
    Write-Host "Cannot get OS architecture." -ForegroundColor Yellow
    return $null
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "OS architecture is: $returnValue" -foregroundcolor Green
Write-Host "EXECUTE [Get-OSArchitecture.ps1] SUCCEED." -foregroundcolor Green
return $returnValue
