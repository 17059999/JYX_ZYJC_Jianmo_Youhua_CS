Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$TargetForestName,

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$TargetAdminUserName,

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$TargetAdminPassword
)


$TargetForestContext = New-Object System.DirectoryServices.ActiveDirectory.DirectoryContext `
    -ArgumentList 'Forest',$TargetForestName, $TargetAdminUserName, $TargetAdminPassword
try
{
    # Try to get the target forest
    $TargetForest = [System.DirectoryServices.ActiveDirectory.Forest]::GetForest($TargetForestContext)
}
catch
{
    throw "Unable to get target forest. Error: " + $_.Exception.Message
}

$LocalForest = [System.DirectoryServices.ActiveDirectory.Forest]::GetCurrentForest()
#$LocalForest.CreateTrustRelationship($TargetForest, "Bidirectional")

try
{
    # Build trust relationship
    $LocalForest.CreateTrustRelationship($TargetForest, "Bidirectional")
}
# If trust relationship already exists
catch [System.DirectoryServices.ActiveDirectory.ActiveDirectoryObjectExistsException]
{
    Write-Host "Trust relationship already exists."
}
catch
{
    throw "Failed to create trust relationship. Error: " + $_.Exception.Message
}
