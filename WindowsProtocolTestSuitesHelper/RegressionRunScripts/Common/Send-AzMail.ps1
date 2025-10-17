##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################
###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Send-AzMail.ps1
## Purpose:        Send mail to designated users from designated host and port with customized subject and body
## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later.
## Input parameter:
##      SmtpHost       :  SMTP host used for sending email
##      SmtpPort       :  SMTP port used for sending email
##      SenderUsername :  Username of mail sender
##      SenderPassword :  Password of mail sender
##      SendTo         :  Mail receiver
##      MailSubject    :  Mail subject
##      MailBody       :  Mail body
###########################################################################################
param (
    [string]$SmtpHost,
    [string]$SmtpPort,
    [string]$SenderUsername,
    [Parameter(Mandatory = $TRUE)]
    [SecureString]$SenderPassword,
    [string]$SendTo,
    [string]$MailSubject = "Default Mail Subject",
    [string]$MailBody 
)
[System.Net.ServicePointManager]::SecurityProtocol = 'TLS12'

#The SMTPHost and SMTPPort and other details are not required for Azure Communication Service and will be refactored later.

Install-PackageProvider -Name NuGet -MinimumVersion 2.8.5.201 -Force

Install-Module -Name Az.Communication -Scope CurrentUser -Force -AllowClobber

Import-Module Az.Communication -Force

$emailArray = $SendTo -split ',' | ForEach-Object { $_.Trim() }
Write-Host $SendTo
$emailRecipientTo = @()

foreach ($email in $emailArray) {

    if (-not [string]::IsNullOrWhiteSpace($email)) {

        $_email = $email -split ':'

        $emailObject = @{
            Address = $_email[0]
            DisplayName = $_email[1]
        }

        $emailRecipientTo += $emailObject
    }
}

$message = @{
    ContentSubject = $MailSubject
    RecipientTo = @($emailRecipientTo)
    SenderAddress = 'DoNotReply@b50105b3-83c6-4a97-aa9c-f034139f5cb3.azurecomm.net'
    ContentHtml = $MailBody
    Header = $headers
    UserEngagementTrackingDisabled = $true
}

Send-AzEmailServicedataEmail -Message $Message -endpoint 'https://testsuitecommunication.unitedstates.communication.azure.com'
