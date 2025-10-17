#-----------------------------------------------------------------------------
# Script  : AddLocalIntranetSiteAndSetAutoLogon
# Usage   : Add the specified site to IE local intranet zone. And set 
#           automatic logon with current username and password to the zone.
#           So that when browsing the specified site, no credential dialog
#           will pop up.
# Params  : -Site <string>: The name of the site.
#-----------------------------------------------------------------------------
Param 
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$Site
) 


try
{
    $Mainkey = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains\' 
    New-Item -Path ($Mainkey + $Site) -Force -ErrorAction Stop 
    New-ItemProperty -Path ($Mainkey + $Site) -Name * -Value 1 -PropertyType DWORD -ErrorAction Stop

    # 1 represents local intranet zone
    $LocalIntranetZone = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings\Zones\1\'
    # Setting 1A00: User Authentication: Logon
    # 0x00000000: Automatically logon with current username and password
    Set-ItemProperty -Path $LocalIntranetZone -Name 1A00 -Value 0x00000000 -Force -ErrorAction Stop
}
catch
{
    throw "Error happened: " + $_.Exception.Message
}


