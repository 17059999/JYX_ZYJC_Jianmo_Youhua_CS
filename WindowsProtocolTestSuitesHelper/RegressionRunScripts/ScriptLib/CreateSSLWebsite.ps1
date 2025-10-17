#-----------------------------------------------------------------------------
# Script  : CreateSSLWebsite
# Usage   : Create a SSL website and bind the given certificate to it.
# Params  : -SiteName <String> : The name of the website.
#           -PhysicalPath <String> : The physical path of the website. 
#           i.e. The content of the website.
#           -CertName <String> : Specifies the certificate to be binded to the
#           SSL website. This argument can be either the subject name or the 
#           friendly name of the certificate.
#-----------------------------------------------------------------------------
Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$SiteName,

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$PhysicalPath,

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$CertName
)

Import-Module WebAdministration
Import-Module NetworkTransition

Try
{
    # Stop all website on the server
    # Sites with the same port will conflict
    Get-Website | Stop-Website

    # New SSL site on port 443
    # The website will start automatically if binding successfully 
    New-Website -Name $SiteName -PhysicalPath $PhysicalPath -Port 443 -SSL -Force -ErrorAction Stop

    # Try to get the certificate
    # Parameter CertName can be either the subject name or the friendly name
    $Thumbprint = ( Get-ChildItem "Cert:\LocalMachine\My" `
                   | Where-Object {($_.Subject -like $CertName) -or ($_.FriendlyName -like $CertName)} `
                   | Select-Object -First 1 ).Thumbprint
    if($Thumbprint -eq $Null)
    {
        throw "No certificate with the given name was found."
    }
    $Cert = Get-Item "Cert:\LocalMachine\My\$Thumbprint" 
    $Cert | Write-Host

    # If a certificate has already been binded with any port,
    # Add-NetIPHttpsCertBinding will fail.
    # So remove all the SSL bindings first.
    Get-ChildItem "IIS:\SSLBindings" | Remove-Item
    # Bind the certificate to port 443
    # IIS always use the AppId as "4dc3e181-e14b-4a21-b022-59fc669b0914"
    Add-NetIPHttpsCertBinding -IpPort "0.0.0.0:443" -CertificateHash $Cert.GetCertHashString() `
        -CertificateStoreName "My" -ApplicationId "{4dc3e181-e14b-4a21-b022-59fc669b0914}" `
        -NullEncryption $false -ErrorAction Stop
}
Catch
{
    throw "Failed to create the SSL website. Error happened: " + $_.Exception.Message
}
