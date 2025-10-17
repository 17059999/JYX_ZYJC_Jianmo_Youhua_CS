#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:            Retrive-GPODNID.PS1
## Purpose:         Retrive GPO ID 
## Version:         1.0 (Dec 23 2008)
##
########################################################################################################################
Param (
$GPOName,
$DomainName,
$Output
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Retrive-GPODNID.PS1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
Function Show-ScriptUsage
{    
    Write-host    
    Write-host "Usage:                       `t:This will Retrive GPO ID DN in GPMC"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First Parameter             `t:$GPOName : GPO display name  "
    Write-host "Second Parameter            `t:$DomainName : Domain name ,where GPO exists"
    Write-host "Third Parameter             `t:output : property name you want to retrive from specified GPO Name"
    Write-host "Example1"
    Write-host "This example is used to retrive SOM_GPO_1 GPO ID/common name "
    Write-host ".\Retrive-GPODNID.ps1 SOM_GPO_1 $domainInVM GPOID"
    Write-host "Example2"
    Write-host "This example is used to retrive SOM_GPO_2 GPO Distinguished name "
    Write-host ".\Retrive-GPODNID.ps1 SOM_GPO_2 $domainInVM GPODN"    
}

#----------------------------------------------------------------------------
#Verify Help parameters
#----------------------------------------------------------------------------
if ($args[0] -match '-(\?|(h|(help)))')
{
    write-host 
    write-host
    show-scriptusage 
    return
}

#----------------------------------------------------------------------------
#Connecting to group policy management console
#----------------------------------------------------------------------------
$GPM  = New-Object -com gpmgmt.gpm

#----------------------------------------------------------------------------
#Connecting to specified Domain 
#----------------------------------------------------------------------------
$Domain = $gpm.GetDomain($domainname, $null,$gpm.GetConstants().UseAnyDC )

#----------------------------------------------------------------------------
#Search criteria (vaildation)
#-----------------------------------------------------------------------------
$Searcher = $GPM.CreateSearchCriteria()
$Searcher.Add( $gpm.GetConstants().SearchPropertyGPODisplayName,$gpm.GetConstants().SearchOpEquals,$GPOName)
$GPOList = $Domain.SearchGPOs( $searcher )
If ($GPOList.count -eq 0 )
    {
    write-host "Specified GPO name does not exists in $DomainName  " -foregroundcolor Red
    }
else
{
If ($output -eq "GPOID")
    {
    foreach ($ID in $GPOList)
    {
    $GPOID = $ID.ID
    return $GPOID
    }
    }
elseif ($output -eq "GPODN")
    {
    foreach ($ID in $GPOList)
    {
    $Path = $ID.Path
    $DN=[ADSI]"LDAP://$Path"
    return $DN.distinguishedName
    }
    }
else 
    {
     write-host "Specified output value is not supported by this PS1 script.. " -foregroundcolor Red
    }
}


#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Retrive-GPODNID.PS1] SUCCEED." -foregroundcolor Green
