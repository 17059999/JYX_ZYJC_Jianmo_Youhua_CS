#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Createdc.ps1
## Purpose:        Script to create domain controller.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2012 and later
## 
##############################################################################

# Function to Control Writing Information to the screen
Function Write-Log
{
    Param ([Parameter(ValueFromPipeline=$true)] $text	)

    $timeString = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
    $message = "[$timeString] $text"
    $osbuildnum= "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
    if([double]$osbuildnum -eq [double]"6.3")
    {
        # WinBlue issue: Start-Transcript cannot write the log printed out by Write-Host, as a workaround, use Write-output instead
        # Write-Output does not support color
        "$message" | Out-Host
    }
    else
    {
        Write-Host -NoNewline "$message`r`n" 
    }

}


# Main body of the script
#=================================================

# Start Logging
Start-Transcript -Path c:\Temp\CreateDC.log -Append -Force

# Get the XML file
[xml]$setup = Get-Content c:\temp\Protocol.xml

# Determine our Server
$name = Get-Content c:\temp\name.txt
$server = $setup.lab.servers.vm | where {$_.name -eq $name}
if([System.String]::IsNullOrEmpty($server.domain))
{   
     $domainName = "contoso.com"    
}
else
{
	$domainName = $server.domain
}

$userName = "IOLab"

if([System.String]::IsNullOrEmpty($server.username ))
{
    if(![System.String]::IsNullOrEmpty($setup.lab.core.username))
    {	
        $userName = $setup.lab.core.username
    }
}
else
{
	$userName = $server.username
}

if([System.String]::IsNullOrEmpty($server.password))
{
    if([System.String]::IsNullOrEmpty($setup.lab.core.password))
    {	
        $adminPwd = "Password01!"
    }
    else
    {
        $adminPwd = $setup.lab.core.password
    }	

}
else
{
	$adminPwd = $server.password
}

# Promote DC
Write-Log "Promoting this computer to DC." 
c:\temp\PromoteDomainController.ps1 -DomainName $domainName -AdminPwd $adminPwd -AdminUser $userName

Write-Log "Setting auto logon." 
c:\temp\Set-AutoLogon.ps1 -Domain $domainName -Username "$domainName\$userName" -Password $adminPwd

# Stop the transcript
Stop-Transcript -ErrorAction SilentlyContinue