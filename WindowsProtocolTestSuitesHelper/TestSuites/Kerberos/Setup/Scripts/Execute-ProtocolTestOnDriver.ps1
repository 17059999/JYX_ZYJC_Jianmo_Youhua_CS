###############################################################
## Copyright (c) Microsoft Corporation. All rights reserved. ##
###############################################################

Function Write-Log
{
    Param ([Parameter(ValueFromPipeline=$true)] $text)
    $date = Get-Date
    Write-Output "`r`n$date $text"
}

# Find specified file or directory from the MicrosoftProtocolTests folder on 
# the computer. The folder will be created after the test suite MSI is installed.
function GetItemInTestSuite($Name)
{
    

    # Try if the name specified is a directory
    [string]$Path = [System.IO.Directory]::GetDirectories("$env:HOMEDRIVE\Kerberos-TestSuite-ServerEP",`
                   $Name,[System.IO.SearchOption]::AllDirectories)
 
    if(($Path -eq $null) -or ($Path -eq ""))
    {
        # Try if the name specified is a file
        [string]$Path = [System.IO.Directory]::GetFiles("$env:HOMEDRIVE\Kerberos-TestSuite-ServerEP",`
                        $Name,[System.IO.SearchOption]::AllDirectories)
    }

    return $Path
}

#----------------------------------------------------------------------------
# Global variables
#----------------------------------------------------------------------------
$WorkingPath = "c:\temp"
$env:Path += ";c:\temp;c:\temp\Scripts"
$DataFile = "C:\Temp\Scripts\ParamConfig.xml"

#-----------------------------------------------------------------------------------------------
# Create $logFile if not exist
#-----------------------------------------------------------------------------------------------
$logFile = ".\" + $MyInvocation.MyCommand.Name + ".log"
if(!(Test-Path -Path $logFile))
{
	New-Item -Type File -Path $logFile -Force
}
Start-Transcript $logFile -Append

# Get parameters
[xml]$KrbParams = Get-Content -Path $DataFile

# Try to access AP02 to make sure the trust is solid between DC01 and DC02.
$Ap02Name = $KrbParams.Parameters.TrustRealm.FileShare.NetBiosName -replace '\$$'
$Ap02Domain = $KrbParams.Parameters.TrustRealm.RealmName

[String]$ShareOnAp02 = [String]"\\$Ap02Name.$Ap02Domain\Share"
$RetryTimes = 90

while($RetryTimes -ge 0)
{
    $IfExist = Test-path $ShareOnAp02
    if ($IfExist -eq $true)
    {
        Write-Log "Can connect to $ShareOnAp02"
        break;
    }
    else
    {
        Write-Log "Cannot connect to $ShareOnAp02, retry it later"
        $RetryTimes--;
        sleep 10
    }
}

if ($RetryTimes -le 0)
{
    Write-Log "Cannot connect to $ShareOnAp02 in 15 minutes, quit current script"
    exit 1
}

$BatchFolderOnVM = GetItemInTestSuite "Batch"

pushd $BatchFolderOnVM   

Write-Log "Start to run all test cases..."
Stop-Transcript
$BatchToRunAllCase = "RunAllTestCases.ps1" 
Write-Log "`$BatchFolderOnVM=$BatchFolderOnVM"
powershell.exe $BatchFolderOnVM\$BatchToRunAllCase
Write-Log "finish"


