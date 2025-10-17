# Copyright (c) Microsoft. All rights reserved.
# Licensed under the MIT license. See LICENSE file in the project root for full license information.

Write-Host ((get-date).ToString() + ": Monitor QTAgent32.exe process and close exception UI.")

While($true)
{
    # By default, QTAgent32.exe run in background process, MainWindowHandle is 0
    # When QTAgent32.exe throws exception, it will show the exception UI in desktop with below 2 attributes
    #     MainWindowHandle is set to non-zero value.
    #     MainWIndowTitle is "QTAgent32.exe"
    $p = Get-Process | where {$_.MainWindowHandle -ne 0 -and $_.MainWIndowTitle -eq "QTAgent32.exe"}
    if($p -ne $null)
    {
        Write-Host ((get-date).ToString() + ": Kill QTAgent32.exe exception UI.")
        $p.Kill()
    }
    sleep 5
}

