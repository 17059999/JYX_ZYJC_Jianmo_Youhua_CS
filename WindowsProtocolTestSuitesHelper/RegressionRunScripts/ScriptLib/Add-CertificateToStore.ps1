#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Add-CertificateToStore.ps1
## Purpose:        Add the certificate to store.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$certFileIn,
[string]$storeName = "my",
[string]$context
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Add-CertificateToStore.ps1] ..." -foregroundcolor cyan
Write-Host "`$certFileIn = $certFileIn"
Write-Host "`$storeName = $storeName"
Write-Host "`$context = $context"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Add the certificate to store."
    Write-host "Parm1: Certificate file to add. (Required)"
    Write-host "Parm2: Certificate store name.  (Optional, Default value:my, Legal value: my|root|CA|... see Certutil -store -? for more store names.)"
    Write-host "Parm3: Context for accepting the zresponse. (Optional. Legal value: user|enterprise|NULL )"
    Write-host "       user:       Use the Current User context for adding the response."
    Write-host "       enterprise: Use the local machine Enterprise context for adding the response."
    Write-host "       NULL:       Use the default context for adding the response."
    Write-host
    Write-host "Example: Add-CertificateToStore.ps1  myCert.cer"
    Write-host "         Add-CertificateToStore.ps1  myCert.cer  my  user"
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
if (($contextNullFlag -ne $true) -and (!($context.ToLower().Equals("user") -or $context.ToLower().Equals("enterprise"))))
{
    Throw "Parameter `$context should be user|enterprise|NULL."
}

#----------------------------------------------------------------------------
# Add the certificate
#----------------------------------------------------------------------------
Write-Host "Add the certificate to store ..."
if ( (Get-Item $certFileIn) -eq $null )
{
    Throw "Can not find cert file."
}

$result = ""
if ($contextNullFlag -eq $true) #default context
{ 
    $result = certutil.exe -addstore $storeName $certFileIn
}
else
{
    if ($context.ToLower().Equals("user")) #user context
    {
        $result = certutil.exe -user -addstore $storeName $certFileIn
    }
    else #machine context
    {
        $result = certutil.exe -enterprise -addstore $storeName $certFileIn
    }
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Add-CertificateToStore.ps1] ..." -foregroundcolor Yellow
if (($result -ne $null) -and (!($result[$result.Length-1].contains("successfully"))))  #When add cert successfully, result will be an array, its last element constais string: successfully
{
    Throw "EXECUTE [Add-CertificateToStore.ps1] FAILED. Error message: $result"
}
Write-Host "Aadd certificate sucessfully to $storeName." -foregroundcolor Green

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Add-CertificateToStore.ps1] SUCCEED." -foregroundcolor Green
