#########################################################################################################################
##
## Microsoft Windows Powershell Scripting
## File:            Set-FileSystemACE.Ps1
## Purpose:         Set Access control entry /permissions for File System (file or folder)
## Version:         1.0 
##
########################################################################################################################

Param (
 $Object,
[string]$Identity,
[string]$AccessMask,
[string]$Type
)

#----------------------------------------------------------------------------
# Print execution information
#----------------------------------------------------------------------------
Write-Host "EXECUTING [Set-FileSystemACE.Ps1]..." -foregroundcolor cyan

#----------------------------------------------------------------------------
#Function: Show-ScriptUsage
#Usage   : Describes the usage information and options
#----------------------------------------------------------------------------
Function Show-ScriptUsage
{    
    Write-host 
    Write-host "Usage:                       `t:This PS1 script will Set Access control entry /permissions for registry file."
    Write-host
    Write-host "Options:"
    Write-host
    Write-host "First  Parameter             `t:OBJect     : Target object(eg: HKLM:\SYSTEM\CurrentControlSet\Services\Eventlog)"
    Write-host "Second Parameter             `t:Identity   : Username (eg: Testuser )"
    Write-host "Third Parameter              `t:AccessMask : Accessmask for the object (Eg: Readkey,writekey)"
    Write-host "Fourth Parameter             `t:Type       : Accesstype(Eg: allow ,Deny)"  
    Write-host "Example:"
    Write-host "Set-FileSystemACE.Ps1 HKLM:\SYSTEM\CurrentControlSet\Services\Eventlog testuser readkey allow"
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
if(!$Object)
{
    Write-host
    Write-host "Please specify the Object !" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}
if(!$Identity)
{
    Write-host
    Write-host "Please specify the Identity(username)!" -foregroundcolor Red
    Write-host 
    Show-ScriptUsage
    return
}
if(!$AccessMask)
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
$Inflag =[system.security.accesscontrol.inheritanceflags]"containerinherit,objectinherit"
$PropagationFlags =[system.security.accesscontrol.propagationflags]"none"
$SD = get-acl $object
$Rule= new-object system.security.accesscontrol.filesystemaccessrule([system.security.principal.ntaccount]$identity,[system.security.accesscontrol.filesystemrights]$accessmask,$inflag,$propagationflags,[system.security.accesscontrol.accesscontroltype]$type)
$SD.AddAccessRule($rule)
set-acl $object $SD

#----------------------------------------------------------------------------
# Print Exit information
#----------------------------------------------------------------------------
Write-Host "EXECUTE [Set-FileSystemACE.Ps1] FINISHED (NOT VERIFIED)." -foregroundcolor yellow

Exit 0
