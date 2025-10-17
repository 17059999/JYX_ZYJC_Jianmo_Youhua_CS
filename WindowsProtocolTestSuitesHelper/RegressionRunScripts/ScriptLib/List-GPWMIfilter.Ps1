#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:            List-GPWMIFilter.Ps1
## Purpose:         List WMI filter properties in GPMC 
## Version:         1.0 (Dec 23 2008)
##
########################################################################################################################
Param (
$Domainname,
$WMIFILTER,
$Output
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [List-GPWMIFilter.ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
Function Show-ScriptUsage
{    
    Write-host    
    Write-host "Usage:                       `t:This will List WMI filter properties in GPMC"
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First Parameter              `t:DomainName : Domain name "
    Write-host "Second Parameter             `t:WMIFILTER : WMI filter display name for which to retrive properties"
    Write-host "Second Parameter             `t:output : property name you want to retrive from specified WMIFilter"
    Write-host "Example1"
    Write-host "This example is used to retrive WMI filter creationdate"
    Write-host ".\List-GPWMIfilter contoso.com vistafilter msWMI-CreationDate"
    Write-host "Example2"
    Write-host "This example is used to retrive WMI filter Changedate"
    Write-host ".\List-GPWMIfilter contoso.com vistafilter msWMI-ChangeDate"
    Write-host "Example3"
    Write-host "This example is used to retrive WMI filter Author name"
    Write-host ".\List-GPWMIfilter contoso.com vistafilter msWMI-Author"
    Write-host "Example4"
    Write-host "This example is used to retrive WMI filter Description"
    Write-host ".\List-GPWMIfilter contoso.com vistafilter MSWMI-Parm1"
    Write-host
}

#----------------------------------------------------------------------------
#Verify Help parameters
#----------------------------------------------------------------------------
If ($args[0] -match '-(\?|(h|(help)))')
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
$LDAP =[ADSI]""
$DomainDN = $LDAP.distinguishedName

#----------------------------------------------------------------------------
#Search criteria for WMI filtername
#----------------------------------------------------------------------------
$searcher = $gpm.CreateSearchCriteria()
$GPOlist = $domain.SearchWMIFilters($searcher)

If ($GPOlist.count -eq 0)
{
   write-host "WMI FILTER does not exsits  in this domain" -foregroundcolor red
}
else
{

$WMIStatus = $False
foreach($WMI in $gpolist)
{
    If($WMI.name -eq $WMIFILTER)
    {
    $Filter = $WMI.path
    $WMIStatus = $True
    }
}
If ($WMIStatus -eq "True")
    {
    $Splitfilter = $Filter.Split("{")[1]
    $WMIFILTERGUID = $Splitfilter.Split("}")[0]
    $path = "CN={$WMIFILTERGUID},CN=SOM,CN=WMIPolicy,CN=System,$DomainDN"
    $DN=[ADSI]"LDAP://$Path"

#----------------------------------------------------------------------------
#List specified WMI filter property
#----------------------------------------------------------------------------

If ($output -eq "FilterGUID")
    {
    return $DN.cn
    }
elseIf ($output -eq "FilterDN")
    {
    return $DN.distinguishedName
    }
elseIf ($output -eq "msWMI-Author")
    {
    return $DN."msWMI-Author"
    }
elseIf ($output -eq "msWMI-ChangeDate")
    {
    return $DN."msWMI-ChangeDate"
    }
elseIf ($output -eq "msWMI-CreationDate")
    {
    return $DN."msWMI-CreationDate"
    }
elseIf ($output -eq "msWMI-ID")
    {
    return $DN."msWMI-ID"
    }
elseIf ($output -eq "msWMI-Name")
    {
    return $DN."msWMI-Name"
    }
elseIf ($output -eq "msWMI-Parm1")
    {
    return $DN."msWMI-Parm1"
    }
elseIf ($output -eq "msWMI-Parm2")
    {
    return $DN."msWMI-Parm2"
    }
elseIf ($output -eq "objectCategory")
    {
    return $DN.objectCategory
    }
elseIf ($output -eq "objectGUID")
    {
    return $DN.objectGUID
    }
else
    {
    write-host "Specified WMI filter property is not supported by this powershell script.. " -foregroundcolor Red
    }
    }
else
    {
    write-host "Specified $WMIFIlter does not exist in specified $domainname" -foregroundcolor Red
    }
}

#----------------------------------------------------------------------------
# Print exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [List-GPWMIfilter] SUCCEED." -foregroundcolor Green



