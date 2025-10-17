#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:            Link-GPOSite.Ps1
## Purpose:         Link Group policy object to a Site
## Version:         1.0 (Dec 23 2008)
##
########################################################################################################################
Param (
$GPOname,
$GPOlinkPos,
$DomainName,
$Sitename,
$Forestname,
$Enabled,
$Enforced
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Link-GPOSite.ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
Function Show-ScriptUsage
{    
    Write-host    
    Write-host "Usage:                       `t:This PS1 script will Link Group policy object to a Site"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First  Parameter             `t:GPOname:Group policy name"
    Write-host "Second Parameter             `t:GPOlinkPos : link position of a GPO to site"
    Write-host "Third Parameter              `t:DomainName : Domain name for which GPO is created"
    Write-host "Fourth Parameter             `t:sitename : Site name for which GPO to be linked"
    Write-host "Fifth Parameter              `t:forestname : forest name of the domain"
    Write-host "Sixth Parameter              `t:enabled : GPO Link enabled to the site "
    Write-host "Seventh Parameter            `t:enforced : GPO Link enforced to the site"
    Write-host "Example:"
    Write-host ".\Link-GPOSite.ps1 GPOL_som1 1 contoso.com gpolsite contoso.com 1 1"
    Write-host
}

#----------------------------------------------------------------------------
#Verify Help parameters
#----------------------------------------------------------------------------
If($args[0] -match '-(\?|(h|(help)))')
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
#Connecting to specified Domain & get the site name
#----------------------------------------------------------------------------
$Domain = $GPM.GetDomain($DomainName, $null,$gpm.GetConstants().UseAnyDC )
$site= $gpm.GetSitesContainer($forestname,$domainname,"","")
$sitelink=$site.getsite($sitename)
$GpoSite =$sitelink.getgpolinks()
$Count = $GpoSite.count

#----------------------------------------------------------------------------
#Search criteria (vaildation)
#----------------------------------------------------------------------------
$Searcher = $GPM.CreateSearchCriteria()
$Searcher.Add( $gpm.GetConstants().SearchPropertyGPODisplayName,$gpm.GetConstants().SearchOpEquals,$GPOName)
$GPOList = $Domain.SearchGPOs( $searcher )
If($GPOList.count -eq 0 )
{
   write-host "GPO with $GPOName does not exists in the $domainname" -foregroundcolor Red
}

else
{
#----------------------------------------------------------------------------
#Create GPO link
#-----------------------------------------------------------------------------
$GPMlink = $Sitelink.CreateGPOLink($GPOlinkPos, $GPOlist.Item(1))
$GPMlink.enabled = $enabled
$GPMlink.enforced = $enforced

#----------------------------------------------------------------------------
#Validate the GPO link to site
#-----------------------------------------------------------------------------
$GpoSite =$sitelink.getgpolinks()
$LinkCount = $GpoSite.count
If($linkcount -gt $count)
{
    write-host "$GPOName Linked to $Sitename Sucessfully " -foregroundcolor Green
}
else
{
    write-host "Check whether $GPOName is already Linked to $Sitename..FAILURE.. " -foregroundcolor Red
}
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [LinkGPO-Site.ps1] SUCCEED." -foregroundcolor Green