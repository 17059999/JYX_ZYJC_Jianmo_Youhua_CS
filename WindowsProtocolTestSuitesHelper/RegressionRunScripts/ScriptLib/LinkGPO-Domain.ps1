#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:            LinkGPO-Domain.Ps1
## Purpose:         This PS1 script will Link GPO to domain
## Version:         1.0 (July 14th 2008)
##
########################################################################################################################

Param ($GPOname,$DomainName,$Enabled,$Enforced)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [LinkGPO-Domain.ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#
#Function: Show-ScriptUsage
#
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
  
function Show-ScriptUsage
{    
    Write-host    
    Write-host "Usage:		             `t:This PS1 script will Link GPO to domain in GPMC"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First  Parameter             `t:GPOname:Group policy name"
    Write-host "Second Parameter             `t:DomainName : Domain name for which GPO is created"
    Write-host "Third  Parameter             `t:Enabled: Group policy enabled /disabled (True/false)"
    Write-host "Fourth Parameter             `t:Enforced : Group policy object enforced (True/false)"
    Write-host "Example:"
    Write-host "LinkGPO-Domain.ps1 GPSI_GPO_1 contoso.com true true"
    Write-host
}

#----------------------------------------------------------------------------
#
#Verify Help parameters
#
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

if(!$GPOname)
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

if(!$Enabled)
{
    Write-host
    Write-host "Please specify the GPO Enabled status (TRUE/FALSE)!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

if(!$Enforced)
{
    Write-host
    Write-host "Please specify the GPO Enforced status (TRUE/FALSE)!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

#----------------------------------------------------------------------------
#Connecting to group policy management console
#-----------------------------------------------------------------------------

$Root = [adsi]""
$DN =$Root.Distinguishedname
$GPM  = New-Object -com gpmgmt.gpm

#----------------------------------------------------------------------------
#Connecting to specified Domain
#-----------------------------------------------------------------------------
$Domain = $gpm.GetDomain($domainname, $null,$gpm.GetConstants().UseAnyDC )

#----------------------------------------------------------------------------
#Search criteria (vaildation)
#-----------------------------------------------------------------------------

$searcher = $gpm.CreateSearchCriteria()
$searcher.Add( $gpm.GetConstants().SearchPropertyGPODisplayName,$gpm.GetConstants().SearchOpEquals,$GPOname)
$GPOlist = $Domain.SearchGPOs( $searcher )

#----------------------------------------------------------------------------
# Create GPOLINK 
#-----------------------------------------------------------------------------

$SOM = $Domain.GetSOM($DN)
$GPMlink = $SOM.CreateGPOLink( "-1", $GPOlist.Item(1))

#----------------------------------------------------------------------------
# GPOLINK Enabled & Enforced Status
#-----------------------------------------------------------------------------

$GPMlink.enabled = $Enabled
$GPMlink.enforced = $Enforced

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [LinkGPO-Domain.ps1] SUCCEED." -foregroundcolor Green



