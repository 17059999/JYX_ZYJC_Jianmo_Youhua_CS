The folder /home/test/ptfconfig includes the ptfconfig files which will be mounted to the docker image, and the generated test result (.trx) will also be copied to this path.

#Build the image
Build MS-WSP-TestSuite-ServerEP.tar.gz from the source or get it from the release page.
Copy MS-WSP-TestSuite-ServerEP.tar.gz to the folder where the dockerfile locates.
Use "sudo docker build -t xxx:version ." to build a local docker image via the dockerfile.

#Run the image
sudo docker run --hostname WSP-Client --network host --env ServerComputerName="SutComputer" --env UserName="Administrator" --env Password="XXXX" --env DomainName="contoso" --env ShareName="Test" --env QueryPath="file://SutComputer/Test/" --env QueryText="test" --env Filter="TestCategory=BVT" --env DryRun="y" -v /home/test/ptfconfig:/data/ms-wsp -i windowsprotocoltestsuites:ms-wsp
# ServerComputerName: IP/MachineName of the server under test.
# UserName: Name of the user to access the server under test.
# Password: Password of the user to access the server under test.
# DomainName: Name of the domain under test.
# ShareName: Name of the share on the server under test.
# QueryPath: It specifies the search scope (Path of the network folder to be searched). Please specify this value with computer name or IP address of the server.
# QueryText: It specifies the search query (Text to be present in name of searched files).
# Filter: Expression used to filter test cases. For example, "TestCategory=BVT" will filter out test cases which have test category BVT. 
# DryRun: If set as "y", just list all filtered test cases instead of running tests actually. Else if it's null or empty, the filtered test cases will be executed directly.
