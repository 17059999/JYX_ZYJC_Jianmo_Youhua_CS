#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Execute-TestSuite-Ex.ps1
## Purpose:        Executes the test suite on Hyper-V host machine.
## Version:        1.1 (31 March, 2009)
##
##############################################################################

param(
[string]$protocolName, 
[string]$cluster, 

[string]$srcVMPath        = "\\nmtest\PETLabStore\VMLib",
[string]$srcScriptLibPath = "\\nmtest\PETLabStore\ScriptLib",
[string]$srcToolPath      = "\\nmtest\PETLabStore\Tools",
[string]$srcTestSuitePath = "\\nmtest\PETLabStore\TestSuite",
[string]$ISOPath          = "\\pt3controller\ISO",

#The following is for WTT Mix parameters
[string]$clientOS              = "Vista",
[string]$serverOS              = "W2K8",
[string]$workgroupDomain       = "Workgroup",   #"Domain"
[string]$IPVersion             = "IPv4",        #"IPv6"
[string]$ClientCPUArchitecture = "x86",         #"x64"
[string]$ServerCPUArchitecture = "x86",         #"x64"
#End of WTT Mix parameters

[string]$workingDir       = "D:\PCTLabTest", 
[string]$VMDir            = "D:\VM",
[string]$testResultDir    = "D:\PCTLabTest\TestResults",

[string]$userNameInVM     = "administrator",
[string]$userPwdInVM      = "Password01!",
[string]$domainInVM       = "contoso.com", 
[string]$testDirInVM      = "C:\Test" 
)


#----------------------------------------------------------------------------
# Change the working directory.
#----------------------------------------------------------------------------
Write-Host "Set $workingDir\ScriptLib as current dir."
Push-Location $workingDir\ScriptLib

#----------------------------------------------------------------------------
# Starting script
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Execute-TestSuite.ps1] ..." -foregroundcolor cyan
Write-Host "`$protocolName          = $protocolName"
Write-Host "`$cluster               = $cluster"

Write-Host "`$srcVMPath             = $srcVMPath"
Write-Host "`$srcScriptLibPath      = $srcScriptLibPath"
Write-Host "`$srcToolPath           = $srcToolPath"
Write-Host "`$srcTestSuitePath      = $srcTestSuitePath"
Write-Host "`$ISOPath               = $ISOPath"

Write-Host "`$clientOS              = $clientOS"
Write-Host "`$serverOS              = $serverOS"
Write-Host "`$workgroupDomain       = $workgroupDomain"
Write-Host "`$IPVersion             = $IPVersion"
Write-Host "`$ClientCPUArchitecture = $ClientCPUArchitecture"
Write-Host "`$ServerCPUArchitecture = $ServerCPUArchitecture"

Write-Host "`$workingDir            = $workingDir"
Write-Host "`$VMDir                 = $VMDir"
Write-Host "`$testResultDir         = $testResultDir"

Write-Host "`$userNameInVM          = $userNameInVM"
Write-Host "`$userPwdInVM           = $userPwdInVM"
Write-Host "`$domainInVM            = $domainInVM"
Write-Host "`$testDirInVM           = $testDirInVM"

#----------------------------------------------------------------------------
# Verify required parameters
#----------------------------------------------------------------------------
if ($protocolName -eq $null -or $protocolName -eq "")
{
    Throw "Parameter protocolName is required."
}
$srcTestSuitePath = "$srcTestSuitePath\$protocolName"


#----------------------------------------------------------------------------
# Cache the domain, username and password, so that other functions can use it if not provided by their caller.
#----------------------------------------------------------------------------
if ($workgroupDomain -eq "Domain")
{
    $global:domain = $domainInVM
}
else
{
    $global:domain = $null
}
$global:usr = $userNameInVM
$global:pwd = $userPwdInVM


#----------------------------------------------------------------------------
# Cache the test result dir
#----------------------------------------------------------------------------
$global:testResultDir = $testResultDir


#----------------------------------------------------------------------------
# Execute protocol entry script.
#----------------------------------------------------------------------------
Write-Host "Execute protocol entry script ..." -foregroundcolor Yellow
& ..\$protocolName\Scripts\Execute-ProtocolTest-Ex.ps1 `
$protocolName $cluster `
$srcVMPath $srcScriptLibPath $srcToolPath $srcTestSuitePath $ISOPath  `
$clientOS $serverOS $workgroupDomain $IPVersion $ClientCPUArchitecture $ServerCPUArchitecture `
$workingDir $VMDir $testResultDir `
$userNameInVM $userPwdInVM $domainInVM $testDirInVM

Pop-Location

#----------------------------------------------------------------------------
# Ending script
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Execute-TestSuite.ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

exit 0
