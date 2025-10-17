# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

Write-Host  "Write signal file to system drive."
$MSIScriptsFile = [System.IO.Directory]::GetFiles("$env:HOMEDRIVE\MicrosoftProtocolTests", "ParamConfig.xml", [System.IO.SearchOption]::AllDirectories)
[String]$TestSuiteScriptsFullPath = [System.IO.Directory]::GetParent($MSIScriptsFile)
cmd /c ECHO $TestSuiteScriptsFullPath >$env:HOMEDRIVE\MSIInstalled.signal
