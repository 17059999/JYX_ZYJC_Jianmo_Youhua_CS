Import-Module ServerManager
try{

    Add-WindowsFeature RemoteAccess -IncludeAllSubFeature -IncludeManagementTools
    CMD /C NETSH ras set type lanonly lanonly IPv4
    CMD /C NETSH ras set conf ENABLED
    CMD /C NET stop RemoteAccess
    CMD /C SC config RemoteAccess start=Auto
    CMD /C NET start RemoteAccess 
}catch
{
    Write-Warning "Enable Remote Access Failed"
}