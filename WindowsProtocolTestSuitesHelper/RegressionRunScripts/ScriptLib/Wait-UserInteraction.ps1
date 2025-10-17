param
(
[int]$serialNum,
[int]$timeout
)

$continueExecSignalFile = "$env:SystemDrive\ContinueExec$serialNum.signal"
$cancelExecSignalFile = "$env:SystemDrive\cancelExec$serialNum.signal"

for (; $timeout -gt 0; $timeout = $timeout - 2)
{
    if (Test-Path $continueExecSignalFile)
    {
        Write-Host "Continue execution"
        return $true
    }
    if (Test-Path $cancelExecSignalFile)
    {
        Write-Host "Cancel execution"
        return $false
    }
    Write-Host "." -noNewLine
    Start-Sleep 2
}


Write-Host "Wait timeout"
return $false
