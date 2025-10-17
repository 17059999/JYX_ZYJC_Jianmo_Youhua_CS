##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################
###########################################################################################
##
## Microsoft Windows Powershell Scripting
## File:           GenerateCSVWithDynamicVMName.ps1
## Purpose:        Alter the Regression Virtual Machine Name By Appending Random Numbers

## Requirements:   Windows Powershell 5.0
## Supported OS:   Windows Server 2012 R2, Windows Server 2016, and later.
## Input parameter:
##      $CSVFilePath       :  Folder Path On Regression Virtual Machine
###########################################################################################

Param(
    [string]$CSVFilePath
)

$randomNumber = (-join ((0x30..0x39) + ( 0x41..0x5A) + ( 0x61..0x7A) | Get-Random -Count 3  | % {[char]$_}));
$randomNumber = $randomNumber.ToUpper();

$file = Get-Content $CSVFilePath;

$dash = "-";

$csvItems = Import-Csv $CSVFilePath

foreach($Item in $csvItems)
{
    if ($Item."VM Name" -and $Item."VM Name" -ne "N/A"){
        
        $newVmName = "{0}{1}{2}" -f $randomNumber,$dash,$Item."VM Name"
        $vmName = $Item."VM Name"

        $file = $file.replace($vmName,$newVmName);
    }
}

$file | Out-File -FilePath  $CSVFilePath