# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

param(
[string]
$ProcessName = "mstsc"
)

$process = Get-Process $ProcessName

if($process.Name -eq $ProcessName){
	cmd /c ECHO $ProcessName EXIST > $env:HOMEDRIVE\$processName.finished.signal
	return 0
}

return 1

