#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Create-CertificateTemplate.ps1
## Purpose:        Create a certificate template from a INF file.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$machineDnsName,
[string]$CAName,
[string]$templateInfFile,
[string]$DCName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-CertificateTemplate.ps1] ..." -foregroundcolor cyan
Write-Host "`$machineDnsName = $machineDnsName"
Write-Host "`$CAName = $CAName"
Write-Host "`$templateInfFile = $templateInfFile"
Write-Host "`$DCName = $DCName"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Create a certificate template from a INF file, and add the template to Certificate Authority. After that, CA can issue certificate based on this template."
    Write-host "       You should provide a policy(.inf) file, which describs the template's behavior."
    Write-host "       After the template is created successfully by this script, you can request a certificate based on this template by using Request-Certificate.ps1."
    Write-host
    Write-host "Parm1: CA server's DNS name. (Required)"
    Write-host "Parm2: CA name. (Required)"
    Write-host "Parm3: Template policy file(.inf). (Required) "
    Write-host "       Refer to: "
    Write-host "Parm4: Domain Controller name. (Opthinal) "
    Write-host
    Write-host "Example: Create-CertificateTemplate.ps1  SUT01.contoso.com  contoso-SUT01-CA  myTemplate.inf"
    Write-host
    Write-host "Note: Type of the Certificate Authority should be Enterprise CA in order to support certificate template. A Stand along CA can not use template to issue certificate."
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
if ($machineDnsName -eq $null -or $machineDnsName -eq "")
{
    Throw "Parameter machineDnsName is required."
}
if ($CAName -eq $null -or $CAName -eq "")
{
    Throw "Parameter CAName is required."
}
if ($templateInfFile -eq $null -or $templateInfFile -eq "")
{
    Throw "Parameter templateInfFile is required."
}

#----------------------------------------------------------------------------
# Create template, and import to Active Directory
#----------------------------------------------------------------------------
Write-Host "Create certificate template by INF file:$templateInfFile ..."
$result = ""
if ($DCName -eq $null -or $DCName -eq "")
{
    $result = certutil.exe -dsaddtemplate $templateInfFile
}
else
{
    $result = certutil.exe -dc $DCName -dsaddtemplate $templateInfFile
}
if (!($result[$result.Length-1].contains("successfully")))
{
    Throw "EXECUTE [Create-CertificateTemplate.ps1] FAILED. Error message: $result"
}

#----------------------------------------------------------------------------
# Add the template to CA, so the CA can issue certificate based on this template
#----------------------------------------------------------------------------
if ($DCName -eq $null -or $DCName -eq "")
{
    $result = certutil.exe -config $machineDnsName\$CAName -setcatemplates +$templateName
}
else
{
    $result = certutil.exe -config $machineDnsName\$CAName -dc $DCName -setcatemplates +$templateName
}

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Create-CertificateTemplate.ps1] ..." -foregroundcolor Yellow

if (!($result[$result.Length-2].contains("successfully")))
{
    Throw "EXECUTE [Create-CertificateTemplate.ps1] FAILED. Error message: $result"
}
Write-Host "Create certificate template successfully. The CertSvc service may need to be restarted for changes to take effact" -foregroundcolor Green

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Create-CertificateTemplate.ps1] SUCCEED." -foregroundcolor Green

