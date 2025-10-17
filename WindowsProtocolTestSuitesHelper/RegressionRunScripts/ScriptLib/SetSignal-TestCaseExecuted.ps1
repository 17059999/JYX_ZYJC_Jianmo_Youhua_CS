param
(
[int]$serialNum,
[string]$test,
[string]$resultFile,
[string]$nextTest
)

$signalFile = "$env:SystemDrive\TestCaseExecuted$serialNum.signal"

$test       | Out-File $signalFile -force
$resultFile | Out-File $signalFile -append -force
$nextTest   | Out-File $signalFile -append -force
