# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

##############################################################################
#
# Microsoft Windows Powershell Scripting
# File:           Check-HtmlReport.ps1
# Purpose:        Check the CodeSign verification report.
# Requirements:   Windows Powershell 2.0
# Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
#                 Windows Server 2016 and later
#
##############################################################################

#----------------------------------------------------------------------------
# Parameters
# $ReportPath:            The path to the HTML report file
#----------------------------------------------------------------------------

param(
    [string]$ReportPath
)

Write-Host "ReportPath: $ReportPath"

$content = Get-Content $ReportPath -Raw
$reg = "<p id='Summary'>[\W\w]*<span class='Failed'>(\d+)</span>"

if ($content -match $reg) {
    if ($Matches[1] -gt 0) {
        throw "Total number of failures: $($Matches[1])."
    }
    else {
        Write-Host "Verification completed."
    }
}
else {
    throw "The report file does not match the correct format."
}
  