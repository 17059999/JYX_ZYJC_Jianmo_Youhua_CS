#-----------------------------------------------------------------------------
# Enable Domain Recycle Bin Feature
#-----------------------------------------------------------------------------
Param
(
    [string]$domainName,
    [string]$ldsInstanceGUID,
    [string]$ldsLdapPort
)

if($domainName -ne $null)
{
    Write-Host "Enabling Domain Recycle Bin Feature for AD DS..." -ForegroundColor Yellow
    $domainNC = "DC=" + $domainName.Replace(".", ",DC=")
    try
    {
        $OptionalFeatureDN = "CN=Recycle Bin Feature,CN=Optional Features,CN=Directory Service,CN=Windows NT,CN=Services,CN=Configuration," + $domainNC
        Enable-ADOptionalFeature -Identity $OptionalFeatureDN `
                                 -Scope ForestOrConfigurationSet `
                                 -Target $domainName `
                                 -Confirm:$false 2>&1 | Write-Host
    }
    catch
    {
       Write-Host $_.Exception -ForegroundColor Red
    }
}

if($ldsInstanceGUID -ne $null)
{
    Write-Host "Enabling Domain Recycle Bin Feature for AD LDS..." -ForegroundColor Yellow

    if(($ldsldapport -eq $null) -or ($ldsldapport -eq ""))
    {
        Throw "AD LDS LDAP port is not provided."
    }

    try
    {
        Enable-ADOptionalFeature 'recycle bin feature' `
                                 -Scope ForestOrConfigurationSet `
                                 -Server localhost:$ldsldapport `
                                 -Target "CN=Configuration,CN={$ldsInstanceGUID}" `
                                 -Confirm:$false 2>&1 | Write-Host
    }
    catch
    {
       Write-Host $_.Exception -ForegroundColor Red
    }
}