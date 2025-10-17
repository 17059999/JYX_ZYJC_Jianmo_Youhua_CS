
###########################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
###########################################################################################

param(
    [Parameter(ValueFromPipeline = $True, Mandatory = $True)]
    [string]$resourceGroup,

    [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
    [string]
    $VMName,
    
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
    [string]
    $DeployName,
    
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
    [string]
    $templateFilePath,
    
    [Parameter(Mandatory = $true, ValueFromPipeline = $true)] 
    [string]
    $parametersFilePath
)
    
#******************************************************************************
# Script body
# Execution begins here
#******************************************************************************
$ErrorActionPreference = "Stop"
#Get-AzContext

$preDeploy = Get-AzResourceGroupDeployment -ResourceGroupName $resourceGroup | Where-Object {$_.DeploymentName -eq $DeployName}| Select-Object -Property DeploymentName, ProvisioningState
if (($preDeploy | Where-Object {$_.ProvisioningState -eq "Creating"} | Measure-Object).Count -gt 0) {
    $errorContent = "Same job is running on azure, please try re-deploy again later. Job Name:$DeployName"
    throw $errorContent
}
    
$preDeploy | ForEach-Object -Process {
    Remove-AzResourceGroupDeployment -Name $DeployName -ResourceGroupName $resourceGroup
    Write-Host "DeployName = $DeployName"
    }
    
# Split the string by the - character
#$vmPrefixIDParts = $VMName -split '-'
#$vmPostFix = $VMName.Replace($vmPrefixIDParts[0] + '-', '')

# Get VM Disk
#$disks = Get-AzDisk -ResourceGroupName $resourceGroup | Where-Object Name -match $vmPostFix | Select-Object Name
 $disks = Get-AzDisk -ResourceGroupName $resourceGroup | Where-Object {$_.Name.StartsWith($VMName + "_")} | Select-Object Name

# Get VM Network Security Groups
#$networkSecurityGroups = Get-AzNetworkSecurityGroup -ResourceGroupName $resourceGroup | Where-Object Name -match ($vmPostFix + "_nsg") | Select-Object Name
$networkSecurityGroups = Get-AzNetworkSecurityGroup -ResourceGroupName $resourceGroup | Where-Object {$_.Name.Equals($VMName + "_nsg")} | Select-Object Name

# Get VM Network interface
#$networkInterfaces = Get-AzNetworkInterface -ResourceGroupName $resourceGroup | Where-Object Name -Match $vmPostFix | Select-Object Name
$networkInterfaces = Get-AzNetworkInterface -ResourceGroupName $resourceGroup | Where-Object Name -Match $VMName | Select-Object Name

# Get VM 
#$virtualMachine = Get-AzVM -ResourceGroupName $resourceGroup | Where-Object Name -Match $vmPostFix | Select-Object Name

#Write-Warning "Starting remove Azure VM $vmPostFix and all dependencies"
Write-Warning "Starting remove Azure VM $VMName"

# Remove Azure VM
#if ($virtualMachine -ne $null) {
#    $virtualMachine | ForEach-Object -Process {
#        $deleteVM = $_.Name
#        Write-Warning "Starting remove Azure VM $deleteVM"
#        Remove-AzVM -ResourceGroupName $resourceGroup -Name $deleteVM -Force > $null
#    }
#}
#else{
#    Write-Warning "No VM to remove"
#}
Write-Warning "Starting remove Azure VM $VMName"
Remove-AzVM -ResourceGroupName $resourceGroup -Name $VMName -Force > $null

# Remove Azure Disk
#if ($disks -ne $null){
    $disks | ForEach-Object -Process {
        $deleteDiskName = $_.Name
        Write-Host "deleteDiskName = $deleteDiskName"
#        Write-Warning "Starting remove Disk $deleteDiskName of VM $virtualMachine "
        Write-Warning "Starting remove Disk $deleteDiskName of VM $VMName "
        Remove-AzDisk -ResourceGroupName $resourceGroup -DiskName $deleteDiskName -Force > $null;
    }
#}
    
# Remove Azure Network interface
#if ($networkInterfaces -ne $null){
    $networkInterfaces | ForEach-Object -Process {
        $deleteInterfaceName = $_.Name
        Write-Host "deleteInterfaceName = $deleteInterfaceName"
#        Write-Warning "Starting remove NetworkInterface $deleteInterfaceName of VM $virtualMachine "
        Write-Warning "Starting remove NetworkInterface $deleteInterfaceName of VM $VMName "
        Remove-AzNetworkInterface -ResourceGroupName $resourceGroup -Name $deleteInterfaceName -Force > $null;
    }
#}
 
# Remove Azure Network Security Groups
#if ($networkSecurityGroups -ne $null){
    $networkSecurityGroups | ForEach-Object -Process {
        $deleteSecurityGroupName = $_.Name
        Write-Host "deleteSecurityGroupName = $deleteSecurityGroupName"
        Write-Warning "Starting remove SecurityGroup $deleteSecurityGroupName of VM $virtualMachine "
        Remove-AzNetworkSecurityGroup -ResourceGroupName $resourceGroup -Name $deleteSecurityGroupName -Force > $null;
    }
#}
    
#Write-Host "Azure VM $vmPostFix Remove completed"
Write-Host "Azure VM $VMName Remove completed"

# push current location
$currentPath = Split-Path -parent $MyInvocation.MyCommand.Definition
Push-Location $currentPath
# Start the deployment
Write-Host "Starting deploy Azure VM: $VMName"

#Write-Host "Starting deploy Resource Group: $resourceGroup"
#Write-Host "Starting deploy Deploy Name: $DeployName"

New-AzResourceGroupDeployment -Name $DeployName -ResourceGroupName $resourceGroup -TemplateFile $templateFilePath -TemplateParameterFile $parametersFilePath -AsJob
Pop-Location

