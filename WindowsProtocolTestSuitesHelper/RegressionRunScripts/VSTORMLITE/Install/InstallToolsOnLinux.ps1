#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           InstallToolsOnLinux.ps1
## Purpose:        Install Tools On Linux.
## Version:        1.0 (8 Feb, 2021)
##
##############################################################################

$signalfile = "InstallToolsOnLinux.Completed.signal"
if(Test-Path $signalfile)
{
    Remove-Item $signalfile -Force
}
$logFile = "./" + $MyInvocation.MyCommand.Name + ".log"
Start-Transcript -Path $logFile -Append -Force

$computerName = Get-Content "Temp/Name.txt"

$ConfigureFile = "Temp/Protocol.xml"
[xml]$xmlContent = Get-Content $ConfigureFile
if($xmlContent -eq $null)
{
    Write-Host "Protocol configure file $ConfigureFile is invalid." -ForegroundColor Red
	Stop-Transcript
    exit 1
}

$VMs = $xmlContent.SelectNodes("lab/servers/vm")
foreach($VMNode in $VMs)
{
	if($VMNode.name -eq $computerName)
	{
		$VM = $VMNode
	}
}

if($VM -eq $null)
{
    Write-Host "Cannot find Vm configure for Vm $computerName." -ForegroundColor Red
	Stop-Transcript
    exit 1
}

if($null -eq $VM.Tools)
{
	Write-Host "Cannot find Vm Tools configure for Vm $computerName." -ForegroundColor Red
	Stop-Transcript
    exit 0
}

$TestsuiteZips = $VM.Tools.GetElementsByTagName("TestsuiteZip")
foreach($TestsuiteZip in $TestsuiteZips)
{
	$ZipName = $TestsuiteZip.ZipName
	$targetFolder = $TestsuiteZip.targetFolder

    write-host "Expand test suite: $ZipName to $targetFolder"
	Expand-Archive ~/Temp/Deploy/$ZipName -DestinationPath $targetFolder
}

#Installing with APT can be done with a few commands. Before you install .NET,
#run the following commands to add the Microsoft package signing key to your list of trusted keys and add the package repository.
$osSKU = $VM.imageReference.sku
$osName = bash -c "cat /etc/*-release | grep 'NAME='"
write-host "OS name: $osName"

if($osName -match "Ubuntu") {
    if($osSKU -match "(?<ver1>\d{2})[.|_](?<ver2>\d{2})")
    {
        $expectedVersion = $Matches["ver1"]+ "." + $Matches["ver2"]
        if($expectedVersion -eq "20.04")
        {
            # Enable dns server
            $domainName = $VM.domain
            $dnsName = $VM.dns
            $vmname = $VM.name
            $vmip = $VM.ip
            if(($VM.ip | Measure-Object).Count -gt 1) {
                $vmip = $VM.ip[0]
            }
            bash -c "sudo chmod 777 /etc/hosts"
            bash -c "sudo echo '$vmip $vmname'>>/etc/hosts"
            bash -c "sudo resolvectl dns eth0 8.8.8.8 1.1.1.1 $dnsName 168.63.129.16"
            bash -c "sudo rm -rf /etc/resolv.conf"
            bash -c "sudo touch /etc/resolv.conf"
            bash -c "sudo chmod 777 /etc/resolv.conf"
            bash -c "sudo echo 'domain $domainName'>/etc/resolv.conf"
            bash -c "sudo echo 'nameserver $dnsName'>>/etc/resolv.conf"
            bash -c "sudo echo 'nameserver 8.8.8.8'>>/etc/resolv.conf"
            bash -c "sudo echo 'nameserver 1.1.1.1'>>/etc/resolv.conf"
            bash -c "sudo echo 'options edns0 trust-ad'>>/etc/resolv.conf"
            bash -c "sudo echo 'search $domainName reddog.microsoft.com'>>/etc/resolv.conf"
            bash -c "sudo systemctl restart systemd-resolved"
        }
        $packagesSource = "https://packages.microsoft.com/config/ubuntu/$expectedVersion/packages-microsoft-prod.deb"
    }

    if($null -eq $packagesSource)
    {
        Write-Host "cannot find packages Source for $osSKU" -ForegroundColor Red
	    Stop-Transcript
        exit 1
    }

    bash -c "wget $packagesSource -O packages-microsoft-prod.deb"
    bash -c "sudo dpkg -i packages-microsoft-prod.deb"
    bash -c "sudo apt-get update -y"
    bash -c "sudo apt-get install -y apt-transport-https"
    bash -c "sudo sleep 10s"
    bash -c "sudo apt-get update -y"
    bash -c "sudo sleep 10s"
    $installDotnetResult = bash -c "sudo apt-get install -y dotnet-sdk-8.0"
    if($installDotnetResult -match "Unable to locate package") {
        #Use snap when repo has issue: https://github.com/dotnet/core/issues/6277
        Write-Host "Use snap to install since microsoft apt-get repo has issue"
        bash -c "sudo sleep 10s"
        bash -c "sudo snap install dotnet-sdk --classic --channel=5.0"
        bash -c "sudo snap alias dotnet-sdk.dotnet dotnet"
    }
}
elseif($osName -match "Fedora")
{
    $packagesSource = "https://packages.microsoft.com/config/fedora/34/packages-microsoft-prod.rpm"
    bash -c "sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc"
    bash -c "wget $packagesSource -O packages-microsoft-prod.rpm"
    bash -c "sudo rpm -Uvh packages-microsoft-prod.rpm"
    bash -c "sudo dnf check-update"
    bash -c "sudo dnf install -y dotnet-sdk-8.0"
}
elseif($osName -match "openSUSE Leap")
{
    bash -c "sudo rpm --import https://packages.microsoft.com/keys/microsoft.asc"
    bash -c "sudo zypper addrepo https://packages.microsoft.com/config/opensuse/15/prod.repo"
    bash -c "sudo zypper update -y"
    # Sleep 60s to avoid rpm lock
    bash -c "sudo sleep 60s"
    bash -c "sudo zypper --non-interactive install dotnet-sdk-8.0"
    
}
elseif($osName -match "CentOS Stream")
{
    bash -c "sudo dnf install -y dotnet-sdk-8.0"
}

# Enable username and password login
if($osName -match "Fedora") {
    bash -c "sudo sed -i 's/#PasswordAuthentication yes/PasswordAuthentication yes/g' /etc/ssh/sshd_config" #Fedora
}
else
{
    bash -c "sudo sed -i 's/PasswordAuthentication no/PasswordAuthentication yes/g' /etc/ssh/sshd_config" #Ubuntu/SUSE
}
bash -c "sudo service sshd restart"
$vmusername = $xmlContent.lab.core.username
$vmpassword = $xmlContent.lab.core.password
bash -c "echo '${vmusername}:${vmpassword}' | sudo chpasswd"


$Tools = $VM.Tools.GetElementsByTagName("tool")

foreach($Tool in $Tools)
{
	$ZipName = $Tool.ZipName
	if($ZipName -ne $null)
	{
		$targetFolder = $Tool.targetFolder

		if(!(Test-Path -Path $targetFolder))
		{
			Write-Host "create folder: $targetFolder"
			New-Item -ItemType directory -Path $targetFolder
		}

		Write-Host "expand zip tool: ~/Temp/Deploy/$ZipName to $targetFolder"
		Expand-Archive ~/Temp/Deploy/$ZipName -DestinationPath $targetFolder
	}

	$installCommand = $Tool.installCommand
    write-host "Install command: $installCommand"

	if($installCommand -ne $null) {
		Write-Host "install tool($($Tool.name))..."
		foreach ($currCommand in [Array]$installCommand.Split(";")) {
			Write-Host "run command: $currCommand"
			bash -c $currCommand
		}
	}
}



ECHO "Completed" > $signalfile
Stop-Transcript
exit 0