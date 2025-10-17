#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Accept-CertificateResponse.ps1
## Purpose:        Accept the certificate issued by CA.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$certFileIn,
[string]$context
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Accept-CertificateResponse.ps1] ..." -foregroundcolor cyan
Write-Host "`$certFileIn = $certFileIn"
Write-Host "`$context = $context"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Accept the certificate issued by CA."
    Write-host "Parm1: Certificate file to accept. (Required)"
    Write-host "Parm2: Context for accepting the response. (Optional. Legal value: user|machine|NULL )"
    Write-host "       user:     Use the Current User context for accepting the response."
    Write-host "       machine:  Use the Local Machine context for accepting the response."
    Write-host "       NULL:     Use the default context for accepting the response."
    Write-host
    Write-host "Example: Accept-CertificateResponse.ps1  myCert.cer"
    Write-host "         Accept-CertificateResponse.ps1  myCert.cer machine"
    Write-host
    Write-host "Note: Accept the certificate means the response from CA will be accepted, and the cert will be add to store automatically. Then, you don not need to add the cert to store by other ways."
    Write-host "      If the cert responsed from CA has already accepted, or the cert file is no longer be related to a response, this script will fail. Refer to Add-CertificateToStore.ps1 for other ways to add cert to store."
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
if ($certFileIn -eq $null -or $certFileIn -eq "")
{
    Throw "Parameter `$certFileIn is required."
}
$contextNullFlag = $context -eq $null -or $context -eq ""
if (($contextNullFlag -ne $true) -and (!($context.ToLower().Equals("user") -or $context.ToLower().Equals("machine"))))
{
    Throw "Parameter `$context should be user|machine|NULL."
}

#----------------------------------------------------------------------------
# Accept the certificate
#----------------------------------------------------------------------------
Write-Host "Accept the certificate ..."
if ( (Get-Item $certFileIn) -eq $null )
{
    Throw "Can not find cert file."
}

$result = ""
if ($contextNullFlag -eq $true) #default context
{
    $result = certreq.exe -accept -q $certFileIn 
}
else
{
    if ($context.ToLower().Equals("user")) #user context
    {
        $result = certreq.exe -accept -user -q $certFileIn
    }
    else #machine context
    {
        $result = certreq.exe -accept -machine -q $certFileIn
    }
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Accept-CertificateResponse.ps1] ..." -foregroundcolor Yellow
if ($result -ne $null)  #When accept cert successfully, result will be NULL
{
    Throw "EXECUTE [Accept-CertificateResponse.ps1] FAILED. Error message: $result" 
}
Write-Host "Accept certificate sucessfully." -foregroundcolor Green

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Accept-CertificateResponse.ps1] SUCCEED." -foregroundcolor Green

