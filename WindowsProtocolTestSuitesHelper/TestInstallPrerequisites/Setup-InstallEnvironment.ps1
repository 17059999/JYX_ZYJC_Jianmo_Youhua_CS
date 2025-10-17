##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################

Param
(
    # The SourcePath is the source code path
    [string]$SourcePath,
    # Branch of WindowsProtocolTestSuitesHelper
    [string]$HelperBranch,
    # Branch of WindowsProtocolTestSuites
    [string]$TestSuiteBranch,
    # The user who started the job
    [string]$BuildUser,
    # The virtual hard disk for creating a server VM
    [string]$VHDName = "18362.1.amd64fre.19h1_release.190318-1202_client_enterprise_en-us_vl.vhd",
    # The share path that stores all the VHDs
    [string]$VHDPath = "\\pet-storage-04\PrototestRegressionShare\VHDShare",
    # The path that upload result
    [string]$ShareResultPath = "\\pet-storage-04\PrototestRegressionShare\ProtocolTestSuite",
    # Name of the answer files
    [string]$AnswerFile = "18362_Enterprise.xml",
    # This is the path to get the release MSI
    [string]$ReleaseMSIPath = "\\ziz-dfsr02\WINTEROP\OpenSource\ReleaseMSIs\3.19.9.0",
    # This is the account to get the release MSI
    [string]$ReleaseMSIUserName = "FAREAST\pettest",
    # This is the password to get the release MSI
    [string]$ReleaseMSIPassword,
    [string]$SenderUsername,
    [string]$SenderPassword
)

$InvocationPath = Split-Path $MyInvocation.MyCommand.Definition -parent
$DriverRoot = (Get-Location).Drive.Name + ":"
$WinteropProtocolTesting = $DriverRoot + "\WinteropProtocolTesting"
$HostOsBuildNumber = "" + [Environment]::OSVersion.Version.Major + "." + [Environment]::OSVersion.Version.Minor
$Script:VmDirPath = "$WinteropProtocolTesting\VM\InstallPrerequisites"
$LocalVHDPath = "$WinteropProtocolTesting\VHD"
$ISOForderPath = "$VmDirPath\ISO"
$ResultPath = "$VmDirPath\TestResults"
$VmIsoFullPath = "$VmDirPath\InstallTest.iso"
$OscdimgPath = "$InvocationPath\..\RegressionRunScripts\VSTORMLITE\Install"
$AnswerFilePath = "$InvocationPath\..\RegressionRunScripts\VSTORMLITE\AnswerFile\$AnswerFile"
$ScriptsPath = "$InvocationPath\..\RegressionRunScripts\ScriptLib\script"

Write-Host "=========================================================="
Write-Host "SourcePath               $SourcePath"
Write-Host "HelperBranch             $HelperBranch"
Write-Host "TestSuiteBranch          $TestSuiteBranch"
Write-Host "InvocationPath:          $InvocationPath"
Write-Host "WinteropProtocolTesting: $WinteropProtocolTesting"
Write-Host "VmDirPath:               $Script:VmDirPath"
Write-Host "LocalVHDPath:            $LocalVHDPath"
Write-Host "ISOForderPath:           $ISOForderPath"
Write-Host "ResultPath:              $ResultPath"
Write-Host "VmIsoFullPath:           $VmIsoFullPath"
Write-Host "OscdimgPath:             $OscdimgPath"
Write-Host "AnswerFilePath:          $AnswerFilePath"
Write-Host "ScriptsPath:             $ScriptsPath"
Write-Host "ReleaseMSIPath:          $ReleaseMSIPath"
Write-Host "=========================================================="

#------------------------------------------------------------------------------------------
# Sleeping for a particular amount of time to wait for an activity to be completed
#------------------------------------------------------------------------------------------
Function Wait-TestSuiteActivityComplete {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    [string]$ActivityName,
    [int]$TimeoutInSeconds = 0)

    for ([int]$Tick = 0; $Tick -le $TimeoutInSeconds; $Tick++) {
        Write-Progress -Activity "Wait for $ActivityName ..." -SecondsRemaining ($TimeoutInSeconds - $Tick) -PercentComplete (($Tick / $TimeoutInSeconds) * 100)
        if ($Tick -lt $TimeoutInSeconds) { Start-Sleep 1 }
    }
    Write-Progress -Activity "Wait for $ActivityName ..." -Completed
}
#------------------------------------------------------------------------------------------
# Format the input xml file and display it to the screen
#------------------------------------------------------------------------------------------
Function Format-TestSuiteXml {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    [xml]$Xml,
    [int]$Indent = 2)

    Process {
        $StringWriter = New-Object System.IO.StringWriter
        $XmlWriter = New-Object System.Xml.XmlTextWriter $StringWriter
        $XmlWriter.Formatting = "indented"
        $XmlWriter.Indentation = $Indent
        [xml]$Xml.WriteContentTo($XmlWriter)
        $XmlWriter.Flush()
        $StringWriter.Flush()

        # Output the result
        Write-Output $("`n" + $StringWriter.ToString())
    }
}

function Prepare-ServerSetup {
    [string]$Script:StartTime = [System.dateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss")
    if(Test-Path $ResultPath) {
        Remove-Item $ResultPath -Force -Recurse
    }
    mkdir $ResultPath
    Start-Transcript -Path "$ResultPath/Setup-InstallEnvironment.log" -Append -Force
}

function Read-Configurationfile {
    Write-Host "Read and parse the XML configuration file."
    if(!(Test-Path -Path "$InvocationPath\InstallPrerequisites.xml")){
        Write-Error "Cannot find InstallPrerequisites.xml" Exit
    }
    [Xml]$Script:Setup = Get-Content "$InvocationPath\InstallPrerequisites.xml"
    $Script:Setup | Format-TestSuiteXml -Indent 4
    $Script:VM = $Script:Setup.lab.servers.vm
}

#------------------------------------------------------------------------------------------
# Check the prerequisites of the host machine before setup test suite environment
#------------------------------------------------------------------------------------------
Function Check-HostPrerequisites {

    Write-Host "Check prerequisites of the host for test suite environment setup"

    Write-Host "Check if the host operating system version is supported or not."
    if ([Double]$Script:HostOsBuildNumber -le [Double]"6.1") {
        Write-Host "Unsupported operating system version $Script:HostOsBuildNumber. Must be larger than 6.1." -BackgroundColor "Red" -Exit
    }
    else {
        Write-Host "Supported operating system version $Script:HostOsBuildNumber."
    }

    Write-Host "Check if the host has enabled router by registry key."
    # http://technet.microsoft.com/en-us/library/cc962461.aspx
    If ((Get-ItemProperty -path HKLM:\system\CurrentControlSet\services\Tcpip\Parameters -name IpEnableRouter -ErrorAction Silentlycontinue).ipenablerouter -ne 1) {
        Write-Host "Router is disabled. Registry key IpEnableRouter under path HKLM:\system\CurrentControlSet\services\Tcpip\Parameters is not set to 1. Set it now..."
        Set-ItemProperty -Path HKLM:\system\CurrentControlSet\services\Tcpip\Parameters -Name IpEnableRouter -Value 1
    }
    else {
        Write-Host "Router is enabled."
    }

    Write-Host "Check if `"RSAT-Hyper-V-Tools`" feature is installed or not."
    Write-Host "Import ServerManager module if not imported."
    Import-Module ServerManager
    $FeatureName = "RSAT-Hyper-V-Tools"
    $Feature = Get-WindowsFeature | Where { $_.Name -eq "$FeatureName" }
    if($Feature.Installed -eq $false) {
        Write-Host "Feature not installed. Install it now..."
        Add-WindowsFeature -Name $FeatureName -IncludeAllSubFeature -IncludeManagementTools
        Wait-TestSuiteActivityComplete -ActivityName "Install $FeatureName" -TimeoutInSeconds 5
    }
    else {
        Write-Host "Feature already installed." -BackgroundColor "Blue"
    }
    
    Write-Host "Check if `"Hyper-V v3.0 PowerShell Module`" is imported:"
    if (!(Get-Module -ListAvailable Hyper-V)) {
        Write-Host "Module not imported. Import it now..." -BackgroundColor "Yellow"
        Import-Module Hyper-V
    } 
    else {
        Write-Host "Module already imported."
    }
}

#------------------------------------------------------------------------------------------
# Download VHD from share folder to workspace
#------------------------------------------------------------------------------------------

function Download-VHD {
    Write-Host "Copy VHD from $VHDPath\$VHDName to $WinteropProtocolTesting\VM\InstallPrerequisites..."
    if(!(Test-Path "$WinteropProtocolTesting\VM\InstallPrerequisites")) {
        mkdir "$WinteropProtocolTesting\VM\InstallPrerequisites"
    }
    if(Test-Path "$WinteropProtocolTesting\VM\InstallPrerequisites\InstallPrerequisites.vhd") {
        Remove-Item "$WinteropProtocolTesting\VM\InstallPrerequisites\InstallPrerequisites.vhd" -Force
    }

    if(!(Test-Path "$LocalVHDPath\$VHDName")){
        Write-Host "Cannot find $VHDName on $LocalVHDPath, download at $VHDPath"
        Copy-Item "$VHDPath\$VHDName" "$LocalVHDPath\$VHDName" -Force
    }
    Write-Host "Copy $LocalVHDPath\$VHDName to $WinteropProtocolTesting\VM\InstallPrerequisites\"
    Copy-Item "$LocalVHDPath\$VHDName" "$WinteropProtocolTesting\VM\InstallPrerequisites\InstallPrerequisites.vhd" 
    Write-Host "Copy VHD finished"
}

#------------------------------------------------------------------------------------------
# Check the prerequisites of the host machine before setup test suite environment
#------------------------------------------------------------------------------------------
Function Mount-TestSuiteISO {
    Param(
    [string]$Vm,
    [string]$VmIsoFullPath,
    [int]$TimeoutInSeconds = 3600,
    [int]$SleepTimePerIteration = 5
    )

    Process {

        Write-Host "Start mounting ISO for $($Vm.hypervname)."

        Write-Host "Get the DVD drive information of this virtual machine."
        $VmDvdDrive = Get-VMDvdDrive -VMName $Vm.hypervname | Select-Object -First 1 
        $VmDvdDrive | Format-List VMName,ControllerType,ControllerNumber,ControllerLocation,DvdMediaType,Path,IsDeleted

        Write-Host "Check whether ISO file exists or not."
        if (!(Test-Path $VmIsoFullPath)) {
            Write-Host "$VmIsoFullPath file not found." -Exit
        }

        Write-Host "Mount $VmIsoFullPath to the DVD drive."
        $IsMounted = $false
        $TimeLeft = $TimeoutInSeconds

        do {
            Set-VMDvdDrive -VMName $Vm.hypervname -Path $VmIsoFullPath -ControllerNumber $VmDvdDrive.ControllerNumber -ControllerLocation $VmDvdDrive.ControllerLocation
            Wait-TestSuiteActivityComplete -ActivityName "Mount ISO $VmIsoFullPath" -TimeoutInSeconds 15
            
            $VmDvdDrivePath = (Get-VMDvdDrive -VMName $Vm.hypervname).Path
            if(![System.String]::IsNullOrEmpty($VmDvdDrivePath)) {
                $IsMounted = $true 
                break
            }

            Write-Host "Mount $VmIsoFullPath to the DVD drive failed, will retry in 2 minutes."
            $SleepTimePerIteration = 120
            Wait-TestSuiteActivityComplete -ActivityName "Mount ISO $VmIsoFullPath" -TimeoutInSeconds $SleepTimePerIteration
        } while ($TimeLeft -= $SleepTimePerIteration)
        if ($IsMounted) {
            Write-Host "Successfully mounted."
        }
        else {
            Write-Host "Unable to mount ISO within $TimeoutInSeconds seconds." -Exit
        }
    }
}

function Clean-Environment {   
    Write-Host "Clean the VM"    
    $VM = Get-VM -Name $Script:VM.Name -ErrorAction Ignore
    if ($VM -ne $null) {
        Write-Host "Clean up the VM. VMName: $($Script:VM.Name)"
        if($VM.State -ne 'off'){
            Stop-VM -Name $Script:VM.Name -Force
        }
        Remove-VM -Name $Script:VM.Name -Force
    }

    Write-Host "Clean the ISO and Folder"
    if (Test-Path $ISOForderPath) {
        Remove-Item $ISOForderPath -Force -Recurse
    }
    if(Test-Path $VmIsoFullPath) {
        Remove-Item $VmIsoFullPath -Force
    }
}

#------------------------------------------------------------------------------------------
# Check if the host machine has duplicate IP addresses that will be used as the gateways for this test suite's virtual networks
#------------------------------------------------------------------------------------------
Function Check-DuplicateIpAddresses {

    Write-Host "Check if the host machine has duplicate IP addresses that will be used as the gateways for this test suite's virtual networks."

    $isDuplicated = $false

    Write-Host "Get all the IP addresses for the physical network adapters in the host."
    $HostNetworkAdapterConfigurations = Get-WmiObject -Class Win32_NetworkAdapterConfiguration -Filter IPEnabled=TRUE -ComputerName .
    $HostNetworkAdapterConfigurations | Format-Table -Property Description, IPAddress -AutoSize

    Write-Host "Get all the IP addresses from XML configuration file."
    $ConfigureVirtualNetworks = $Script:Setup.lab.network.vnet
    $ConfigureVirtualNetworks | Format-Table -Property name, ip -AutoSize

    Write-Host "Check if the host has duplicate IP addresses that will be used by this test suite already configured on the physical network adapters in the host."
    foreach ($HostNetworkAdapterConfiguration in $HostNetworkAdapterConfigurations) {
        foreach ($HostNetworkAdapterIp in $HostNetworkAdapterConfiguration.IPAddress) {
            foreach ($ConfigureVirtualNetwork in $ConfigureVirtualNetworks) {
                if ($ConfigureVirtualNetwork.ip -eq $HostNetworkAdapterIp) {
                    Write-Host $("The IP address - " + $ConfigureVirtualNetwork.ip + " for virtual network - " + $ConfigureVirtualNetwork.name + " has already been configured on network adapter - " + $HostNetworkAdapterConfiguration.Description)
                    $isDuplicated = $true
                }
            }
        }
    }

    if ($isDuplicated) {
        Write-Host "There are IP addresses already configured on the host that will be used as the gateways for this test suite's virtual networks." -Exit
    }
}

function Deploy-VirtualNetworkSwitches {
    Write-Host "Deploy virtual network switches for this test suite."

    [array]$ExternalSwitchName = Get-VMSwitch  -SwitchType External 
    if($ExternalSwitchName -eq $null){
        Remove-VMSwitch -Name $VNetName -ErrorAction SilentlyContinue   

        Check-DuplicateIpAddresses

        Write-Host $("No external virtual switch found by name - " + $VNetName + ". Create a new one...")
        Write-Host "Get an existing physical network adapter."

        $NetworkAdapter = Get-NetAdapter -ErrorAction SilentlyContinue | Where-Object {($_.InterfaceDescription -notmatch "Hyper-V Virtual") -and ($_.Status -eq "Up") } | Select-Object -First 1 -ErrorAction SilentlyContinue
        if ($NetworkAdapter -eq $null) {
            Write-Host "No physical network adapter found. Please add hardware to this host machine." -Exit
        }
        else {
            $NetworkAdapter
            Write-Host "Create a new external virtual switch on this physical network adapter."                      
            New-VMSwitch -Name $VNetName -AllowManagementOS $true -NetAdapterInterfaceDescription $NetworkAdapter.InterfaceDescription
        }
    }

    $ExternalSwitchName = Get-VMSwitch  -SwitchType External 
    Write-Host "Find external switch: $ExternalSwitchName, it will use it as switch of test VM, update VNetName: $($ExternalSwitchName.Name)"
    $VNetName = $ExternalSwitchName[0].Name
    Write-Host "Update VNetName: $VNetName"
    Get-NetIPAddress -AddressFamily IPv4 | ForEach-Object{
        if($_.InterfaceAlias.replace("vEthernet (","").Contains($VNetName)){
            Write-Host "Network card with Name : $($_.InterfaceAlias)"
            Write-Host "Network card with IP : $($_.IPAddress)"
            $Script:Setup.lab.network.vnet.name = $VNetName
            $Script:Setup.lab.network.vnet.ip = $_.IPAddress
            $Script:Setup.lab.servers.vm.vnet = $VNetName
            $Script:Setup.Save("$InvocationPath\InstallPrerequisites.xml")            
        }
    }    
    Write-Host "VNetName: $VNetName"
}

#------------------------------------------------------------------------------------------
# Create ISO
#------------------------------------------------------------------------------------------
function Create-ISO {

    if(!(Test-Path $ISOForderPath)) {
        mkdir $ISOForderPath
    }

    Write-Host "Copy tools to the ISO directory"
    if(!(Test-Path "$ISOForderPath\Tools"))
    {
        mkdir "$ISOForderPath\Tools"
    }

    #upload tools
    if(Test-Path "Tools:"){
        Remove-PSDrive "Tools"
    }
    $Pwd = ConvertTo-SecureString $ReleaseMSIPassword -AsPlainText -Force
    $Cred = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $ReleaseMSIUserName,$Pwd
    $Setup.lab.tools.tool | ForEach-Object {       
        New-PSDrive -Name "Tools" -PSProvider FileSystem -Root $_.Path -Credential $Cred
        Write-Host "Copy $($_.Name) from to $($_.Path) to $ISOForderPath\Tools"
        Copy-Item "Tools:\$($_.Name)" -Destination "$ISOForderPath\Tools" -Force
        Remove-PSDrive "Tools"
    }    

    Write-Host "Copy the Answer file to the ISO directory, and rename it to unattend.xml"
    Copy-Item -Path $AnswerFilePath -Destination "$ISOForderPath\unattend.xml" -Force

    Write-Host "Copy the Scripts to the ISO directory"
    Copy-Item -Path $ScriptsPath -Destination $ISOForderPath -Force -Recurse

    Write-Host "Copy the InstallPrerequisites.xml to the ISO directory"
    Copy-Item -Path "$InvocationPath\InstallPrerequisites.xml" "$ISOForderPath\InstallPrerequisites.xml" -Force
    
    Write-Host "Copy the runcmd, run_client.cmd and run.ps1 to the ISO directory"
    Get-ChildItem -Path "$InvocationPath\run*" | ForEach-Object {
        Copy-Item -Path $_ -Destination $ISOForderPath -Force
    }

    Write-Host "Copy test MSI from ReleaseMSIFolder to  the ISO directory"
    Write-Host "$ReleaseMSIPath $ReleaseMSIUserName $ReleaseMSIPassword"
    if(!(Test-Path "$ISOForderPath\ReleaseMSI")) {
        mkdir "$ISOForderPath\ReleaseMSI"
    }
    net use "$ReleaseMSIPath" "$ReleaseMSIPassword" /user:"$ReleaseMSIUserName" 
    $Script:Setup.lab.testcase.test | ForEach-Object {
        Write-Host "Copy MSI : $ReleaseMSIPath\$($_.msi)"
        robocopy $ReleaseMSIPath "$ISOForderPath\ReleaseMSI" $($_.msi) /R:10
    }
    net use $ReleaseMSIPath /del

    Write-Host "Copy the TestSuitesFolder to the ISO directory"
    Get-ChildItem -Path $SourcePath -Exclude "_Helper" | ForEach-Object {
        Copy-Item $_ -Destination $ISOForderPath -Recurse -Force
    }

    Write-Host "Copy the source of CaseNumberValidator to the ISO directory"
    Copy-Item "$SourcePath\_Helper\CaseNumberValidator" -Destination "$ISOForderPath\CaseNumberValidator" -Recurse -Force

    Write-Host "Start generating ISO"
    try {
        cmd /c "$($oscdimgPath)\oscdimg.exe -j2 -o -m `"$($ISOForderPath)`" `"$($VmIsoFullPath)`"" 2>&1
    }
    catch {
        $ErrorMessage = $_.Exception.Message
        Write-Host $ErrorMessage
    }
    
}

#------------------------------------------------------------------------------------------
# Create virtual machine
#------------------------------------------------------------------------------------------
Function Create-TestSuiteVM {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    $Vm)
    
    Process {

        Write-Host "Start creating VM for $($Vm.hypervname)."

        Write-Host "Create a new virtual machine with name - $($Vm.hypervname) under location - $($Script:VmDirPath)."
        New-VM -Name $Vm.hypervname -Path $Script:VmDirPath

        Write-Host "Configure the CPU for this virtual machine to - $($Vm.cpu)."
        Set-VM -Name $Vm.hypervname -ProcessorCount $Vm.cpu

        $VmMem = [int]$Vm.memory * 1024 * 1024
        Write-Host "Configure the memory for this virtual machine to - $($Vm.memory) MB ($VmMem Bytes)."
        if (($Vm.minimumram -ne $null) -and ($Vm.maximumram -ne $null)) {
            $MinMem = [int]$Vm.minimumram * 1024 * 1024
            $MaxMem = [int]$Vm.maximumram * 1024 * 1024
            Write-Host "Minimum memory - $($Vm.minimumram) MB ($MinMem Bytes) and Maximum memory - $($Vm.maximumram) MB ($MaxMem Bytes)."
            Set-VM -Name $Vm.hypervname -DynamicMemory -MemoryStartupBytes $VmMem -MemoryMinimumBytes $MinMem -MemoryMaximumBytes $MaxMem
        }
        else {
            Set-VM -Name $Vm.hypervname -StaticMemory -MemoryStartupBytes $VmMem
        }

        Write-Host "Remove the existing virtual network adapters of this virtual machine."
        Remove-VMNetworkAdapter -VMName $Vm.hypervname
        
        Write-Host "Add a new virtual network adapter to this virtual machine, and connect it to the following virtual switches."
        $NicNumber = 0;
        [array]$ServerVnet    = $Vm.vnet
        $VirtualSwitch = $ServerVnet[0]
        foreach($ip in $Vm.ip) {
            if($ServerVnet.Count -gt 1)
            {
                $VirtualSwitch = $ServerVnet[$NicNumber]
            }
            
            Write-Host "set virtual network adapter for $VirtualSwitch"
            Add-VMNetworkAdapter -VMName $Vm.hypervname -SwitchName $VirtualSwitch
            $NicNumber++;
        }
        
        Write-Host "Check whether VHD file exists or not."
        $VmDisk = "$WinteropProtocolTesting\VM\InstallPrerequisites\\InstallPrerequisites.vhd"
        Write-Host "Vm.disk: $VmDisk"
        if (!(Test-Path $VmDisk)) {
            Write-Host "$($VmDisk) file not found." -Exit
        }

        Write-Host "Attach VHD to this virtual machine."
        Add-VMHardDiskDrive -VMName $Vm.hypervname -ControllerType IDE -ControllerNumber 0 -ControllerLocation 0 -Path $VmDisk

        Write-Host "Set the VM note with the Current User, Computer Name and IP Addresses (The note will be shown in VStorm Portal as VM Description)."
        $VmNote = $env:USERNAME + ": " + $Vm.name + ": " + $Vm.ip
        Set-VM -VMName $Vm.hypervname -Notes $VmNote
        Start-Sleep -Seconds 30
        Start-VM -Name $Vm.hypervname
    }
}
#------------------------------------------------------------------------------------------
# Mount the ISO to a particular virtual machine
#------------------------------------------------------------------------------------------
Function Mount-TestSuiteISO {
    Param(
    [Parameter(ValueFromPipeline=$True)]
    $Vm,
    [string]$VmIsoFullPath,
    [int]$TimeoutInSeconds = 3600,
    [int]$SleepTimePerIteration = 5
    )

    Process {

        Write-Host "Start mounting ISO for $($Vm.hypervname)."

        Write-Host "Get the DVD drive information of this virtual machine."
        $VmDvdDrive = Get-VMDvdDrive -VMName $Vm.hypervname | Select-Object -First 1 
        $VmDvdDrive | Format-List VMName,ControllerType,ControllerNumber,ControllerLocation,DvdMediaType,Path,IsDeleted

        Write-Host "Check whether ISO file exists or not."
        if (!(Test-Path $VmIsoFullPath)) {
            Write-Host "$VmIsoFullPath file not found." -Exit
        }

        Write-Host "Mount $VmIsoFullPath to the DVD drive."
        $IsMounted = $false
        $TimeLeft = $TimeoutInSeconds

        do {
            Set-VMDvdDrive -VMName $Vm.hypervname -Path $VmIsoFullPath -ControllerNumber $VmDvdDrive.ControllerNumber -ControllerLocation $VmDvdDrive.ControllerLocation
            Wait-TestSuiteActivityComplete -ActivityName "Mount ISO $VmIsoFullPath" -TimeoutInSeconds 15
            
            $VmDvdDrivePath = (Get-VMDvdDrive -VMName $Vm.hypervname).Path
            if(![System.String]::IsNullOrEmpty($VmDvdDrivePath)) {
                $IsMounted = $true 
                break
            }

            Write-Host "Mount $VmIsoFullPath to the DVD drive failed, will retry in 2 minutes."
            $SleepTimePerIteration = 120
            Wait-TestSuiteActivityComplete -ActivityName "Mount ISO $VmIsoFullPath" -TimeoutInSeconds $SleepTimePerIteration
        } while ($TimeLeft -= $SleepTimePerIteration)
        if ($IsMounted) {
            Write-Host "Successfully mounted."
        }
        else {
            Write-Host "Unable to mount ISO within $TimeoutInSeconds seconds." -Exit
        }
    }
}
Function Deploy-TestSuiteVirtualMachines {
    Download-VHD
    Create-ISO
    Read-Configurationfile
    $Script:VM | Sort -Property installorder | Create-TestSuiteVM
    $Script:VM | Mount-TestSuiteISO -VmIsoFullPath $VmIsoFullPath
}

function Wait-DeployVirtualMachines {    
    $Number = 0
    $TimeOut = 720
    while ($true) {
        Write-Host "Wait for the end of installation, attempts: $Number"

        $DVD = Get-VMDvdDrive -VMName "InstallTest" | Select-Object -First 1         
        if($DVD.Path -eq $null) {
            Write-Host "Regression is completed."
            break
        }
        if($Number -ge $TimeOut) {
            Write-Host "Timeout" Exit
        }
        Start-Sleep -Seconds 60
        $Number++
    }
    [array]$VmAdapter = get-vm -Name 'InstallTest' | Select-Object -ExpandProperty Networkadapters
    $Script:Setup.lab.servers.vm.ip = $VmAdapter.IPAddresses[0]
    $Script:VM.IP = $VmAdapter.IPAddresses[0]
    Write-Host "Find IP of VM : $($Script:VM.IP)"
}
function Parse-Trx {
    Param(
        [string]$protocol,
        [string]$errorMessage
    )
    $result = $false
    try{
        if(Test-Path "$ResultPath\$($protocol.replace("\","_")).trx") {
            [xml]$content = Get-Content "$ResultPath\$($protocol.replace("\","_")).trx"
            if($content.TestRun.ResultSummary.Counters.total -gt 0) {
                if([string]::IsNullOrEmpty($errorMessage)){
                    $result = $true
                }
                else{
                    if($content.TestRun.Results.UnitTestResult.Output.ErrorInfo.Message.Contains($errorMessage)){
                        Write-Host "Check ErrorMessage of $protocol"
                        $result = $true
                    }
                }                
            }
        }
    }
    catch{
        return $false
    }
    return $result
}

function CheckAutoCapture {
    $result = ""
    $etl = Get-ChildItem "$ResultPath\*.etl"
    if($etl.Count -eq 0){
        $result = "<tr><td>No etl files were found</td></tr>"
    }
    else {
        $result = "<tr><td>Find files:</td></tr>"
        $etl | ForEach-Object {
            $result += "<tr><td>$($_.Name)</td></tr>"
        }
    }     
    return $result
}

function GenerateTestCaseRunTable {
    [string]$result = ""
    $Script:Setup.lab.testcase.test | ForEach-Object {
        $result += "<tr align=`"center`">"
        $result += "<td>$($_.protocol)</td>"
        $result += "<td>$($_.case)</td>"
        $result += "$(({<td style="color: red">Failed</td>},{<td>Passed</td>})[$(Parse-Trx -protocol $_.protocol -errorMessage $($_.errorMessage) )])"
        $result += "$(({<td style="color: red">Failed</td>},{<td>Passed</td>})[$(Test-Path "$ResultPath\$($_.msi)" )])"
        $result += "</tr>"
    }
    if($result.Contains("Failed")){
        $Script:RegressionResult = "Failed"
    }
    return $result
}

function Generate-Report { 
    [string]$Script:RegressionResult = "Passed"

    Remove-PSDrive -Name TestVM -ErrorAction Ignore
    $Pwd = ConvertTo-SecureString $Script:VM.password -AsPlainText -Force
    $Cred = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList $Script:VM.username,$Pwd
    Write-Host "Connect Driver: \\$($Script:VM.ip)\c$\Temp"
    New-PSDrive -Name TestVM -PSProvider FileSystem -Root "\\$($Script:VM.ip)\c$\Temp" -Credential $Cred
    Get-ChildItem "TestVM:\TestResults\*.trx" | Copy-Item -Destination $ResultPath
    Get-ChildItem "TestVM:\TestResults\*.txt" | Copy-Item -Destination $ResultPath
    Get-ChildItem "TestVM:\TestResults\FileServerCaptureFileDirectory\*.etl" -Recurse -ErrorAction Ignore | Copy-Item -Destination $ResultPath
    Get-ChildItem "TestVM:\drop\*.msi" -Recurse |  Copy-Item -Destination $ResultPath
    Copy-Item "TestVM:\install.log" -Destination $ResultPath
    Remove-PSDrive -Name TestVM -ErrorAction Ignore -Force
    
    $SharePath = "$ShareResultPath\InstallPrerequisites-$([System.dateTime]::UtcNow.ToString("HHmmss"))"
    if(Test-Path $SharePath){
        Remove-Item $SharePath -Recurse -Force
    }
    mkdir $SharePath

    # CheckErrorMessages
    $ErrorMessage = ""
    $log = Get-Content "$ResultPath\install.log" -raw
    $log -split "\n" | ForEach-Object {
        if($_.Contains("Failed to")){
            $Script:RegressionResult = "Failed"
            $ErrorMessage += "<tr><td>"
            $ErrorMessage += $_
            $ErrorMessage += "</td></tr>"
        }
    }

    # Check auto cupture
    $autoCaptureResult = CheckAutoCapture

    #Generate html file for regression report
    Write-Host "Generate html file for regression report"
    $reportBody = GenerateTestCaseRunTable
    $content = Get-Content "$InvocationPath\..\RegressionResultTemplate.html";
    $content = $content `
        -replace "{{START_TIME}}",$Script:StartTime  `
        -replace "{{END_TIME}}","$([System.dateTime]::UtcNow.ToString("MM/dd/yyyy HH:mm:ss"))" `
        -replace "{{TRIGGERED_BY}}",$BuildUser `
        -replace "{{TESTSUITE_BRANCH}}",$TestSuiteBranch `
        -replace "{{HELPER_BRANCH}}",$HelperBranch `
        -replace "{{Log_Path}}",$SharePath `
        -replace "{{REPORT_BODY}}",$reportBody `
        -replace "{{REPORT_ERRORMESSAGE}}",$ErrorMessage `
        -replace "{{AUTO_CAPTURE}}",$autoCaptureResult
    $content | Out-File "$ResultPath\Report.html" -Encoding utf8 -Force
    #Upload result to share folder
    Write-Host "Upload result to share folder"
    Get-ChildItem $ResultPath -Recurse | Copy-Item -Destination $SharePath -Recurse -Force
    #Send Email
    Write-Host "Send Email"
    [string]$MailBody = $content
    $mailSubject = "[InstallPrerequisites Result] triggered by $BuildUser - $Script:RegressionResult"
    & "$InvocationPath\..\RegressionRunScripts\Common\SendMail.ps1" `
		    -SenderUser $SenderUsername `
		    -SenderPassword $SenderPassword `
            -MailSubject $mailSubject `
            -MailBody    $MailBody
}

function Main { 
    Prepare-ServerSetup
    Read-Configurationfile
    Check-HostPrerequisites
    Clean-Environment
    Deploy-VirtualNetworkSwitches
    Deploy-TestSuiteVirtualMachines
    Wait-DeployVirtualMachines
    Generate-Report
    Stop-Transcript
}

Main