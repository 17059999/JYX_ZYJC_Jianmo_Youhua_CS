set basepath=%~dp0
set certrequest=%basepath%cert.inf
set certreq=%basepath%cert.req
set certreply=%basepath%cert.cer

rem cleanup
del /f /q %certrequest%
del /f /q %certreq%
del /f /q %certreply%

echo [Version] >  %certrequest%
echo Signature="$Windows NT$" >>  %certrequest%
echo [NewRequest] >>  %certrequest%
echo Subject="CN=%computername%.%userdnsdomain%"   >>  %certrequest%
echo Exportable=TRUE >>  %certrequest%
echo KeyLength=2048  >>  %certrequest%
echo KeySpec=1  >>  %certrequest%
echo KeyUsage=0xA0 >>  %certrequest%
echo MachineKeySet=True >>  %certrequest%
echo ProviderName="Microsoft RSA SChannel Cryptographic Provider" >>  %certrequest%
echo ProviderType=12 >>  %certrequest%
echo SMIME=FALSE >>  %certrequest%
echo RequestType=CMC >>  %certrequest%
echo [Strings] >>  %certrequest%
echo szOID_SUBJECT_ALT_NAME2 = "2.5.29.17" >>  %certrequest%
echo szOID_ENHANCED_KEY_USAGE = "2.5.29.37" >>  %certrequest%
echo szOID_PKIX_KP_SERVER_AUTH = "1.3.6.1.5.5.7.3.1" >>  %certrequest%
echo szOID_PKIX_KP_CLIENT_AUTH = "1.3.6.1.5.5.7.3.2">>  %certrequest%
echo [RequestAttributes] >>  %certrequest%
echo CertificateTemplate=WebServer >>  %certrequest%

certreq -new %certrequest% %certreq%
certreq -submit -config "%logonserver%.%userdnadomain%\%userdomain%RootCA" %certreq% %certreply%
certreq -accept %certreply%

$certificate = Get-ChildItem cert:\LocalMachine\MY | Where-Object {$_.Subject -match "CN=VM*"} 
$certificate.thumbprint