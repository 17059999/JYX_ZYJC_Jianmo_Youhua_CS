#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Template_Config-Server.ps1
## Purpose:        Configure server for [***PROTOCOLNAME***] test suite lab automation.
## Version:        1.1 (26 Jun, 2008)
##
##############################################################################

Param(
[String]$toolsPath, 
[String]$scriptsPath, 
[String]$testSuitesPath,
[String]$logPath,
[String]$clientOS,

[String]$IPVersion,
[String]$workgroupDomain,
[string]$userNameInVM,
[string]$userPwdInVM,
[string]$domainInVM
)

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
$logFile = $logPath+"\Config-Server.ps1.log"
Start-Transcript $logFile -force

Write-Host "EXECUTING [Config-Server.ps1] ..." -foregroundcolor cyan
Write-Host "`$toolsPath       = $toolsPath"
Write-Host "`$scriptsPath     = $scriptsPath"
Write-Host "`$testSuitesPath  = $testSuitesPath"
Write-Host "`$logFile         = $logFile"
Write-Host "`$clientOS        = $clientOS"
Write-Host "`$IPVersion       = $IPVersion"
Write-Host "`$workgroupDomain = $workgroupDomain"
Write-Host "`$userNameInVM    = $userNameInVM"
Write-Host "`$userPwdInVM     = $userPwdInVM"
Write-Host "`$domainInVM      = $domainInVM"

Write-Host "Put current dir as $scriptsPath."
pushd $scriptsPath

Write-Host  "Verifying environment..."
.\Verify-NetworkEnvironment.ps1 $IPVersion $workgroupDomain
 
#-----------------------------------------------------
# Begin to config server
#-----------------------------------------------------
[***Config your server here***]

#-----------------------------------------------------
# Finished to config server
#-----------------------------------------------------
popd
Write-Host "Write signal file: config.finished.signal to system drive."
cmd /C ECHO CONFIG FINISHED>$env:HOMEDRIVE\config.finished.signal

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "Config finished."
Write-Host "EXECUTE [Config-Server.ps1] FINISHED (NOT VERIFIED)."
Stop-Transcript

exit 0