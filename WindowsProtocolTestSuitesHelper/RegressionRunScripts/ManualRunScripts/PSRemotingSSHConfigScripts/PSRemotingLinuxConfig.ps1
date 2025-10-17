#############################################################################
## Copyright (c) Microsoft. All rights reserved.
##
## Microsoft Windows PowerShell Scripting
## File:    PSRemotingLinuxConfig.ps1
## Purpose: This script sets up a Linux driver for PS-Remoting over SSH using 
## a public/private key pair for authentication.
##
#############################################################################

#Install Open-SSH client if it's not already installed.
sudo apt install openssh-client

#Install Open-SSH Server if it's not already installed.
sudo apt install openssh-server

#Restart the sshd service.
sudo systemctl restart sshd.service

#Replace the sshd config file with a version containing the necessary changes
#i.e. enable key authentication, creating an SSH subsystem for PowerShell.
sudo cp -a ./sshd_config_unix /etc/ssh/sshd_config

#Restart the sshd service.
sudo systemctl restart sshd.service

# Below is a work-around for a default installation on Ubuntu 22.04 where some shared object files might be missing with the default install.
# If the libmi.so file does not exist, install Powershell using snap and copy the necessary shared object files over.
$file = "/usr/lib/libmi.so"
if ( -not (Test-Path -Path $file -PathType Leaf)) {
    sudo snap install powershell --classic

    Set-Location "/snap/powershell/current/opt/powershell"

    sudo cp libpsrpclient.so libmi.so libssl.so.1.0.0 libcrypto.so.1.0.0 /usr/lib
}

#Generate a public-private key pair using the ed25519 scheme.
#The default scheme is RSA but some new Linux machines might not allow
#RSA for a key pair.
#Generate to the default location (-f ~/.ssh/id_ed25519), use an empty passphrase (-q -N '""')
#If the files already exists, this will generate a prompt asking if to replace them.
ssh-keygen -t ed25519 -f ~/.ssh/id_ed25519 -q -N '""'

#Set appropriate permissions on the private key i.e. grant read-write access to the current user.
#If the appropriate permissions are not set, key authentication will not work.
chmod 600 ~/.ssh/id_ed25519

#Copy the public key to the SUT.
#This copies the key to both the default location for SSH on Linux machines as well as the 
#location used by SSH on Windows installations.
$server = Read-Host -Prompt 'Input IP address for the SUT'
scp ~/.ssh/id_ed25519.pub "$administrator@${server}:/C:\ProgramData\ssh\administrators_authorized_keys"
scp ~/.ssh/id_ed25519.pub "$administrator@${server}:/C:\Users\Administrator\.ssh\authorized_keys"