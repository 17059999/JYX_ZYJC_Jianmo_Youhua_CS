#############################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##############################################################################
param(
[string]$IsWorkingDirUpdateRequired = "TRUE"
)

function Combine-Object { 
    param( 
    $object1, 
    $object2 
    ) 
 
    trap { 
        $a = 1 
        continue 
    } 
    $propertylistObj1 = @($object1 | Get-Member -ea Stop -memberType *Property | Select-Object -ExpandProperty Name) 
    $propertylistObj2 = @($object2 | Get-Member -memberType *Property | Select-Object -ExpandProperty Name | Where-Object { $_ -notlike '__*'}) 
 
    $propertylistObj2 | ForEach-Object { 
        if ($propertyListObj1 -contains $_) { 
            $name = '_{0}' -f $_ 
        } else { 
            $name = $_ 
        } 
 
        $object1 = $object1 | Add-Member NoteProperty $name ($object2.$_) -PassThru 
    } 
 
    $object1 
}  
 
function Get-Drives { 
    Get-WmiObject Win32_DiskPartition | 
    ForEach-Object { 
        $partition = $_ 
        $logicaldisk = $partition.psbase.GetRelated('Win32_LogicalDisk') 
        if ($logicaldisk -ne $null) { 
            Combine-Object $logicaldisk $partition  
        } 
    } | select-Object Name, VolumeName, DiskIndex, Index, Size 
} 

if($IsWorkingDirUpdateRequired.ToLower().Trim().Equals("false"))
{
    exit 0
}

Push-Location $env:windir\system32

$DriveLetter = (Get-WmiObject win32_logicaldisk | where{$_.VolumeName -eq "IATUse"}).DeviceID
if(($DriveLetter -ne $null)-and($DriveLetter -ne ""))
{
    echo "$DriveLetter\WinteropProtocolTesting" > "C:\WorkingDir.txt"
    #wttcmd.exe /SysInitKey /Key:WorkingDir /Value:$DriveLetter\WinteropProtocolTesting
    Write-Host $DriveLetter -ForegroundColor Yellow
    exit 0
}
 
$DriveLetter = (Get-Drives | where{($_.DiskIndex -eq 1)-and($_.Index -eq 0)-and($_.Size -gt 150*1024*1024*1024)}).Name
if(($DriveLetter -ne $null)-and($DriveLetter -ne ""))
{
    echo "$DriveLetter\WinteropProtocolTesting" > "C:\WorkingDir.txt"
    #wttcmd.exe /SysInitKey /Key:WorkingDir /Value:$DriveLetter\WinteropProtocolTesting
    Write-Host $DriveLetter -ForegroundColor Yellow
    exit 0
}

$Drives = Get-Drives | where{$_.Size -gt 150*1024*1024*1024}
$DriveLetter = $Drives[0].Name
if(($DriveLetter -ne $null)-and($DriveLetter -ne ""))
{
    echo "$DriveLetter\WinteropProtocolTesting" > "C:\WorkingDir.txt"
    #wttcmd.exe /SysInitKey /Key:WorkingDir /Value:$DriveLetter\WinteropProtocolTesting
    Write-Host $DriveLetter -ForegroundColor Yellow
    exit 0
}

Pop-Location