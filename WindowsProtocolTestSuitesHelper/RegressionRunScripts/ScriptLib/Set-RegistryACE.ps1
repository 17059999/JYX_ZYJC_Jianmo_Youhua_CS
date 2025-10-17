#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:            Set-RegistryACE.Ps1
## Purpose:         Set Access control entry /permissions for registry file
## Version:         1.0 
##
########################################################################################################################

Param (
$object,
[string]$identity,
[string]$accessmask,
[string]$type
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-RegistryACE.Ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
  
function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage:         This PS1 script will Set Access control entry /permissions for registry file."
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "Parameter one:    OBJect     : Target object(eg: HKLM:\SYSTEM\CurrentControlSet\Services\Eventlog)"
    Write-host "Parameter two:    Identity   : Username (eg: Testuser )"
    Write-host "Parameter three:  AccessMask : Accessmask for the object (Eg: Readkey,writekey)"
    Write-host "Parameter four:   Type       : Accesstype(Eg: allow ,Deny)"  
    Write-host "Example:"
    Write-host "Set_Registry_AccessControlEntry.Ps1 HKLM:\SYSTEM\CurrentControlSet\Services\Eventlog testuser readkey allow"
    Write-host
}

#----------------------------------------------------------------------------
#Verify Help parameters
#----------------------------------------------------------------------------

if($args[0] -match '-(\?|(h|(help)))')
{
    write-host 
    write-host
    show-scriptusage 
    return
}

#----------------------------------------------------------------------------
#Check for input parameters
#-----------------------------------------------------------------------------

if(!$object)
{
    Write-host
    Write-host "Please specify the Object !" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

if(!$identity)
{
    Write-host
    Write-host "Please specify the Identity(username)!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

if(!$accessmask)
{
    Write-host
    Write-host "Please specify the Access mask!" -foregroundcolor Red
    Write-Host
    Show-ScriptUsage
    return
}

if(!$Type)
{
    Write-host
    Write-host "Please specify the Access trype(Allow/Deny)!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}

#--------------------------------------------------------------------------
# Setting the access control entry for the registry file
#--------------------------------------------------------------------------

$inflag =[system.security.accesscontrol.inheritanceflags]"containerinherit,objectinherit"

$propagationflags =[system.security.accesscontrol.propagationflags]"none"

$sd = get-acl $object

$rule= new-object system.security.accesscontrol.registryaccessrule([system.security.principal.ntaccount]$identity,[system.security.accesscontrol.registryrights]$accessmask,$inflag,$propagationflags,[system.security.accesscontrol.accesscontroltype]$type)

$sd.AddAccessRule($rule)

set-acl $object $sd
#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------

Write-Host "EXECUTE [Set-RegistryACE.Ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

Exit 0