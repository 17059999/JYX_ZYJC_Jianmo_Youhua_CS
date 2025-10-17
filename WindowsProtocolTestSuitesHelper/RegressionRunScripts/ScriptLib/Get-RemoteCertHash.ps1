#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Get-RemoteCertHash.ps1
## Purpose:        Get certificate's thumbprint from a specified computer.
## Version:        1.0 (26 May, 2009)
##
##############################################################################

param(
[string]$certificateStoreName,
[string]$subject,
[string]$computerName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Get-RemoteCertHash.ps1] ..." -foregroundcolor cyan
Write-Host "`$certificateStoreName = $certificateStoreName"
Write-Host "`$subject              = $subject"
Write-Host "`$computerName         = $computerName"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Get certificate's thumbprint from the computer specified by parameter `$computerName,"
    write-Host "       Scripts will get the thumbprint from local certificate store, if `$computerName is NULL."
    Write-host "       Return value will be one of the following:"
    Write-host "       A GUID stands for thumbprint or `$NULL if fail to get certificate's thumbprint."
    Write-host
    Write-host "Example: $certHash = Get-RemoteCertHash.ps1 `"remote desktop`" `"CN=SUT01.contoso.com`" SUT01 "
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
if ($certificateStoreName -eq $null -or $certificateStoreName -eq "")
{
    Throw "Parameter `$certificateStoreName is required."
    return
}
if ($subject -eq $null -or $subject -eq "")
{
    Throw "Parameter `$subject is required."
    return
}
if ($computerName -eq $null -or $computerName -eq "")
{
    $computerName = $env:COMPUTERNAME
}

#----------------------------------------------------------------------------
# Start to retrieve the certificate thumbprint
#----------------------------------------------------------------------------
[String]$result = $null
$readOnly = [System.Security.Cryptography.X509Certificates.OpenFlags]"ReadOnly"
$localMachine = [System.Security.Cryptography.X509Certificates.StoreLocation]"LocalMachine"
$certStore = New-Object System.Security.Cryptography.X509Certificates.X509Store("\\$computerName\$certificateStoreName",$localMachine)
$certStore.Open($readOnly)
$certificates = $certStore.Certificates
foreach ($certificate in $certificates)
{
    if ($certificate.Subject -eq $subject)
    {
        $result = $certificate.Thumbprint
        Break
    }
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "The certificate thumbprint is: $result" -foregroundcolor Green
Write-Host "EXECUTE [Get-RemoteCertHash.ps1] SUCCEED." -foregroundcolor Green
return $result
