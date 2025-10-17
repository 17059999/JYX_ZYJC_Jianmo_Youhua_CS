# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           LocalLinuxFunctionLib.psm1
# Purpose:        Library about comminucating with Linux related api.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

function Get-LinuxIP{
[CmdletBinding()]
    param(
		[Parameter(ValueFromPipeline=$True, Mandatory = $True)]
		$VmName,
		[Parameter(ValueFromPipeline=$True, Mandatory = $True)]
		$NetworkName
	)
	process{
		$currentLinuxVMIP = $null

		do{
            Start-Sleep -Seconds 2

            $currentLinuxVMIP = get-vm -VMName $vmName | Select-Object -ExpandProperty Networkadapters | Where-Object {$_.SwitchName -like "*$networkName*"}  | Select-Object -Property ipaddresses
            $currentLinuxVMIP = $currentLinuxVMIP.IPAddresses[0]
            if(![string]::IsNullOrEmpty($currentLinuxVMIP)){
                Write-Host "$vmName $networkName IP address $currentLinuxVMIP"
                if($currentLinuxVMIP.contains(":")){
                    # it get the MAC address, not ip address, so reset the value to get the IP address again.
                    $currentLinuxVMIP = $null
                }
            }else{
                Write-Host "Waiting to get $vmName $networkName IP address"
            }
        }while($null -eq $currentLinuxVMIP)
	
		return $currentLinuxVMIP
	}
}

function Get-LinuxVMPublicIP{
[CmdletBinding()]
    param(
		[Parameter(ValueFromPipeline=$True, Mandatory = $True)]
		$Vm
	)
	process {
		$vmName = $Vm.hypervname
		$externalName = $Vm.vnetExternal

		$currentLinuxVMIP = Get-LinuxIP -VmName $vmName -NetworkName $externalName

		return $currentLinuxVMIP
	}
}

function Get-LinuxVMPrivateIP{
[CmdletBinding()]
    param(
		[Parameter(ValueFromPipeline=$True, Mandatory = $True)]
		$Vm
	)
	process {	
		$vmName = $Vm.hypervname
		$internalName = $Vm.vnet

		$currentLinuxVMIP = Get-LinuxIP -VmName $vmName -NetworkName $internalName

		return $currentLinuxVMIP
	}
}

function Disable-PublicIPNetwork{
[CmdletBinding()]
    param(
		[Parameter(ValueFromPipeline=$True, Mandatory = $True)]
		$Vm
	)
	process {	
		$currentLinuxVMIP = Get-LinuxVMPublicIP -VM $Vm

		[array]$ipList = $Vm.IP
		$ipCount = $ipList.Count
		$publicNetworkIndex = "eth$ipCount" # The last network index is public network
		$disablePublicIPNetworkCommand = "ifconfig $publicNetworkIndex down"
		Execute-PlinkShCommand -VmIP $currentLinuxVMIP -ShCommand $disablePublicIPNetworkCommand -ShCommandKey "disable_public_ip"
	}
}

function Create-TrustConnection{
[CmdletBinding()]
    param(
		[Parameter(Mandatory=$true)][string]$VmIP,
		[string]$VmAccount = "root",
		[string]$VmPassword = "Password01!"
	)
	process{
	    $workingDirOnHost = Get-WorkingDirOnHost
	    $puttyScp = $workingDirOnHost + "\Tools\PuTTY\pscp.exe"
	    $trustCommand = "cmd /c ""echo y | ""$puttyScp"" -l $VmAccount -pw $VmPassword -ls $VmIP"+":/"""

	    Write-Host $trustCommand

		$trustConnectBatFile = "C:\auto_generate_trust_connect.bat"
		$utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
		[System.IO.File]::WriteAllLines($trustConnectBatFile, $trustCommand, $utf8NoBomEncoding)

		Start-Sleep -Seconds 10
		# Execute bat file
        &$trustConnectBatFile

		Start-Sleep -Seconds 10
        # Remove bat file after execute over
        Remove-Item $trustConnectBatFile
	}
}

function Execute-PlinkShCommand{
[CmdletBinding()]
    param(
		[Parameter(Mandatory=$true)][string]$VmIP,
		[string]$VmAccount = "root",
		[string]$VmPpassword = "Password01!",
		[Parameter(Mandatory=$true)][string]$ShCommand,
		[string]$ShCommandKey = "",
		[string]$ActionDescription = ""
	)
	process {
	    $workingDirOnHost = Get-WorkingDirOnHost
	    $puttyLink = $workingDirOnHost + "\Tools\PuTTY\plink.exe"
		$scriptParameter = " -batch -ssh " + $VmAccount +"@"+$VmIP + " -pw " + $VmPpassword + " """ + $ShCommand + """"
		$info = $ActionDescription + $puttyLink + $scriptParameter
		Write-Host $info

		$shFileBatFile = "C:\auto_generate_{$ShCommandKey}_file.bat"
		$shFileBatCommand = """$puttyLink""" + $scriptParameter
		$utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
		[System.IO.File]::WriteAllLines($shFileBatFile, $shFileBatCommand, $utf8NoBomEncoding)

		# Execute bat file and then remove the temp bat file
		&$shFileBatFile
		Remove-Item $shFileBatFile
	}
}

function Execute-PscpCopyLinuxFolderToWindowsCommand{
[CmdletBinding()]
    param(
		[Parameter(Mandatory=$true)][string]$VmIP,
		[string]$VmAccount = "root",
		[string]$VmPassword = "Password01!",
		[Parameter(Mandatory=$true)][string]$SourceFilePath,
		[Parameter(Mandatory=$true)][string]$DestinationFilePath,
		[string]$ShCommandKey = "",
		[string]$ActionDescription = ""
	)
	process{
	    $workingDirOnHost = Get-WorkingDirOnHost
	    $puttyScp = $workingDirOnHost + "\Tools\PuTTY\pscp.exe"
		$copyFileParameter = " -r -q -pw " + $VmPassword + " " + $VmAccount + "@"+$VmIP+":$SourceFilePath $DestinationFilePath"
		$info = $actionDescription + $puttyScp + $copyFileParameter
		Write-Host $info

		$shFileBatFile = "C:\auto_generate_{$ShCommandKey}_file.bat"
		$shFileBatCommand = """$puttyScp""" + $copyFileParameter
		$utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
		[System.IO.File]::WriteAllLines($shFileBatFile, $shFileBatCommand, $utf8NoBomEncoding)

		# Execute bat file and then remove the temp bat file
		&$shFileBatFile
		Remove-Item $shFileBatFile
	}
}

function Execute-PscpCopyLinuxFileToWindowsCommand{
[CmdletBinding()]
    param(
		[Parameter(Mandatory=$true)][string]$VmIP,
		[string]$VmAccount = "root",
		[string]$VmPassword = "Password01!",
		[Parameter(Mandatory=$true)][string]$SourceFilePath,
		[Parameter(Mandatory=$true)][string]$DestinationFilePath,
		[string]$ShCommandKey = "",
		[string]$ActionDescription = ""
	)
	process{
	    $workingDirOnHost = Get-WorkingDirOnHost
	    $puttyScp = $workingDirOnHost + "\Tools\PuTTY\pscp.exe"
		$copyFileParameter = " -q -pw " + $VmPassword + " " + $VmAccount + "@"+$VmIP+":$SourceFilePath $DestinationFilePath"
		$info = $actionDescription + $puttyScp + $copyFileParameter
		Write-Host $info

		$shFileBatFile = "C:\auto_generate_{$ShCommandKey}_file.bat"
		$shFileBatCommand = """$puttyScp""" + $copyFileParameter
		$utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
		[System.IO.File]::WriteAllLines($shFileBatFile, $shFileBatCommand, $utf8NoBomEncoding)

		# Execute bat file and then remove the temp bat file
		&$shFileBatFile
		Remove-Item $shFileBatFile
	}
}

function Execute-PscpCopyWindowsFileToLinuxCommand{
[CmdletBinding()]
    param(
		[Parameter(Mandatory=$true)][string]$VmIP,
		[string]$VmAccount = "root",
		[string]$VmPassword = "Password01!",
		[Parameter(Mandatory=$true)][string]$SourceFilePath,
		[Parameter(Mandatory=$true)][string]$DestinationFilePath,
		[string]$ActionDescription = ""
	)
	process{
	    $workingDirOnHost = Get-WorkingDirOnHost
	    $puttyScp = $workingDirOnHost + "\Tools\PuTTY\pscp.exe"
		$copyFileParameter = "-q -pw " + $VmPassword + " " + $SourceFilePath + " " + $VmAccount + "@"+$VmIP + ":$DestinationFilePath"
		$info = $ActionDescription + $puttyScp + $copyFileParameter
		Write-Host $info

		$rediectStandardConfirmInput = "C:\auto_generate_confirm_text.txt"
        Write-Output "Y" > $rediectStandardConfirmInput

		$ps = new-object System.Diagnostics.Process
		$ps.StartInfo.Filename = $puttyScp
		$ps.StartInfo.Arguments = $copyFileParameter
		$ps.StartInfo.RedirectStandardOutput = $false
		$ps.StartInfo.UseShellExecute = $false
		$ps.StartInfo.RedirectStandardError = $true
		$ps.StartInfo.RedirectStandardInput = $rediectStandardConfirmInput

		try{
			$ps.start()
			$ps.WaitForExit() | Out-Null
			[string] $Out = $ps.StandardError.ReadToEnd()
			Write-Host $Out
		}catch{
		}

		Start-Sleep -Seconds 10
		Remove-item $rediectStandardConfirmInput
	}
}

function Execute-PscpCopyWindowsFolderToLinuxCommand{
[CmdletBinding()]
    param(
		[Parameter(Mandatory=$true)][string]$VmIP,
		[string]$VmAccount = "root",
		[string]$VmPassword = "Password01!",
		[Parameter(Mandatory=$true)][string]$SourceFilePath,
		[Parameter(Mandatory=$true)][string]$DestinationFilePath,
		[string]$ShCommandKey = "",
		[string]$ActionDescription = ""
	)
	process{
	    $workingDirOnHost = Get-WorkingDirOnHost
	    $puttyScp = $workingDirOnHost + "\Tools\PuTTY\pscp.exe"
		$copyFileParameter = " -q -pw " + $VmPassword + " -r " + $SourceFilePath + " " + $VmAccount + "@"+$VmIP + ":$DestinationFilePath"
		$info = $ActionDescription + $puttyScp + $copyFileParameter
		Write-Host $info

		$shFileBatFile = "C:\auto_generate_{$ShCommandKey}_file.bat"
		$shFileBatCommand = """$puttyScp""" + $copyFileParameter
		$utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
		[System.IO.File]::WriteAllLines($shFileBatFile, $shFileBatCommand, $utf8NoBomEncoding)

		# Execute bat file and then remove the temp bat file
		&$shFileBatFile
		Remove-Item $shFileBatFile
	}
}

function Get-WorkingDirOnHost{
    $driverName = $PSScriptRoot.Split(':')[0]
	return $driverName + ":\WinteropProtocolTesting"
}

# export all the functions in this module
Export-ModuleMember -Function *
