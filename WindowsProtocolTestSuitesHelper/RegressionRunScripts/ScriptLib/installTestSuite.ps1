# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

Param(
    [switch]$install,
    [string]$sourceFolderPath,
    [string]$targetFolderPath
)

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($sourceFolderPath -eq $null -or $sourceFolderPath -eq "")
{
    $sourceFolderPath = '.\'
}
if ($targetFolderPath -eq $null -or $targetFolderPath -eq "")
{
    $targetFolderPath = $Env:SystemDrive +'\MicrosoftProtocolTests'
} 

Get-ChildItem -Path $sourceFolderPath\* -Include *.msi,*.zip | ForEach-Object { 
    if(([IO.FileInfo]$($_.Name)).Extension -eq '.zip') {    
        $targetFolder = "$targetFolderPath\$($_.Basename)"
        if($install -eq $true) {
            Write-Host "Expand Archive $($_.Name)"                        
            if ($psversiontable.PSVersion.Major -ge 5)
            {
                Expand-Archive $_.FullName -DestinationPath $targetFolder	
            }
            else
            {
                $shell = New-Object -com shell.application
                $zip = $shell.NameSpace("$_.FullName")
                if(!(Test-Path -Path $targetFolder))
                {
                    New-Item -ItemType directory -Path $targetFolder
                }
                $shell.Namespace($targetFolder).CopyHere($zip.items(), 0x14)
            }
        } else {
            Write-Host "remove $targetFolder"
            Get-ChildItem $targetFolder -Recurse | Remove-Item -Recurse -Force
            Remove-Item $targetFolder -Force
        }
    } else {        
        if($install -eq $true) {
            Write-Host "install $($_.Name)"
            cmd /c msiexec -i $_.FullName -q
        } else {
            Write-Host "uninstall $($_.Name)"
            cmd /c msiexec -uninstall $_.FullName -q
        }
    }
}