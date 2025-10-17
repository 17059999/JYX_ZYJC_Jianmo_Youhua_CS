#############################################################################
## Copyright (c) Microsoft. All rights reserved.
##
## Microsoft Windows PowerShell Scripting
## File:    PSRemotingWindowsConfig.ps1
## Purpose: This script sets up a Windows SUT for PS-Remoting over SSH using 
## a public/private key pair for authentication.
##
#############################################################################

#Install/enable Open-SSH client on Windows if it isn't already.
Add-WindowsCapability -Online -Name OpenSSH.Client~~~~0.0.1.0

#Install/enable Open-SSH server on Windows if it isn't already.
Add-WindowsCapability -Online -Name OpenSSH.Server~~~~0.0.1.0

#Restart the sshd service and set it up to start automatically.
Restart-Service sshd
Set-Service sshd -StartupType Automatic

#Replace the sshd config file with a version containing the necessary config changes.
Copy-Item ".\sshd_config_win" "$($env:ProgramData)\ssh\sshd_config" -Force

#Restart the sshd service.
Restart-Service sshd

$directory = "$($env:ProgramData)\ssh"
$file = "administrators_authorized_keys"
$path = "$directory\$file"

#Create an empty file as a place holder for the authorized_keys file so permissions can be set on it if the file does not already exist.
#The Linux driver will populate this file with the public key after generating the key-pair.
if(![System.IO.File]::Exists($path))
{
    New-Item -Path $directory -Name $file -ItemType "file" -Value ""
}

$acl = get-acl $path

# Disable inheritance (copying permissions), so the modifications will actually take place.
$acl.SetAccessRuleProtection($true,$true)

# Perform the modification.
$acl | Set-Acl -Path $path

# Get the existing ACL again.
$acl = Get-Acl -Path $path

# Update permissions on file C:\ProgramData\ssh\administrators_authorized_keys where the public key from the linux driver will be kept.
# Remove permission for user: NT AUTHORITY\\Authenticated from the created file.
# Public-private key authentication fails if this permission isn't removed.
$identity = "NT AUTHORITY\Authenticated Users"
$rules = $acl.Access | Where-Object { $_.IdentityReference -eq $identity }
foreach($rule in $rules) {
    $acl.RemoveAccessRule($rule)
}

# Perform the modification.
$acl | Set-Acl -Path $path