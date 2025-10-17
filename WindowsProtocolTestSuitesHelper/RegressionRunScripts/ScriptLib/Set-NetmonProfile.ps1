#-------------------------------------------------------------------------------
# Function: Set-NetmonProfile
# Usage   : Set Network Monitor Profile to ProfileName
#-------------------------------------------------------------------------------
Param
(
    [parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$ProfileName = "Windows"
)

[string[]]$Profiles = cmd /c "nmcap /DisplayProfiles"
[string]$res = $Profiles | Select-String $ProfileName
[string]$ProfileKey = $res.Substring($res.IndexOf("Parse")+6)
cmd /c "nmcap /SetActiveProfile $ProfileKey"
