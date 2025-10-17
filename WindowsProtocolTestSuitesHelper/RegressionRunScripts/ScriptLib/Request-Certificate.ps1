#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Request-Certificate.ps1
## Purpose:        Generate a new certificate request, and submit to CA.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

param(
[string]$machineDnsName,
[string]$CAName,
[string]$policyFileIn,
[string]$certFileOut
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Request-Certificate.ps1] ..." -foregroundcolor cyan
Write-Host "`$machineDnsName = $machineDnsName"
Write-Host "`$CAName = $CAName"
Write-Host "`$policyFileIn = $policyFileIn"
Write-Host "`$certFileOut = $certFileOut"

#----------------------------------------------------------------------------
# Function: Show-ScriptUsage
# Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage: Generate a new certificate request, and submit to CA."
    Write-host "       You should provide a policy file, which records the cert's info you want to request."
    Write-host "       If the cert is issued, a output file will create. Then, you can either accept the resopnse(refer to Accept-CertificateResponse.ps1), or add the cert to store(refer to Add-CertificateToStore.ps1)."
    Write-host
    Write-host "Parm1: CA server's DNS name. (Required)"
    Write-host "Parm2: CA name. (Required)"
    Write-host "Parm3: Policy file(.inf) to request a certificate. (Required) "
    Write-host "       Refer to: http://technet2.microsoft.com/windowsserver/en/library/008acdeb-0650-4063-a9a2-1258b3229d4f1033.mspx?mfr=true"
    Write-host "Parm4: Output file for store the certificate issued by CA. (Required)"
    Write-host
    Write-host "Example: Request-Certificate.ps1  SUT01.contoso.com  contoso-SUT01-CA  myCert.inf  myCert.cer"
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
if ($policyFileIn -eq $null -or $policyFileIn -eq "")
{
    Throw "Parameter policyFileIn is required."
}
if ($certFileOut -eq $null -or $certFileOut -eq "")
{
    Throw "Parameter certFileOut is required."
}

#----------------------------------------------------------------------------
# Create request from Policy(INF) file
#----------------------------------------------------------------------------
Write-Host "Request certificate from $machineDnsName\$CAName by INF file:$policyFileIn ..."
$result = certreq.exe -new -q -f $policyFileIn .\tmpCert.req
$tmpReqFile = Get-Item .\tmpCert.req
if ($tmpReqFile -eq $NULL)
{
    Throw "EXECUTE [Request-Certificate.ps1] FAILED. Error message: $result" 
}

#----------------------------------------------------------------------------
# Submit request
#----------------------------------------------------------------------------
$result = certreq.exe -submit -q -f -config $machineDnsName\$CAName .\tmpCert.req $certFileOut
Remove-Item .\tmpCert.req -Force

#----------------------------------------------------------------------------
# Verifying the result
#----------------------------------------------------------------------------
Write-Host "Verifying [Request-Certificate.ps1] ..." -foregroundcolor Yellow
$tmpCertFile = Get-Item $certFileOut
if ($tmpCertFile -eq $NULL)
{
    Throw "EXECUTE [Request-Certificate.ps1] FAILED. Error message: $result" 
}
Write-Host "Get certificate successfully. $result. Certificate file: $certFileOut " -foregroundcolor Green

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Request-Certificate.ps1] SUCCEED." -foregroundcolor Green

