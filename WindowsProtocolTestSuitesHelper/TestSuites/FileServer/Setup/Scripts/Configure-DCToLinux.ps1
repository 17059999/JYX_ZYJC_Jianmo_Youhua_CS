    ###########################################################################################
    ## Copyright (c) Microsoft Corporation. All rights reserved.
    ###########################################################################################

    $logFile = "./" + $MyInvocation.MyCommand.Name + ".log"
    Start-Transcript -Path $logFile -Append -Force

    $computerName = Get-Content "Temp/Name.txt"
    $ConfigureFile = "Temp/Protocol.xml"
    [Xml]$Script:Setup = Get-Content $ConfigureFile
    if($Script:Setup -eq $null)
    {
        Write-Host "Protocol configure file $ConfigureFile is invalid." -ForegroundColor Red
        Stop-Transcript
        exit 1
    }
    $scriptName = "Configure-DCToLinux.sh"
    $domainAccount = $Script:Setup.lab.core.username
    $machineName = [Environment]::MachineName
    $VmSettings = $Script:Setup.lab.servers.vm | where {$_.name -eq $machineName}
    $domainName = $VmSettings.domain
    $dns = $VmSettings.dns

    #------------------------------------------------------------------------------------------
    # Generate the sh file
    #------------------------------------------------------------------------------------------
    Function GenerateConfigurationScriptFileForLinux{
        $shSCriptParamBuilder = [System.Text.StringBuilder]::new()
        [void]$shSCriptParamBuilder.AppendLine("#!/bin/sh");

        [void]$shSCriptParamBuilder.AppendLine("if [ -f /etc/os-release ]; then");
        [void]$shSCriptParamBuilder.AppendLine(". /etc/os-release");
        [void]$shSCriptParamBuilder.AppendLine("OS=$(grep '^ID=' /etc/os-release | cut -d'=' -f2)");
        [void]$shSCriptParamBuilder.AppendLine("fi");

        [void]$shSCriptParamBuilder.AppendLine('case "$OS" in');
        [void]$shSCriptParamBuilder.AppendLine("    ubuntu)");
        [void]$shSCriptParamBuilder.AppendLine("        sudo ufw disable");
        [void]$shSCriptParamBuilder.AppendLine("        sudo apt-get update");
        [void]$shSCriptParamBuilder.AppendLine('        sudo DEBIAN_FRONTEND=noninteractive apt-get install -y -o Dpkg::Options::="--force-confnew" krb5-user sssd sssd-tools libnss-sss libpam-sss ntp ntpdate realmd adcli');
        [void]$shSCriptParamBuilder.AppendLine("        ;;");
        [void]$shSCriptParamBuilder.AppendLine("    centos)");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl stop firewalld");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl disable firewalld");
        [void]$shSCriptParamBuilder.AppendLine("        sudo yum install -y krb5-workstation oddjob oddjob-mkhomedir sssd sssd-tools realmd adcli chrony");
        [void]$shSCriptParamBuilder.AppendLine("        sudo dnf install -y oddjob oddjob-mkhomedir sssd realmd adcli");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl enable --now oddjobd.service");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl enable --now sssd");
        [void]$shSCriptParamBuilder.AppendLine("        ;;");
        [void]$shSCriptParamBuilder.AppendLine("    fedora)");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl stop nftables");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl disable nftables");
        [void]$shSCriptParamBuilder.AppendLine("        sudo yum install -y krb5-workstation oddjob oddjob-mkhomedir sssd sssd-tools realmd adcli chrony");
        [void]$shSCriptParamBuilder.AppendLine("        sudo dnf install -y oddjob oddjob-mkhomedir sssd realmd adcli");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl enable --now oddjobd.service");
        [void]$shSCriptParamBuilder.AppendLine("        sudo systemctl enable --now sssd");        [void]$shSCriptParamBuilder.AppendLine("        ;;");
        [void]$shSCriptParamBuilder.AppendLine("    opensuse-leap)");
        [void]$shSCriptParamBuilder.AppendLine("        sudo zypper refresh");
        [void]$shSCriptParamBuilder.AppendLine("        sudo zypper install -y krb5-client sssd sssd-tools ntp realmd adcli");
        [void]$shSCriptParamBuilder.AppendLine("        ;;");
        [void]$shSCriptParamBuilder.AppendLine("esac");

        
        # Configure the Seach DNS
        $resolveConfigFile = "/etc/resolv.conf"
        [void]$shSCriptParamBuilder.AppendLine("echo `"nameserver $dns`" > $resolveConfigFile");
        [void]$shSCriptParamBuilder.AppendLine("echo `"search $domainName`" >> $resolveConfigFile");

        # Configure the hosts file
        $hostsConfigFile = "/etc/hosts"
        [void]$shSCriptParamBuilder.AppendLine("echo `"127.0.0.1 $machineName.$domainName $machineName`" >> $hostsConfigFile");
        

        $ignoreValidUserName = "--force-badname"
        $defaultPassword = "Password01!"

        # Configure the krb5.conf
        #[void]$shSCriptParamBuilder.AppendLine("sudo awk '/^\[libdefaults\]/ { print; print "        default_realm = SNIA-EMEA-2025.org"; print "        dns_lookup_realm = true"; print "        dns_lookup_kdc = true"; print "        rdns=false"; next }1' /etc/krb5.conf > /tmp/krb5.conf && sudo mv /tmp/krb5.conf /etc/krb5.conf")
        #[void]$shSCriptParamBuilder.AppendLine("sudo awk '/^\[realms\]/ { print; print "        $domainName = {"; print "                kdc = $dns"; print "                admin_server = $dns"; print "        }";    next }1'  /etc/krb5.conf > /tmp/krb5.conf && sudo mv /tmp/krb5.conf /etc/krb5.conf")
        #[void]$shSCriptParamBuilder.AppendLine("sudo awk '/^\[domain_realm\]/ { print; print "        .$domainName = $domainName"; print "        $domainName = $domainName"; next }1'  /etc/krb5.conf > /tmp/krb5.conf && sudo mv /tmp/krb5.conf /etc/krb5.conf")
        
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[libdefaults\]/a        rdns = false' /etc/krb5.conf")
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[libdefaults\]/a        dns_lookup_kdc = true' /etc/krb5.conf")
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[libdefaults\]/a        dns_lookup_realm = true' /etc/krb5.conf")
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[libdefaults\]/a        default_realm = $domainName' /etc/krb5.conf")
        
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[realms\]/a         }' /etc/krb5.conf")
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[realms\]/a             admin_server = $dns' /etc/krb5.conf")
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[realms\]/a             kdc = $dns' /etc/krb5.conf")
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[realms\]/a         $domainName = {' /etc/krb5.conf")

        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[domain_realm\]/a       $domainName = $domainName' /etc/krb5.conf")
        [void]$shSCriptParamBuilder.AppendLine("sudo sed -i '/^\[domain_realm\]/a       .$domainName = $domainName' /etc/krb5.conf")

        # Configure the krb5.keytab
        [void]$shSCriptParamBuilder.AppendLine("sudo ktutil <<EOF")
        [void]$shSCriptParamBuilder.AppendLine("addent -password -p $domainAccount@$domainName -k 42 -e aes256-cts-hmac-sha1-96")
        [void]$shSCriptParamBuilder.AppendLine("$defaultPassword")
        [void]$shSCriptParamBuilder.AppendLine("wkt /etc/krb5.keytab")
        [void]$shSCriptParamBuilder.AppendLine("quit")
        [void]$shSCriptParamBuilder.AppendLine("EOF")

        # Configure network time protocol
        $linuxNetworkConfigFile = "/etc/ntp.conf"
        [void]$shSCriptParamBuilder.AppendLine("echo `"server $domainName`" >> $linuxNetworkConfigFile");
        
        # Restart the network configuration
        [void]$shSCriptParamBuilder.AppendLine("sudo systemctl stop ntp");
        [void]$shSCriptParamBuilder.AppendLine("sudo ntpdate $domainName");
        [void]$shSCriptParamBuilder.AppendLine("sudo systemctl start ntp");

        $linuxHostConfigFile = "/etc/hosts"
        [void]$shSCriptParamBuilder.AppendLine("echo `"$dns DC1.$domainName primarydc`" >> $linuxHostConfigFile");
        [void]$shSCriptParamBuilder.AppendLine("echo `"$dns DC1.$domainName primarykrb`" >> $linuxHostConfigFile");

        # Discover the Azure AD DS managed domain
        [void]$shSCriptParamBuilder.AppendLine("sudo realm discover $domainName");

        # Enter the user account that's a part of the Azure AD DS managed domain
        [void]$shSCriptParamBuilder.AppendLine("echo $defaultPassword | kinit $domainAccount@$domainName");

        # Join the machine to the Azure AD DS managed domain
        [void]$shSCriptParamBuilder.AppendLine("echo $defaultPassword | sudo realm join --verbose $domainName -U '$domainAccount' --install=/");

        $streamWriter = [System.IO.StreamWriter] $scriptName
        $streamWriter.Write($shSCriptParamBuilder.ToString());
        $streamWriter.Close();
    }

    Function Invoke-Sudo { 
        Write-Host "invoke-sudo $args"
        & /usr/bin/env sudo pwsh -command "& $args" 
    }

    Function Main{
        GenerateConfigurationScriptFileForLinux
        Invoke-Sudo "chmod 777 $scriptName"
        Invoke-Sudo "./$scriptName"
    }

    Main

    Pop-Location