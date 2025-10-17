#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Fix-CertificateTemplate.ps1
## Purpose:        Fix certificate template in CA to accept Subject Name in request.
## Version:        1.0 (1 Sep, 2008)
##
##############################################################################

param(
[string]$templateName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Fix-CertificateTemplate.ps1] ..." -foregroundcolor cyan
Write-Host "`$templateName = $templateName"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Fix any certificate template that needs to accept the subject and subject alternative name as part of a certificate request."
    Write-host "Parm1: Certificate template name. (Required)"
    Write-host
    Write-host "Example: Fix-CertificateTemplate.ps1  DomainControllerAuthentication"
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
if ($templateName -eq $null -or $templateName -eq "")
{
    Throw "Parameter `$templateName is required."
}

#----------------------------------------------------------------------------
# Fix the template
#----------------------------------------------------------------------------
Write-Host "Fixing template $templateName"
$rootDSE = [ADSI]"LDAP://rootDSE"
if ($rootDSE -eq $null)
{
    Throw "EXECUTE [Fix-CertificateTemplate.ps1] FAILED. Cannot get rootDSE."
}
$configDN = $rootDSE.configurationNamingContext

$targetTemplate = [ADSI]"LDAP://CN=$templateName,CN=Certificate Templates,CN=Public Key Services,CN=Services,$configDN"
$name = $targetTemplate.name
if ($name -eq $null)
{
    Throw "EXECUTE [Fix-CertificateTemplate.ps1] FAILED. Template do not exist in Active Directory."
}

[int]$flag = $targetTemplate.Get("msPKI-Certificate-Name-Flag")
if ($flag -ne 1)
{
    $targetTemplate.Put("msPKI-Certificate-Name-Flag", 1)
    $targetTemplate.SetInfo()
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Fix-CertificateTemplate.ps1] ..." -foregroundcolor Yellow
$flag = $targetTemplate.Get("msPKI-Certificate-Name-Flag")
if ($flag -ne 1)
{
    Write-Host "EXECUTE [Fix-CertificateTemplate.ps1] FAILED. Flag is: $flag" -foregroundColor Red
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
else
{
    Write-Host "EXECUTE [Fix-CertificateTemplate.ps1] SUCCEED." -foregroundcolor Green
}
