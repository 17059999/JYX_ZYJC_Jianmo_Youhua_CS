#########################################################################################################################
## 
## Microsoft Windows Powershell Scripting
## File:            Create-GPO.Ps1
## Purpose:         Create Group policy object
## Version:         1.0 (July 14th 2008)
##
########################################################################################################################

Param (
$GPOName,
$DomainName
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Create-GPO.ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
Function Show-ScriptUsage
{    
    Write-host    
    Write-host "Usage:                       `t:This PS1 script will Create group policy object in GPMC"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First  Parameter             `t:GPOname:Group policy name"
    Write-host "Second Parameter             `t:DomainName : Domain name for which GPO is created"
    Write-host "Example:"
    Write-host "Create-GPO.Ps1 GPSI_GPO_1 contoso.com"
    Write-host
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
#Check for input parameters
#-----------------------------------------------------------------------------
if(!$GPOName)
{
    Write-host
    Write-host "Please specify the GPO name!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}
if(!$DomainName)
{
    Write-host
    Write-host "Please specify the target Domain name!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

#----------------------------------------------------------------------------
#Connecting to group policy management console
#-----------------------------------------------------------------------------
$GPM  = New-Object -com gpmgmt.gpm

#----------------------------------------------------------------------------
#Connecting to specified Domain
#-----------------------------------------------------------------------------
$Domain = $GPM.GetDomain($DomainName, $null,$gpm.GetConstants().UseAnyDC )

#----------------------------------------------------------------------------
#Search criteria (vaildation)
#-----------------------------------------------------------------------------
$Searcher = $GPM.CreateSearchCriteria()
$Searcher.Add( $gpm.GetConstants().SearchPropertyGPODisplayName,$gpm.GetConstants().SearchOpEquals,$GPOName)
$GPOList = $Domain.SearchGPOs( $searcher )
If ($GPOList.count -ne 0 )
{
    write-host "GPO with $GPOName already exists " -foregroundcolor Red
}

#----------------------------------------------------------------------------
# Create GPO 
#-----------------------------------------------------------------------------
If ($GPOList.count -eq 0)
{
    $GPO = $Domain.CreateGPO()
    $GPO.DisplayName = $GPOname
    write-host "$GPOName GPO is created sucessfully" -foregroundcolor green
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Create-GPO.ps1] SUCCEED." -foregroundcolor Green

   
  
