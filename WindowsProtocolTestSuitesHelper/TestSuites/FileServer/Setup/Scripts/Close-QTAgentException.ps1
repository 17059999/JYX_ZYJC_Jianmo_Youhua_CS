#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################################

Write-Host ((get-date).ToString() + ": Monitor QTAgent.exe process and close exception UI.")

While($true)
{
    # By default, QTAgent.exe run in background process, MainWindowHandle is 0
    # When QTAgent.exe throws exception, it will show the exception UI in desktop with below 2 attributes
    #     MainWindowHandle is set to non-zero value.
    #     MainWIndowTitle is "QTAgent.exe"
    $p = Get-Process | where {$_.MainWindowHandle -ne 0 -and $_.MainWIndowTitle -eq "QTAgent.exe"}
    if($p -ne $null)
    {
        Write-Host ((get-date).ToString() + ": Kill QTAgent.exe exception UI.")
        $p.Kill()
    }
    sleep 5
}

