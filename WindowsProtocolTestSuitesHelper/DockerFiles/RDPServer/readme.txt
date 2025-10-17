The folder /home/test/ptfconfig includes the ptfconfig files which will be mounted to the docker image, and the generated test result (.trx) will also be copied to this path.

#Build the image
Build RDP-TestSuite-ServerEP.tar.gz from the source or get it from the release page.
Copy RDP-TestSuite-ServerEP.tar.gz to the folder where the dockerfile locates.
Use "sudo docker build -t xxx:version ." to build a local docker image via the dockerfile.

#Run the image
sudo docker run --hostname RDPClient --network host --env Filter="TestCategory=BVT" -env DryRun="" -v /home/test/ptfconfig:/data/rdpserver -i windowsprotocoltestsuites:rdpserver
# Filter: Expression used to filter test cases. For example, "TestCategory=BVT" will filter out test cases which have test category BVT. 
# DryRun: If set as "y", just list all filtered test cases instead of running tests actually. Else if it's null or empty, the filtered test cases will be executed directly.
