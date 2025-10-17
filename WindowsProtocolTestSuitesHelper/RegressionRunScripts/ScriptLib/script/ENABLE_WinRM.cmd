call winrm qc -q

call winrm set winrm/config/winrs @{AllowRemoteShellAccess="true"}
call winrm set winrm/config/winrs @{MaxMemoryPerShellMB="2048"}
call winrm set winrm/config/service/auth @{CredSSP="true"}
call winrm set winrm/config/service @{IPv4Filter="*"}
call winrm set winrm/config/service @{IPv6Filter="*"}

call winrm set winrm/config/client/auth @{CredSSP="True"}
call winrm set winrm/config/client @{TrustedHosts="*"}

netsh advfirewall firewall set rule group="Windows Remote Management" new enable=Yes 
netsh advfirewall firewall set rule group="Network discovery" new enable=Yes 