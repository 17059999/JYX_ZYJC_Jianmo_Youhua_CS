param(
$File = 'C:\WorkingDir.txt'
)

if(Test-Path $File){
    $str = Get-Content $File
    $str = $str.ToString().Trim()
	Wttcmd.exe /SysInitKey /Key:WorkingDir /Value:$str
}
else 
{
  Write-Host "File $File doesn't exist, no need to update the WorkingDir"
  exit 0
}