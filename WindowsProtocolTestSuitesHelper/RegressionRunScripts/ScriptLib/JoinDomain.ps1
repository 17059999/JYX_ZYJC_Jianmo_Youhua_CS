#-----------------------------------------------------------------------------
# Function: JoinDomain
# Usage   : Join the computer to a domain.
# Params  : [string]$domain  : The domain needs to be joined.
# Params  : [string]$username: The user name with the permission to join the 
#                              domain.
#           [string]$password: The password for the username.
# Remark  : A reboot is needed after joining the domain.
#-----------------------------------------------------------------------------
Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]    
    [string]$Domain,
    
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()] 
    [string]$Username,
    
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()] 
    [string]$Password
)
    
try
{
	for($i=0;$i -lt 5;$i++)
	{
		try
		{
			Test-Connection -ComputerName $Domain -ErrorAction Stop
			break
		}
		catch
		{
			Write-Host "Wait 30 seconds then retry."
			sleep 30
		}
	}

    $credential = New-Object System.Management.Automation.PSCredential `
                    -ArgumentList "$domain\$username", (ConvertTo-SecureString $password -AsPlainText -Force) `
                    -ErrorAction Stop
    
    Add-Computer -Credential $credential -DomainName $domain -Restart:$false -Force -ErrorAction Stop
}
catch
{
    throw "Failed to join domain. Error happeded: " + $_.Exception.Message
}
