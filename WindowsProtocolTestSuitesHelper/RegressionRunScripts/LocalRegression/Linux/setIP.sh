if [ -f /cron/started -a -f /mnt/ip.txt ]; then
  ip_address=`cat /cron/ip.txt|head -1`
  ip_subnet=`cat /cron/ip.txt|head -2|tail -1`
  ip_gateway=`cat /cron/ip.txt|head -3|tail -1`
  ip_dns=`cat /cron/ip.txt|tail -1`
  echo "network:" > /etc/netplan/01-network-manager-all.yaml
  echo "    ethernets:" >> /etc/netplan/01-network-manager-all.yaml
  echo "        eth1:" >> /etc/netplan/01-network-manager-all.yaml
  echo "            addresses: [$ip_address/24]" >> /etc/netplan/01-network-manager-all.yaml
  echo "            dhcp4: no" >> /etc/netplan/01-network-manager-all.yaml
  echo "            gateway4: $ip_gateway" >> /etc/netplan/01-network-manager-all.yaml
  echo "            nameservers:" >> /etc/netplan/01-network-manager-all.yaml
  echo "                addresses: [$ip_dns]" >> /etc/netplan/01-network-manager-all.yaml
  echo "                search: []" >> /etc/netplan/01-network-manager-all.yaml
  echo "            optional: true" >> /etc/netplan/01-network-manager-all.yaml
  echo "" >> /etc/netplan/01-network-manager-all.yaml
  echo "    version: 2" >> /etc/netplan/01-network-manager-all.yaml
  echo "    renderer: NetworkManager" >> /etc/netplan/01-network-manager-all.yaml
  netplan apply
fi