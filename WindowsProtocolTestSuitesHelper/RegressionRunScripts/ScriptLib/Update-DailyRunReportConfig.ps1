#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##############################################################################
Param(
[string]$WorkingDir = "$env:SystemDrive\WinteropProtocolTesting",
[string]$ConfigFileName  = "LabDailyReport.config.xml",
[string]$MailSenderUsername = "pettest",
[string]$MailSenderPassword = "******"
)

#----------------------------------------------------------------------------
# Define Common Funcitons
#----------------------------------------------------------------------------
function ExitCode()
{ 
    return $MyInvocation.ScriptLineNumber 
}

Function TrimDoubleQuotationMarks([string]$var)
{
    $ret = $var
    if($ret -ne $null -and $ret.Length -ge 3)
    {
        if($ret[0] -eq "`"" -and $ret[$ret.Length-1] -eq "`"")
        {
            $ret = $ret.Substring(1,$ret.Length-2)
        }
    }
    return $ret
}

#----------------------------------------------------------------------------
# Check parameters
#----------------------------------------------------------------------------
if (!(Test-Path -Path $WorkingDir))
{
    Write-Host "WorkingDir $WorkingDir does not exist."
	exit ExitCode
}

$ConfigFileFullName = "$WorkingDir\Tools\ResultGatherer\Config\$ConfigFileName"
if (!(Test-Path -Path $ConfigFileFullName))
{
    Write-Host "Configure file $ConfigFileFullName does not exist."
	exit ExitCode
}

if ($MailSenderUsername -eq $null -or $MailSenderUsername -eq "")
{
    Write-Host "MailSenderUsername could not be null or empty."
	exit ExitCode
}

if ($MailSenderPassword -eq $null -or $MailSenderPassword -eq "")
{
    Write-Host "MailSenderPassword could not be null or empty."
	exit ExitCode
}


#----------------------------------------------------------------------------
# Modify Config file
#----------------------------------------------------------------------------
CMD /C attrib -s -h -r $ConfigFileFullName
if($lastexitcode -ne 0)
{
    Write-Host "Clear read only attribute failed for $ConfigFileFullName."
	exit ExitCode
}
[xml]$ConfigFileContent = Get-Content $ConfigFileFullName
if($ConfigFileContent -eq $null)
{
    Write-Host "Configure file is not valid: $ConfigFileFullName."
	exit ExitCode
}
$username = $ConfigFileContent.Configuration.Properties.Property | where {$_.name -eq "Mail.Sender.Username"}  
$username.value = TrimDoubleQuotationMarks $MailSenderUsername.Trim()                                                                                                                  
$password = $ConfigFileContent.Configuration.Properties.Property | where {$_.name -eq "Mail.Sender.Password"}   
$password.value = TrimDoubleQuotationMarks $MailSenderPassword.Trim()

$ConfigFileContent.Save($ConfigFileFullName)
if($lastexitcode -ne 0)
{
    Write-Host "Save configure file failed: $ConfigFileFullName."
	exit ExitCode
}

#----------------------------------------------------------------------------
# Stop logging and exit
#----------------------------------------------------------------------------
exit 0