#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

#-----------------------------------------------------------------------------
# Function: Get-RemoteSession
# Usage   : Get Remote seesion.
# Params  : [string]$FullUserName: domain name and username.
#           [string]$UserPassword : The password of the FullUserName.
#           [string]$RemoteIP  : The ip addreess of remote machine.
#-----------------------------------------------------------------------------
Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$FullUserName, 

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$UserPassword,
    
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$RemoteIP
)

#check if Remote machine is in TrustedHosts
$originalValue = (get-item WSMan:\localhost\Client\TrustedHosts).value
[string]$originalValue = $originalValue.Replace("*","");

if($originalValue.Length -gt 0){
    if($originalValue -match "$RemoteIP,+"){
        Write-Host "$RemoteIP already exists in trust hosts list"
    }else{
        $originalValue = $originalValue + ",$RemoteIP"
        Write-Host "Add $RemoteIP to Trusted Hosts"
        set-item WSMan:\localhost\Client\TrustedHosts -Value $originalValue -force
    }
}else{
    Write-Host "Add $RemoteIP to Trusted Hosts"
    set-item WSMan:\localhost\Client\TrustedHosts -Value $RemoteIP -force
}

$retryCount = 1
While($retryCount -lt 10)
{
    Try
    {
        $RemoteCredential = New-Object System.Management.Automation.PSCredential -ArgumentList $FullUserName,(ConvertTo-SecureString $UserPassword -AsPlainText -Force)
        Write-Host "Try To Connect to remote machine $RemoteIP"
        $RemoteSession = New-PSSession -ComputerName $RemoteIP -Credential $RemoteCredential -ErrorAction Stop
            
        if($RemoteSession -eq $null)
        {
            Write-Host "Failed to connect to $remoteIP, Retry $retryCount" -ForegroundColor Yellow
            Start-Sleep -s 60
            $retryCount++
        }
        else{
            Write-Host "Connected to remote session"
			break
        }
    }
    Catch
    {
        $ErrorMessage = $_.Exception.Message
        $errorMsg = "Failed to connect to $remoteIP. Retry count $retryCount, The error message was $ErrorMessage"

        Write-Host $errorMsg -ForegroundColor Yellow
        Start-Sleep -s 20
        $retryCount++
    }
}

IF($RemoteSession -eq $null)
{
    $errorMsg = "Failed to connect to $remoteIP with userName: $FullUserName, password: $UserPassword after retry $retryCount times"
    Write-Error $errorMsg
}

return $RemoteSession