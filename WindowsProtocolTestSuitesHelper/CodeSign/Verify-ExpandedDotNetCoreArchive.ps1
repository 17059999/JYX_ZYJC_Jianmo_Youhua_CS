# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Verify-ExpandedDotNetCoreArchive.ps1
# Purpose:        Verify the digital signature and version of an expanded archive.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $ArchiveName:                   Name of the .Net Core archive
# $ArchiveExtension:              Extension of the .net core archive
# $ExpandedArchivePath:           Full path to the expanded archive
# $CodeSigningFilesPath:          Full path to CodeSigningFiles.json
# $SignerSubject:                 The signer subject
# $ReportRootPath:                Full path to root path of the code sign report output
#----------------------------------------------------------------------------

param
(
    [string]$ArchiveName,
    [string]$ArchiveExtension = ".zip",
    [string]$ExpandedArchivePath,
    [string]$CodeSigningFilesPath,
    [string]$SignerSubject,
    [string]$ReportRootPath
)

#==========================================================================================
# Function Definitions
#==========================================================================================

#------------------------------------------------------------------------------------------
# Check file digital signature
# .msi, .dll, .exe, .ps1, .psm1 file
#------------------------------------------------------------------------------------------
function Check-FileDigitalSignature {
    param(
        [string]$Path
    )

    if (-not (Test-Path $Path)) {
        throw "$Path is not a valid path."
    }

    $signerCert = (Get-AuthenticodeSignature $Path).SignerCertificate
    if ((Get-Date) -ge $signerCert.NotBefore -and ((Get-Date) -le $signerCert.NotAfter) -and ($signerCert.Subject -eq $SignerSubject)) {
        $signStatus = (Get-AuthenticodeSignature $Path).Status
        return $signStatus  
    }

    return "NotValid"
}

#------------------------------------------------------------------------------------------
# Check file version
# .dll, .exe file
#------------------------------------------------------------------------------------------
function Check-FileVersion {
    param(
        [string]$Path
    )
    
    if (-not (Test-Path $Path)) {
        throw "$Path is not a valid path."
    }
    
    $fileVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($Path).FileVersion
    $productVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($Path).ProductVersion

    if (($fileVersion -eq $null) -or [string]::IsNullOrEmpty($fileVersion.Trim())) {
        return [string]::Empty
    }
    
    if (($productVersion -eq $null) -or [string]::IsNullOrEmpty($productVersion.Trim())) {
        return [string]::Empty
    }
    
    if ($fileVersion -eq $productVersion) {
        return $fileVersion
    }
    
    return [string]::Empty
}

#------------------------------------------------------------------------------------------
# Check single file version, digtal signature
# Return file name, version, digtal signature status in an array
#------------------------------------------------------------------------------------------
function Check-SingleFileCodeSign {
    param(
        [string]$FilePath
    )

    if (-not (Test-Path $FilePath)) {
        throw "$FilePath is not a valid path."
    }

    $fileName = $FilePath.Substring($FilePath.LastIndexOf("\") + 1)
    $fileVersion = Check-FileVersion -Path $FilePath 
    $fileDigtalSignature = Check-FileDigitalSignature -Path $FilePath

    $node = [PSCustomObject]@{
        FileName = $fileName
        Version  = $fileVersion
        CodeSign = $fileDigtalSignature
    }

    return $node
}

#------------------------------------------------------------------------------------------
# Check whether the current file type is included in list specified or not
#------------------------------------------------------------------------------------------
function Check-IsFileTypeIncluded {
    param(
        [string]$FileName,
        $TypesIncluded
    )
    
    foreach ($type in $TypesIncluded) {
        if ($FileName.ToLower().EndsWith($type.ToString())) {
            return $true
        }
    }
    
    return $false
}

#------------------------------------------------------------------------------------------
# Check multiple file version, digtal signature in single folder
# File type: .dll, .exe, .ps1, .psm1
# Return file name, version, digtal signature status in an ArrayList
#------------------------------------------------------------------------------------------
function Check-SingleFolderCodeSign {
    param(
        $FileList,
        $TypesIncluded
    )

    if ($FileList -eq $null) {
        throw "File list is null."
    }

    $list = New-Object -TypeName System.Collections.ArrayList
    foreach ($file in $FileList) {
        $isIncluded = Check-IsFileTypeIncluded -FileName $file -TypesIncluded $TypesIncluded
        if (-not $isIncluded) {
            continue
        }

        $fileCodeSign = Check-SingleFileCodeSign -FilePath $file
        if ($fileCodeSign -ne $null) {
            $list.Add($fileCodeSign) | Out-Null
        }
    }
    
    return $List
}

#------------------------------------------------------------------------------------------
# Check file version, digtal signature in multiple folders
# File type: .dll, .exe, .ps1, .psm1
# Return file name, version, digtal signature status in an ArrayList
#------------------------------------------------------------------------------------------
function Check-MultipleFoldersCodeSign {
    param(
        $FolderList
    )

    if ($FolderList -eq $null) {
        throw "Folder list is null"
    }

    $list = New-Object -TypeName System.Collections.ArrayList
    $toCheck = @(".dll", ".ps1", ".psm1", ".exe")
    foreach ($folder in $FolderList) {
        if (-Not (Test-Path $folder.Folder)) {
            continue
        }

        $folderCodeSign = Check-SingleFolderCodeSign -FileList $folder.FileList -TypesIncluded $toCheck
        if ($folderCodeSign -ne $null) {
            $node = [PSCustomObject]@{
                FolderPath    = $folder.Folder 
                CodeSignTable = $folderCodeSign 
            }

            $list.Add($node) | Out-Null
        }
    }

    return $list
}

#------------------------------------------------------------------------------------------
# Generate report
#------------------------------------------------------------------------------------------
function Generate-HtmlReport {
    param(
        $FileCodeSignResult
    )

    $reportName = "$ArchiveName$ArchiveExtension.CodeSignVerification.html"
    $htmlTitle = "Code Sign Report"

    [string]$fileCs = [string]::Empty
    [int]$csPassed = 0
    [int]$csFailed = 0

    foreach ($f in $FileCodeSignResult) {
        $fPath = $f.FolderPath
        $fileCs += "
            <p>$fPath</p>
            <table>
                <tr>
                    <th>FileName</th>
                    <th>Version</th>
                    <th>CodeSign</th>
                </tr>"
        foreach ($t in $f.CodeSignTable) {
            [string]$fileName = $t.FileName
            $fileVersion = $t.Version
            $fileCodeSign = $t.CodeSign

            $fileLog = "";
            $fileLog += "FileName: $fileName;"
            if (-not [string]::IsNullOrEmpty($fileVersion)) {
                $fileLog += " Version: $fileVersion;"
            }
            $fileLog += " CodeSign: $fileCodeSign"
            Write-Host "$fileLog"

            if ($fileCodeSign -ne "Valid") {
                $trTmp = "
                    <tr class='CodeSignFailed'>
                        <td>$fileName</td>
                        <td>$fileVersion</td>
                        <td>$fileCodeSign</td>
                    </tr>"
                $csFailed += 1
            }
            else {
                $trTmp = "
                    <tr>
                        <td>$fileName</td>
                        <td>$fileVersion</td>
                        <td>$fileCodeSign</td>
                    </tr>"
                $csPassed += 1
            }
            $fileCs += $trTmp
        }
        $fileCs += "</table>"
    }   

    $htmlTables = $fileCs

    $htmlBegin = "
    <html>
        <head>
            <title>$htmlTitle</title>
            <style>
                BODY {
                    background-color: lavender;
                }

                TABLE {
                    border-width: 1px;
                    border-style: solid;
                    border-color: black;
                    border-collapse: collapse;
                    Margin: 20px;
                }

                TH {
                    border-width: 1px;
                    padding: 8px;
                    border-style: solid;
                    border-color: black;
                    background-color: thistle;
                }

                TD {
                    border-width: 1px;
                    padding: 8px;
                    border-style: solid;
                    border-color: black;
                }

                P {
                    margin-left: 20px;
                    font-size: 16;
                }

                #ReportName {
                    text-align: center;
                }

                .CodeSignFailed {
                    background-color: red;
                }

                #Summary {
                    text-align: center;
                    margin-top: 20px;
                    margin-bottom: 20px;
                }

                .Passed {
                    color: green;
                }

                .Failed {
                    color: red;
                }
            </style>
        </head>"

    $htmlH1 = "
    <body>
        <h1 id='ReportName'>$reportName</h1>
    "
    $htmlEnd = "
        </body>
    </html>
    "
    $summary = "
    <p id='Summary'>
        <span class='Passed'>Passed</span>/<span class='Failed'>Failed</span>:&nbsp;&nbsp;&nbsp;&nbsp;<span class='Passed'>$csPassed</span>/<span class='Failed'>$csFailed</span>
    </p>"

    $htmlPage = $htmlBegin + $htmlH1 + $summary + $htmlTables + $htmlEnd

    if (-not (Test-Path $ReportRootPath)) {
        New-Item $ReportRootPath -ItemType Directory
    }

    $htmlPage | Out-File "$ReportRootPath/$reportName" -Encoding utf8
}

#==========================================================================================
# Main Script Body
#==========================================================================================

#==========================================================================================
# Delete CodeSignSummary
#==========================================================================================
Write-Host "============================================================"
Write-Host "           Delete CodeSignSummary                          "
Write-Host "============================================================"

Get-ChildItem $ExpandedArchivePath | Where-Object {
    $_.Name -match "CodeSignSummary"
} | ForEach-Object {
    Remove-Item $_.FullName -Force
}

#==========================================================================================
# Check singed files code sign info
#==========================================================================================
Write-Host "============================================================"
Write-Host "           Check singed files code sign info                "
Write-Host "============================================================"

$codeSigningFiles = Get-Content $CodeSigningFilesPath | ConvertFrom-Json

$folderList = @()
[array]$subfolders = Get-ChildItem $ExpandedArchivePath | Where-Object { $_.PSIsContainer }
foreach ($subfolder in $subfolders) {
    [array]$files = Get-ChildItem $subfolder.FullName -Recurse | Where-Object { 
        ($codeSigningFiles.Contains($_.Name)) -or (($_.Extension -eq ".ps1") -or ($_.Extension -eq ".psm1")) 
    } | ForEach-Object { $_.FullName }

    if (($files -ne $null) -and ($files.Length -gt 0)) {
        $node = @{
            Folder   = $subfolder.FullName
            FileList = $files
        }

        $folderList += $node
    }
}

[array]$filesInRootFolder = Get-ChildItem $ExpandedArchivePath | Where-Object { -not $_.PSIsContainer } | Where-Object {
    ($codeSigningFiles.Contains($_.Name)) -or (($_.Extension -eq ".ps1") -or ($_.Extension -eq ".psm1")) 
} | ForEach-Object { $_.FullName }

if (($filesInRootFolder -ne $null) -and ($filesInRootFolder.Length -gt 0)) {
    $node = @{
        Folder   = $ExpandedArchivePath
        FileList = $filesInRootFolder
    }

    $folderList += $node
}

foreach ($node in $folderList) {
    Write-Host "Files to be checked in $($node.Folder)"
    foreach ($file in $node.FileList) {
        Write-Host "    $file"
    }
    Write-Host "Total files count: $($node.FileList.Length)"
}

$csResult = Check-MultipleFoldersCodeSign -FolderList $folderList

#==========================================================================================
# Generate report
#==========================================================================================
Write-Host "============================================================"
Write-Host "                   Start to generate report                 "
Write-Host "============================================================"

Generate-HtmlReport -FileCodeSignResult $csResult

#==========================================================================================
# Code Sign check completed
#==========================================================================================
Write-Host "============================================================"
Write-Host "         Code Sign check completed                               "
Write-Host "============================================================" 