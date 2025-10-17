#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-LDSUniqueInstance.ps1
## Purpose:        Install unqiue LDS Instance
##           
##############################################################################


Param(
#where the answer file is placed
[string]$answerFilePath = "c:\LDSUniqueAnswerFile.txt",
#name of LDS instance
[string]$InstanceName = "instance1",
#normal LDAP port for instance
[string]$Port = 50000,
#SSL LDAP port for instance
[string]$SSLPort = 50001,
#fill it for new application naming context
[string]$AppNC = $null
)

write-host "installing AD LDS Role"
add-windowsfeature adlds | write-host

if(test-path -Path $answerFilePath)
{
  $text = Get-Content -Path $answerFilePath
  if($text -eq $null)
  {
    write-host "$answerFilePath does not have a valid answer file"
    return 1
  }
}
else
{
  write-host "cannot find answer file at path $answerFilePath"
  return 1
}
$text = $text.Replace("%INSTName%",$InstanceName)

$text = $text.Replace("%INSTPort%",$Port)
$text = $text.Replace("%INSTSSLPort%",$SSLPort)

$tmpFilePath = "c:\LDSTmp.tmp"
if(test-path -Path $tmpFilePath)
{
  Remove-Item $tmpFilePath
}
$tmpFile = New-Item -ItemType file -Path $tmpFilePath
$text>>$tmpFile

if($AppNC -ne $null)
{
  "NewApplicationPartitionToCreate=$AppNC">>$tmpFile
}

write-host "installing new LDS instance: $InstanceName"
cmd.exe /c %systemroot%\adam\adaminstall /answer:$tmpFilePath | write-host


return 0