#-----------------------------------------------------------------------------
# Function: EnableIISWindowsAuthentication
# Usage   : Disable Anonymous Authentication and enbale Windows Authentication
#           in IIS.
#-----------------------------------------------------------------------------

# Disable AnonymousAuthentication
Set-WebConfigurationProperty -Filter /System.WebServer/Security/Authentication/AnonymousAuthentication `
    -Name Enabled -Value False -ErrorVariable Err -ErrorAction SilentlyContinue
# If failed to disable AnonymousAuthentication, show prompt but not throw exception
if ($Err -ne $null)
{
    Write-Host "Fail to disable AnonymousAuthentication. " + $Err
}

try
{
	# Enable WindowsAuthentication
	Set-WebConfigurationProperty -Filter /System.WebServer/Security/Authentication/WindowsAuthentication `
	    -Name Enabled -Value True -ErrorAction Stop
}
catch
{
	throw "Failed to enable IIS Windows authentication. Error happened: " + $_.Exception.Message
}