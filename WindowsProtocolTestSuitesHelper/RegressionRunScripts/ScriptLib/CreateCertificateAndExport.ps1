#-----------------------------------------------------------------------------
# Script  : CreateCertificateAndExport
# Usage   : Create a self signed certificate and export it to a given path.
# Params  : -DnsName <string> : Specifies the DNS name to put into 
#           the Subject Alternative Name extension of the certificate.
#           e.g. as.contoso.com
#           -FriendlyName <string>: The friendly name of the certificate.
#           e.g. MyCertificate
#           -ExportFullName <string>: The path and name of the exported 
#           certificate. Must be absolute path and the file extension must
#           be .cer. e.g. C:\Certificate.cer
#-----------------------------------------------------------------------------
Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$DNSName,

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$FriendlyName,

    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [ValidateScript({
        if(!($_ -match "^[a-zA-Z]:([\\].+)*([\\].+[\.]cer)$"))
        {
            throw "ExportFullName should be absolute path and the file extension must be .cer"
        }
        return $True
    })]
    [String]$ExportFullName
)

try
{
    # Create certificate
    $Cert = New-SelfSignedCertificate -CertStoreLocation "Cert:\LocalMachine\My" `
                -DnsName $DNSName -ErrorAction Stop
    $Cert.FriendlyName = $FriendlyName

    # Export 
    Export-Certificate -Cert $Cert -FilePath $ExportFullName -ErrorAction Stop

    # Import the certificate to trust list
    Import-Certificate -CertStoreLocation "Cert:\LocalMachine\Root" `
                -FilePath $ExportFullName -ErrorAction Stop
}
catch
{
    throw "Error happened while executing CreateCertificateAndExport.ps1. " + $_.Exception.Message
}
