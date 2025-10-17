###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

Param(
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string] $TestSuiteName,
    # The name of the XML file, indicating which environment you want to configure
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$configFile,
    # Azure Subscriptoion Id
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$subscriptionId,
    # Azure Storage Account Name
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$storageAccount,
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$vaultName,
    # Whether to create a restore point
    [string]$enableRestorePoint = "false"
)

$scriptPath             = Split-Path $MyInvocation.MyCommand.Definition -parent;
$RegressionRootPath     = "$scriptPath\..\"
Push-Location $scriptPath

#------------------------------------------------------------------------------------------
# Script to Check signal file on remote VM
#------------------------------------------------------------------------------------------
[ScriptBlock]$CheckSignalFile = {
    Param([string]$fileName)

    # Try if the name specified is a directory
    IF(Test-Path -Path "$env:HOMEDRIVE\$fileName"){
        return $true
    }else{
        return $false
    }
}

Function Write-TestSuiteInfo {
    Param(
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [string]$ForegroundColor = "White",
        [string]$BackgroundColor = "DarkBlue")

    # WinBlue issue: Start-Transcript cannot write the log printed out by Write-Host, as a workaround, use Write-output instead
    # Write-Output does not support color
    if ([Double]$Script:HostOsBuildNumber -eq [Double]"6.3") {
        ((Get-Date).ToString() + ": $Message") | Out-Host
    }
    else {
        Write-Host ((Get-Date).ToString() + ": $Message") -ForegroundColor $ForegroundColor -BackgroundColor $BackgroundColor
    }
}

Function Write-TestSuiteWarning {
    Param (
        [Parameter(ValueFromPipeline = $True)]
        [string]$Message,
        [switch]$Exit)

    Write-TestSuiteInfo -Message "[WARNING]: $Message" -ForegroundColor Yellow -BackgroundColor Black
    if ($Exit) {exit 1}
}

#------------------------------------------------------------------------------------------
# Read and parse XML configuration file
# $Setup will be used as a global variable to storage the configuration information
#------------------------------------------------------------------------------------------
Function Read-TestSuiteXml {

    Write-TestSuiteInfo "Read and parse the XML configuration file."

    Write-TestSuiteStep "Check if the XML configuration file exist or not."
    # If $XmlFileFullPath is not found, prompt a list of choices for user to choose
    if (!(Test-Path -Path $configFile)) {
        Write-TestSuiteError "$configFile file not found."
    }
    else {
        Write-TestSuiteInfo "$configFile file found."
    }

    # Read contents from the XML file
    Write-TestSuiteStep "Read contents from the XML configuration file."
    [Xml]$Script:Setup = Get-Content $configFile
    if ($null -eq $Script:Setup) {
        Write-TestSuiteError "$configFile file is not a valid xml configuration file." -Exit
    }
}

#------------------------------------------------------------------------------------------
# Get the virtual machine list by the operation system type
#------------------------------------------------------------------------------------------
Function GetVMListByOSType{
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [string]$osType
    )
    
    if($osType -eq "Linux" -or $osType -eq "RPMBasedLinux"){
        # Return Linux VM
        return $Script:Setup.lab.servers.vm | Where-Object {$_.os -like $osType}
    }elseif($osType -eq "All"){
        # Return all OS VM
        return $Script:Setup.lab.servers.vm
    }else{
        # Return Other OS VM(include Windows or any OS that does not configure the 'os' node value)
        return $Script:Setup.lab.servers.vm | Where-Object {$_.os -like $osType -or [string]::IsNullOrWhiteSpace($_.os)}
    }      
}

#------------------------------------------------------------------------------------------
# Get the domain user account name
#------------------------------------------------------------------------------------------
Function GetDomainUserName{
    return $Script:Setup.lab.core.username
}

Function RunExtensionScriptOnRemoteVM {
    
    param (
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [string]$ExtenstionName,
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [string]$ResourceGroup,
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [string]$Location,
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [string]$SettingsString,
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
        [System.Object[]]$vmList
    )

    if($null -eq $vmList){
        return
    }

    $jobs = @()

    foreach ($Vm in ($vmList | Sort-Object -Property installorder)) {
        $vmName = $Vm.name
        Write-TestSuiteInfo "Start execute extension job $ExtenstionName on $vmName"
        
        $jobName = $vmName + "_" + $ExtenstionName
        $job = Set-AzVMExtension -Name $jobName `
            -ResourceGroupName $ResourceGroup `
            -VMName $VMName `
            -Publisher "Microsoft.Compute" `
            -ExtensionType "CustomScriptExtension" `
            -TypeHandlerVersion "1.0" `
            -SettingString $SettingsString `
            -Location $Location `
            -AsJob

        $jobs += $job;
    }
    
    Write-TestSuiteInfo "waiting extension job $ExtenstionName complete"
    foreach($job in $jobs){
        Wait-Job -id $job.Id -Timeout 3600 | Receive-Job -ErrorAction SilentlyContinue
    }

    Write-TestSuiteInfo "Remove extension job $ExtenstionName"
    foreach ($Vm in ($vmList | Sort-Object -Property installorder)) {
        $vmName = $Vm.name
        $jobName = $vmName + "_" + $ExtenstionName
        Remove-AzVMExtension -ResourceGroupName $ResourceGroup -VMName $vmName -Name $jobName -Force
    }
}

#------------------------------------------------------------------------------------------
# Use Azure Customer Extension Script to Enable PS Remote/Disable Firewall...
#------------------------------------------------------------------------------------------
Function Initialize-TestSuiteVM {
    Initialize-TestSuiteWindowsVM
}

Function Initialize-TestSuiteWindowsVM{
    $location = $Script:Setup.lab.vmsetting.location
    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;

    $windowsVmList = GetVMListByOSType -osType "Windows"
    if($null -eq $windowsVmList){
        return
    }

    # Enable the admin user.
    $ExtenstionName = "EnableTSAdmin"
    $settings = "{`"commandToExecute`":`"powershell cmd /C net user $($Script:Setup.lab.core.username) /active:yes`"}"
    RunExtensionScriptOnRemoteVM -ExtenstionName $ExtenstionName `
        -ResourceGroup $resourceGroup `
        -Location $location `
        -SettingsString $settings `
        -vmList $windowsVmList

}

#------------------------------------------------------------------------------------------
# Enable Encryption...
#------------------------------------------------------------------------------------------
Function EnableEncryption-TestSuiteVM {
    Write-TestSuiteInfo "Gets information about the key vaults $vaultName"
    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;
    
    $KeyVault = Get-AzKeyVault -VaultName $vaultName
    $DiskEncryptionKeyVaultUrl = $KeyVault.VaultUri
    $KeyVaultResourceId = $KeyVault.ResourceId

    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder)) {
        $vmName = $Vm.name
        Write-TestSuiteInfo "Enables encryption on $vmName"
        if($Vm.os -ne "Linux" -and $Vm.os -ne "RPMBasedLinux"){
            Set-AzVMDiskEncryptionExtension -ResourceGroupName $resourceGroup -VMName $vmName -DiskEncryptionKeyVaultUrl $DiskEncryptionKeyVaultUrl -DiskEncryptionKeyVaultId $KeyVaultResourceId -Force
        }else{ # for Linux OS, execute the below Encryption method with special parameters
            #When encrypting Linux OS volumes, the VM should be considered unavailable. We strongly recommend to avoid SSH logins while the encryption is in progress to avoid issues blocking any open files that will need to be accessed during the encryption process.
            #Set-AzureRmVMDiskEncryptionExtension -ResourceGroupName $resourceGroup -VMName $vmName -DiskEncryptionKeyVaultUrl $DiskEncryptionKeyVaultUrl -DiskEncryptionKeyVaultId $KeyVaultResourceId -SkipVmBackup -VolumeType All -Force
        }
    }
}

#------------------------------------------------------------------------------------------
#Create Restore Point for VM
#------------------------------------------------------------------------------------------
Function CreateRestorePoint{
    #enableRestorePoint
    if ($enableRestorePoint -eq "true")
    {
        #Get ResourceGroup
        $ResourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;
        #Get VM list
        foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder))
        {
            try
            {
                $vmName = $Vm.name
                #Get VM information
                $vmInfo = Get-AzVM -ResourceGroupName $ResourceGroup -Name $vmName
                #Define Restore Point Collection name
                $restorePointCollectionName = "$vmName-RPC"                       
                Write-Host "Check whether the Restore Point Collection exists"   
                try
                {
                    # Check whether the Restore Point Collection exists
                    $oldRestorePointCollection = Get-AzRestorePointCollection -ResourceGroupName $ResourceGroup -Name $restorePointCollectionName          
                    Write-Host $oldRestorePointCollection.ProvisioningState                           
                    if ($oldRestorePointCollection.ProvisioningState -eq "Succeeded")
                    {
                        Write-Host "Start remove restore point collection"
                        # Remove Restore Point Collection                               
                        Remove-AzRestorePointCollection -ResourceGroupName $ResourceGroup -Name $restorePointCollectionName                                    
                        Write-Host "Sleep for 30 seconds to ensure the delete operation completes"      
                        Start-Sleep -Seconds 30    
                        Write-Host "Old restore Point Collection $restorePointCollectionName has been deleted."
                    }
                }
                catch
                {
                    Write-Host "Restore Point Collection don't exist."
                }    

                Write-Host "Start create Restore Point Collection: $restorePointCollectionName"
                # Create restore point collection for the VM
                $restorePointCollection = New-AzRestorePointCollection -ResourceGroupName $ResourceGroup -RestorePointCollectionName $restorePointCollectionName -Location $vmInfo.Location -SourceId $vmInfo.Id
                #Define restore point name
                $restorePointName = "$vmName-RP"
                #Create the restore point
                Write-Host "Start create Restore Point: $restorePointName"
                New-AzRestorePoint -ResourceGroupName $ResourceGroup -RestorePointCollectionName $restorePointCollectionName -Name $restorePointName
                Write-Host "Restore Point $restorePointName created successfully for VM $vmName"
            }
            catch
            {
                Write-Warning "Failed to create Restore Point for VM $vmName."
            }
        }
    }
    else
    {
        Write-Host "No restore point needs to be created for the VM"
    }
}

# Create a windows task and run it.
# Remote command invoke does not have full permissions to access local resources.
# Create a task to run the task with local previleges.
[ScriptBlock]$CreateTask = {
    Param([string]$FilePath,[string]$TaskName)

    # Push script parent folder to location stack before run the script
    $ParentDir = [System.IO.Directory]::GetParent($FilePath)
    $Command = "{Push-Location $ParentDir; Invoke-Expression $FilePath}"
    # Guarantee commands run in powershell environment
    $Task = "Powershell Powershell -Command $Command"
    # Create task
    cmd /c schtasks /Create /RU Administrators /SC ONCE /ST 00:00 /TN $TaskName /TR $Task /IT /F
    Sleep 5
}

# Run a windows task.
# Remote command invoke does not have full permissions to access local resources.
# run the task with local previleges.
[ScriptBlock]$RunTask = {
    Param([string]$TaskName)

    $retryCount = 1
    While($retryCount -lt 10)
    {
        # Run task
        cmd /c schtasks /Run /TN $TaskName
        Sleep 5
        $result = Get-ScheduledTask -TaskName $TaskName | Get-ScheduledTaskInfo
        if($result.LastTaskResult -ne 267009)
        {
             Write-Host "Task not running, retry..."
            $retryCount++
        }else {
            Write-Host "Connected to remote session"
            break
        }
    }
}

#------------------------------------------------------------------------------------------
# Create Geneva Monitor start script and run
#------------------------------------------------------------------------------------------
function ConfigureGenevaMonitor {
    Write-TestSuiteInfo "Starting Geneva Monitor configuration"

    $allVmList = GetVMListByOSType -osType "All"
    if ($null -eq $allVmList) {
        return
    }

    [ScriptBlock]$CreateAndRunBatchAsTask = {
        [CmdletBinding()]
        param (
            [string]$Cert,
            [string]$CertPassword,
            [string]$Account,
            [string]$Namespace,
            [string]$Region,
            [string]$Thumbprint
        )

        $azSecPackPath = "C:\AzSecPack"
        $monitoringPath = "$azSecPackPath\Monitoring"
        $dataPath = "$monitoringPath\Data"

        mkdir -Path $azSecPackPath -Force
        mkdir -Path $monitoringPath -Force
        mkdir -Path $dataPath -Force

        $certPath = "$azSecPackPath\AzSecPack.pfx"
        $certBytes = [System.Convert]::FromBase64String($Cert)
        [System.IO.File]::WriteAllBytes($certPath, $certBytes)
        Import-PfxCertificate -FilePath $certPath -Password (ConvertTo-SecureString -String $CertPassword -AsPlainText -Force) -CertStoreLocation Cert:\LocalMachine\My

        $batch = @()
        $batch += "set MONITORING_DATA_DIRECTORY=$dataPath"
        $batch += "set MONITORING_TENANT=%USERNAME%"
        $batch += "set MONITORING_ROLE=$Namespace"
        $batch += "set MONITORING_ROLE_INSTANCE=%COMPUTERNAME%"
        $batch += "set MONITORING_GCS_ENVIRONMENT=Diagnostics PROD"
        $batch += "set MONITORING_GCS_ACCOUNT=$Account"
        $batch += "set MONITORING_GCS_NAMESPACE=$Namespace"
        $batch += "set MONITORING_GCS_REGION=$Region"
        $batch += "set MONITORING_GCS_THUMBPRINT=$Thumbprint"
        $batch += "set MONITORING_GCS_CERTSTORE=LOCAL_MACHINE\My"
        $batch += "set MONITORING_CONFIG_VERSION=1.9"
        if ([System.Environment]::OSVersion.Version.Build -ge 17763) {
            $batch += "set AZSECPACK_PILOT_FEATURES=MdeServer2019Support"
        }
        else {
            $batch += "set AZSECPACK_PILOT_FEATURES=WDATP"
        }
        $batch += "%MonAgentClientLocation%/MonAgentClient.exe -useenv"

        [System.IO.File]::WriteAllLines("$monitoringPath\runAgent.cmd", $batch)

        $taskAction = New-ScheduledTaskAction -Execute "$monitoringPath\runAgent.cmd"
        $taskTrigger = New-ScheduledTaskTrigger -AtStartup
        $taskPrincipal = New-ScheduledTaskPrincipal "SYSTEM"
        $taskSettings = New-ScheduledTaskSettingsSet
        $task = New-ScheduledTask -Action $taskAction -Trigger $taskTrigger -Principal $taskPrincipal -Settings $taskSettings
        Register-ScheduledTask "GenevaMonitoringStartup" -InputObject $task

        Start-Sleep -Seconds 5

        Start-ScheduledTask -TaskName "GenevaMonitoringStartup"
    }

    #$gcsCert = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsCert").SecretValueText
    #$gcsCertPassword = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsCertPassword").SecretValueText
    #$gcsAccount = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsAccount").SecretValueText
    #$gcsNamespace = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsNamespace").SecretValueText
    #$gcsThumbprint = (Get-AzureKeyVaultSecret -VaultName $vaultName -Name "GcsThumbprint").SecretValueText

    foreach ($vm in ($allVmList | Sort-Object -Property installorder)) {
        # Create Remote Session
        $ip = $vm.ip
        if (($vm.ip | Measure-Object ).Count -gt 1) {
            $ip = $vm.ip[0]
        }
        else {
            $ip = $vm.ip
        }
            
        [string]$fullUserName = "";

        $userName = $Script:Setup.lab.core.username;
        $userPass = $Script:Setup.lab.core.password;

        if ($vm.username) {
            $userName = $vm.username
        }

        if ($vm.password) {
            $userPass = $vm.password
        }

        if ($vm.os -ne "Linux" -and $vm.os -ne "RPMBasedLinux") {
            if ($vm.domain) {
                $fullUserName = $vm.domain + "\" + $userName
            }
            else {
                $fullUserName = $ip + "\" + $userName
            }
                
            $vmSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $fullUserName -UserPassword $userPass -RemoteIP $ip
            #Invoke-Command -Session $vmSession -ScriptBlock $CreateAndRunBatchAsTask -ArgumentList @($gcsCert, $gcsCertPassword, $gcsAccount, $gcsNamespace, $Script:Setup.lab.vmsetting.location, $gcsThumbprint)

            Remove-PSSession $vmSession
        }
        else {
            $gcsRegion = $Script:Setup.lab.vmsetting.genevaAccountRegion
            & $PSScriptRoot\Linux\InstallGenevaMonitorToLinux.ps1 -vmIpAddress $ip -username $userName -vaultName $vaultName -gcsRegion $gcsRegion
        }
    }
}


#------------------------------------------------------------------------------------------
# Run Powershell Script on Windows VM
#------------------------------------------------------------------------------------------
Function RunPowershellOnVM {
  Param
  (
      [Parameter(Mandatory=$true)]
      [ValidateNotNullOrEmpty()]
      [string]$ScriptName, 
      [Parameter(Mandatory=$true)]
      [string]$VMIp
  )

  $userName = $Script:Setup.lab.core.username;
  $userPass = $Script:Setup.lab.core.password;
  [string]$fullUserName = $VMIp + "\"+$userName

  Write-Host "Create PSSession to connect to Azure VM: $VMIp"
  $vmSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $fullUserName -UserPassword $userPass -RemoteIP $VMIp

  Write-Host "Enabling TLS 1.2 on $VMIp"
  Invoke-Command -Session $vmSession -FilePath "$ScriptPath\$ScriptName" 

  Write-Host "TLS1.2 setup complete"
  Remove-PSSession $vmSession
}

#------------------------------------------------------------------------------------------
# Enable TLS 1.2
#------------------------------------------------------------------------------------------
function InstallTLS12 {
  $windowsVmList = GetVMListByOSType -osType "Windows"
  if($null -eq $windowsVmList){
      return
  }

  Write-Host "Script Invocation Path: $scriptPath"
  $scriptName = "TLSSettings.ps1"

  foreach ($vm in $windowsVmList) {
    
    Write-Host "VM IP: $($Vm.Ip)"
    if (-Not [string]::IsNullOrWhiteSpace($Vm.Ip)) {
      $ip = $Vm.ip
      if(($Vm.ip | Measure-Object ).Count -gt 1){
          $ip = $Vm.ip[0]
      }
      RunPowershellOnVM -ScriptName $scriptName -VMIp $ip
    }
  }
}

Function RetryWindowsScheduledJob{
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$ScriptType, 
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$vmName, 
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$fullUserName, 
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$userPass, 
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$ip, 
        [Parameter(Mandatory=$true)]
        [Int]$secondsPerTime, 
        [Parameter(Mandatory=$true)]
        [Int]$retryCount
    )
    # The $ScriptType job processing, make sure $ScriptType job is in running status or finished status, otherwise retry to run it.
    while($true) {
        # Sleep some seconds so we can check if it ran failed or not.
        Write-TestSuiteInfo "Sleep $secondsPerTime seconds."
        Start-Sleep -Seconds $secondsPerTime
        $vmSessionForGetJobStatus = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $fullUserName -UserPassword $userPass -RemoteIP $ip
        $scheduledJobStatus = Invoke-Command -Session $vmSessionForGetJobStatus -ScriptBlock {Param([string]$ScriptType)(Get-ScheduledTask -TaskName "$ScriptType" | Get-ScheduledTaskInfo).LastTaskResult} -ArgumentList "$ScriptType"
        Remove-PSSession $vmSessionForGetJobStatus
        if($scheduledJobStatus -eq 267009)
        {
            Write-TestSuiteInfo "$ScriptType is running on $vmName."
            break
        }
        elseif($scheduledJobStatus -eq 0)
        {
            Write-TestSuiteInfo "$ScriptType ran pass on $vmName."
            break
        }
        elseif($retryCount -ne 0) {
            if($scheduledJobStatus -eq 1073807364)
            {
                Write-TestSuiteInfo "$ScriptType ran failed with 0x40010004 on $vmName, but it is possible that a RestartComputer.PS1 script in the job, need to check next signal file."
                $myVmSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $fullUserName -UserPassword $userPass -RemoteIP $ip
                if($ScriptType -eq "InstallScript"){
                    $signalFile = "Install.Completed.signal"
                }elseif($ScriptType -eq "InstallFeatureScript"){
                    $signalFile = "InstallFeatureScript.Completed.signal"
                }elseif($ScriptType -eq "PostScript"){
                    $signalFile = "PostScript.Completed.signal"
                }
                $scriptCompleted = Invoke-Command -Session  $myVmSession -ScriptBlock $CheckSignalFile -ArgumentList $signalFile
                Remove-PSSession $myVmSession
                if($scriptCompleted ) {break;}
            }
            Write-TestSuiteInfo "$ScriptType failed to run on $vmName, need retry..."
            $myVmSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $fullUserName -UserPassword $userPass -RemoteIP $ip
            $job = Invoke-Command -Session $myVmSession -ScriptBlock $RunTask -ArgumentList "$ScriptType" -AsJob
            try {
                Write-TestSuiteInfo "Waiting $ScriptType job $($job.Id) on $vmName."
                $retryCount=3000
                While((Get-Job -id $job.Id).State -eq "Running"){Write-Host "." -NoNewLine:$true;sleep 10;$retryCount--;if($retryCount -lt 0){Write-Host "Timeout for $ScriptType job $($job.Id) on $vmName.";break}}
            }
            catch {
                Write-TestSuiteInfo "Wait Job failed, may caused by Deadlock$($_)"
            }
            Remove-PSSession $myVmSession
            Write-TestSuiteInfo "Sleep $secondsPerTime seconds then check next scheduled job <Last Run Result> value..."
            $retryCount--
            Start-Sleep -Seconds $secondsPerTime
            continue
        }
        else
        {
            Write-TestSuiteInfo "$ScriptType failed to run on $vmName and it has done retry $retryCount times."
            break
        }
    }
}

#------------------------------------------------------------------------------------------
# Execute powershell script on remote VM and wait complete signal file
#------------------------------------------------------------------------------------------
Function ExecuteScriptAndCheckSignalFile{
    Param
    (
        [Parameter(Mandatory=$true)]
        [ValidateNotNullOrEmpty()]
        [string]$ScriptType, 
        [Parameter(Mandatory=$true)]
        [Int]$Timeout
    )

    Write-TestSuiteInfo "Starting Execute $ScriptType, Timeout: $Timeout minutes"
    $sessions = @();
    $needCheckVMs = New-Object 'System.Collections.Generic.List[Object]'
    $jobs = New-Object 'System.Collections.Generic.List[Object]';

    $allVmList = GetVMListByOSType -osType "All"
    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;

    if($null -eq $allVmList){
        return
    }

    foreach ($Vm in ($allVmList | Sort-Object -Property installorder)) {
        [string]$scriptName = ""
        [string]$signalFile = ""

        if($ScriptType -eq "InstallScript"){
            $scriptName = $Vm.installscript
            $signalFile = "Install.Completed.signal"
        }elseif($ScriptType -eq "InstallFeatureScript"){
            $scriptName = $Vm.installfeaturescript
            $signalFile = "InstallFeatureScript.Completed.signal"
        }elseif($ScriptType -eq "PostScript"){
            $scriptName = $Vm.postscript
            $signalFile = "PostScript.Completed.signal"
        }

        if (![string]::IsNullOrEmpty($scriptName)) {

            # Create Remote Session
            $userName = $Script:Setup.lab.core.username;
            $userPass = $Script:Setup.lab.core.password;
            $vmName = $Vm.name
            $ip = $Vm.ip
            if(($Vm.ip | Measure-Object ).Count -gt 1){
                $ip = $Vm.ip[0]
            }else{
                $ip = $Vm.ip
            }
            if($Vm.username){
                $userName = $Vm.username
            }
            if($Vm.password){
                $userPass = $Vm.password
            }

            if($Vm.os -eq "Linux" -or $Vm.os -eq "RPMBasedLinux"){
                # Execute the custom script               
                [string]$fullUserName = $userName + "@"+ $ip
                if($ScriptType -eq "InstallScript"){
                    $runScript = "Install.ps1"                    
                }elseif($ScriptType -eq "InstallFeatureScript"){
                    $runScript = "InstallFeatureScript.ps1"
                }elseif($ScriptType -eq "PostScript"){
                    $runScript = "Post.ps1"
                }
                # Always start service for dns
                ssh $fullUserName sudo systemctl restart systemd-resolved.service
                Write-TestSuiteInfo "ssh $fullUserName pwsh /home/$userName/Temp/$runScript"
                ssh $fullUserName pwsh /home/$userName/Temp/$runScript
                Start-Sleep -Seconds 120
                for($i=0;$i -lt 10;$i++)
                {
                    try
                    {
                        Test-Connection -ComputerName $ip -ErrorAction Stop
                        break
                    }
                    catch
                    {
                        Write-Host "Wait 60 seconds then retry."
                        Start-Sleep -Seconds 60
                    }
                }
                
                Write-TestSuiteInfo "$ScriptType Command completed"
            }else{
                [string]$fullUserName = "";                
                if($Vm.domain){
                    $fullUserName = $Vm.domain + "\"+$userName
                }else{
                    $fullUserName = $ip + "\"+$userName
                }
                
                $vmSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $fullUserName -UserPassword $userPass -RemoteIP $ip
                $sessions+=$vmSession
                # Call Install.ps1
                if($ScriptType -eq "InstallScript"){
                    Write-TestSuiteInfo "Start Execute InstallScript.ps1 on remote server: $vmName"
                    Invoke-Command -Session $vmSession -ScriptBlock $CreateTask -ArgumentList "C:\Temp\Install.ps1","InstallScript"
                    $job =  Invoke-Command -Session $vmSession -ScriptBlock $RunTask -ArgumentList "InstallScript" -AsJob
                    $jobs.Add($job)
                }elseif($ScriptType -eq "InstallFeatureScript"){
                    Write-TestSuiteInfo "Start Execute InstallFeatureScript.ps1 on remote server: $vmName"
                    Invoke-Command -Session $vmSession -ScriptBlock $CreateTask -ArgumentList "C:\Temp\InstallFeatureScript.ps1","InstallFeatureScript"
                    $job =  Invoke-Command -Session $vmSession -ScriptBlock $RunTask -ArgumentList "InstallFeatureScript" -AsJob
                    $jobs.Add($job)
                }elseif($ScriptType -eq "PostScript"){
                    Write-TestSuiteInfo "Start Execute Post.ps1 on remote server: $vmName"
                    Invoke-Command -Session $vmSession -ScriptBlock $CreateTask -ArgumentList "C:\Temp\Post.ps1","PostScript"
                    $job =  Invoke-Command -Session $vmSession -ScriptBlock $RunTask -ArgumentList "PostScript" -AsJob
                    if(![string]::IsNullOrEmpty($Vm.skipwaitingforpostscript) -and ($Vm.skipwaitingforpostscript -eq "false")) {
                        Write-TestSuiteInfo "Waiting 5 minutes while $ScriptType are running."
                        Start-Sleep -Seconds 300
                        Write-TestSuiteInfo "Waiting job $($job.Id)."
                        Wait-Job -id $job.Id | Receive-Job -ErrorAction SilentlyContinue    

                        Write-TestSuiteInfo "Wait for $vmName to complete the post script installation."
                        # By default, wait 20 minutes for post script to complete
                        $PostTimeout = 20
                        $isLookingCompleted = $false;
                        $lookingFullUserName = $ip + "\" + $userName
                        Do
                        {
                            Write-TestSuiteInfo "Sleep 3 minutes then check signal files..."
                            Start-Sleep -Seconds 180
                            $PostTimeout--
                            $lookingVMSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $lookingFullUserName -UserPassword $userPass -RemoteIP $ip
                            if($null -eq $lookingVMSession)
                            {
                                continue;
                            }
                            $scriptCompleted = Invoke-Command -Session  $lookingVMSession -ScriptBlock $CheckSignalFile -ArgumentList $signalFile
                            if($scriptCompleted -eq $true)
                            {
                                $isLookingCompleted = $true;
                            }

                            # failed to Auto-logon system, need retry...
                            if($PostTimeout -eq 10) {
                                Write-TestSuiteInfo "Reboot Machine $vmName..."                                
                                Restart-AzVM -ResourceGroupName $resourceGroup -Name $vmName                                
                                Start-Sleep -Seconds 180
                            }

                            Remove-PSSession $lookingVMSession
                        }
                        Until(($PostTimeout -eq 0) -or ($isLookingCompleted -eq $true))
                        
                        if (($PostTimeout -eq 0) -and ($isLookingCompleted -eq $false)) {
                            Write-TestSuiteWarning "Post scripts unable to complete in $vmName." -Exit
                            #throw $("Waiting $signalFile Timeout.")
                        }
                    } else {
                        $jobs.Add($job)
                    }
                }elseif($ScriptType -eq "InstallVisualStudio"){
                    Write-TestSuiteInfo "Start Execute InstallVisualStudio.ps1 on remote server: $vmName"
                    $job = Invoke-Command -Session $vmSession -ScriptBlock {Push-Location "C:\Temp"; C:\Temp\Install_VisualStudio_Online.ps1; } -AsJob
                    $jobs.Add($job)
                }

                $vmItem = '' | Select-Object Name, Ip, UserName, FullUserName, Password, OS
                $vmItem.Name = $vm.name
                $vmItem.Ip = $ip
                $vmItem.FullUserName = $fullUserName
                $vmItem.UserName = $userName
                $vmItem.Password = $userPass
                $vmItem.OS = $Vm.os
                $needCheckVMs.Add($vmItem)
            }
        }
    }

    if($jobs.Count -gt 0){
        if($ScriptType -eq "InstallScript"){ 
            Start-Sleep -Seconds 300
        }
        Write-TestSuiteInfo "Waiting 5 minutes while $ScriptType are running."
        Start-Sleep -Seconds 300

        foreach($job in $jobs){
            # TODO: Need add code check if job state, if blocked need wait
            try {
                Write-TestSuiteInfo "Waiting job $($job.Id)."
                Wait-Job -id $job.Id | Receive-Job    
            }
            catch {
                Write-TestSuiteInfo "Wait Job failed, may caused by Deadlock$($_)"
                Write-TestSuiteInfo "Waiting 5 minutes ..."
            }
        }
    
        # Wait Signal file
        Write-TestSuiteInfo "Total $ScriptType Count: $($needCheckVMs.Count)"
    
        $timeoutTimes = $Timeout/3
        while(($needCheckVMs.Count -gt 0) -And ($timeoutTimes -gt 0))
        {
            Write-TestSuiteInfo "Sleep 3 minutes then check signal files..."
            Start-Sleep -Seconds 180
            $timeoutTimes--
    
            $removeItems = @();
            foreach ($item in $needCheckVMs) {
                $vmName = $item.Name
                $vmSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $item.FullUserName -UserPassword $item.Password -RemoteIP $item.Ip
                $scriptCompleted = Invoke-Command -Session  $vmSession -ScriptBlock $CheckSignalFile -ArgumentList $signalFile
                
                if($scriptCompleted -eq $true)
                {
                    $removeItems += $item;
                }
                elseif($item.OS -ne "Linux" -and $item.OS -ne "RPMBasedLinux"){
                    # Retry for Windows failed schedule job.
                    RetryWindowsScheduledJob -ScriptType $ScriptType -vmName $item.Name -fullUserName $item.FullUserName -userPass $item.Password -ip $item.Ip -secondsPerTime 300 -retryCount 10
                }


                Remove-PSSession $vmSession
            }
    
            foreach($item in $removeItems){
                $needCheckVMs.Remove($item);
            }
        }
    }

    if($sessions.Count -gt 0){
        #Remove PS Sessions
        foreach($session in $sessions){
            Remove-PSSession $session
        }
        Write-TestSuiteInfo "Session removed"
    }
    
    if(($needCheckVMs.Count -gt 0) -And $timeoutTimes -eq 0){
        throw $("Waiting $signalFile Timeout.")
    }

    Write-TestSuiteInfo "Script $ScriptType Execute completed" -BackgroundColor Green
}

#------------------------------------------------------------------------------------------
# Copy Temp folder to Azure VM then start run configure files and wait complete signal file
#------------------------------------------------------------------------------------------
Function Configure-TestSuiteVM{


    #Get Domain and it's ip mapping
    $userName = $Script:Setup.lab.core.username;
    $userPass = $Script:Setup.lab.core.password;
    $resourceGroup = $Script:Setup.lab.vmsetting.resourceGroup;

    foreach ($Vm in ($Script:Setup.lab.servers.vm | Sort-Object -Property installorder)) {
        # Create Remote Session
        $vmName = $Vm.name
        $ip = $Vm.ip
        if(($Vm.ip | Measure-Object ).Count -gt 1){
            $ip = $Vm.ip[0]
        }else{
            $ip = $Vm.ip
        }
        
        Write-TestSuiteInfo "Reboot Machine $vmName, and starting wait 3 minutes until VM stable..."     
        Restart-AzVM -ResourceGroupName $resourceGroup -Name $vmName        
        Start-Sleep -Seconds 180

        Write-TestSuiteInfo "Copy Temp folder to destination VM: $vmName"
        $sourceFolder = "$RegressionRootPath\AzureRegression\$TestSuiteName\$($vmName)_Temp"

        if($Vm.os -eq "Linux"){
            [string]$fullUserName = $userName + "@"+ $ip

            Write-TestSuiteInfo "cmd /c scp -r $($sourceFolder) $($fullUserName):/home/$userName"
            cmd /c scp -r "$($sourceFolder)" "$($fullUserName):/home/$userName"
            Write-TestSuiteInfo "ssh $fullUserName sudo mv /home/$userName/$($vmName)_Temp /home/$userName/Temp"
            ssh $fullUserName sudo mv /home/$userName/$($vmName)_Temp /home/$userName/Temp
            Write-TestSuiteInfo "ssh $fullUserName sudo chmod -R 777 /home/$userName/Temp"
            ssh $fullUserName sudo chmod -R 777 /home/$userName/Temp
            Write-TestSuiteInfo "Copy completed"

            ssh $fullUserName sudo snap install powershell --classic
            Write-TestSuiteInfo "Installed Powershell"
        } elseif($Vm.os -eq "RPMBasedLinux") {
            [string]$fullUserName = $userName + "@"+ $ip

            Write-TestSuiteInfo "cmd /c scp -r $($sourceFolder) $($fullUserName):/home/$userName"
            cmd /c scp -r "$($sourceFolder)" "$($fullUserName):/home/$userName"
            Write-TestSuiteInfo "ssh $fullUserName sudo mv /home/$userName/$($vmName)_Temp /home/$userName/Temp"
            ssh $fullUserName sudo mv /home/$userName/$($vmName)_Temp /home/$userName/Temp
            Write-TestSuiteInfo "ssh $fullUserName sudo chmod -R 777 /home/$userName/Temp"
            ssh $fullUserName sudo chmod -R 777 /home/$userName/Temp
            Write-TestSuiteInfo "Copy completed"

            ssh $fullUserName sudo rpm -i --nodeps https://github.com/PowerShell/PowerShell/releases/download/v7.4.6/powershell-lts-7.4.6-1.rh.x86_64.rpm
            Write-TestSuiteInfo "Installed Powershell"
        } else {
            [string]$fullUserName = $ip + "\"+$userName

            Write-TestSuiteInfo "Create PSSession to connect to Azure VM: $vmName"
            $Stoploop = $false
            [int]$Retrycount = 0
            do {
                $vmSession = & $RegressionRootPath\ScriptLib\Get-RemoteSession.ps1 -FullUserName $fullUserName -UserPassword $userPass -RemoteIP $ip
            
                Write-TestSuiteInfo "Remove Temp folder if exists on remote vm"
                Invoke-Command -Session $vmSession -ScriptBlock {Push-Location "C:\"; if(Test-Path -Path "C:\Temp"){Remove-Item C:\Temp\ -Force -Recurse;} }
                Write-TestSuiteInfo "Copy Temp folder to destination VM: $vmName"
                $sourceFolder = "$RegressionRootPath\AzureRegression\$TestSuiteName\$($vmName)_Temp"
            try {
                Copy-Item $sourceFolder -Destination "C:\Temp" -ToSession $vmSession -Force -Recurse
                Write-TestSuiteInfo "Copy completed"
                $Stoploop = $true
            }
            catch {
                if ($Retrycount -gt 3){
                    Write-TestSuiteInfo "Fail to copy Temp folder"
                    $Stoploop = $true
                }
                else {
                    Write-TestSuiteInfo "Fail to copy Temp folder retrying in 30 seconds..."
                    Start-Sleep -Seconds 30
                    $Retrycount = $Retrycount + 1
                }
            }

            # Enable Windows Update
            Invoke-Command -Session $vmSession -ScriptBlock {Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU" -Name "NoAutoUpdate" -Value 0 -Type DWord; Set-ItemProperty -Path "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU" -Name "AUOptions" -Value 4 -Type DWord}
        }
        While ($Stoploop -eq $false)
            
            # Don't copy test suite source code for main branch, it will cost many hours for cross region VM.
            #if(Test-Path -Path "$RegressionRootPath\Test") {
            #    Write-TestSuiteInfo "Copy Source folder to destination VM: $vmName"
            #    Copy-Item "$RegressionRootPath\Test" -Destination "C:\" -ToSession $vmSession -Force -Recurse
            #    Write-TestSuiteInfo "Copy completed"
            #}
            
            #Check if need add prefix domain ip
            if(!([string]::IsNullOrEmpty($Vm.dns))){
                $Vm.dns = $Vm.dns -replace ';',','
                Write-TestSuiteInfo "Set DNS Address $($Vm.dns) for VM: $($vmName)"
                Invoke-Command -Session $vmSession -ScriptBlock { param([string]$domainIp) $index = (Get-NetAdapter | Where-Object {$_.Status -eq "Up"} | Select-Object ifIndex).ifIndex; $index | ForEach-Object -Process { Set-DnsClientServerAddress -InterfaceIndex $index -ServerAddresses $domainIp } } -ArgumentList $Vm.dns
            }
            
            $domain = $Vm.domain;
            if([string]::IsNullOrEmpty($domain) -OR ($domain -like 'workgroup')){
                $domain = '.'
            }

            Remove-PSSession $vmSession
        }
        
    }

    Start-Sleep -Seconds 60
    
    ConfigureGenevaMonitor
    

    # Enable TLS 1.2 | May cause a system reboot
    InstallTLS12

    # Step 1 Install installscript, timeout is times of 30 minutes
    Write-TestSuiteStep "Starting execute InstallScript"
    ExecuteScriptAndCheckSignalFile -ScriptType "InstallScript" -Timeout 30
    
    # Step 2 Install installfeaturescript, timeout is times of 45 minutes
    Write-TestSuiteStep "Starting execute InstallFeatureScript"
    ExecuteScriptAndCheckSignalFile -ScriptType "InstallFeatureScript" -Timeout 45
    
    # Step 3 Install postscript, timeout is times of 45 minutes
    Write-TestSuiteStep "Starting execute PostScript"
    ExecuteScriptAndCheckSignalFile -ScriptType "PostScript" -Timeout 45 
}


Function Main{
    Read-TestSuiteXml

    Initialize-TestSuiteVM

    EnableEncryption-TestSuiteVM

    Configure-TestSuiteVM

    CreateRestorePoint
}

Main

Pop-Location