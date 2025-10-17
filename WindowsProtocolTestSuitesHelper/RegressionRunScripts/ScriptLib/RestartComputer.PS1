# Restart Computer
if($IsLinux) {
    & /usr/bin/env sudo reboot    
}else {
    Restart-Computer -Force;
}