The folder /home/test/ptfconfig includes the ptfconfig files and SSH private key named "id_rsa" which will be mounted to the docker image, and the generated test result (.trx) will also be copied to this path.

#Build the image
Build FileServer-TestSuite-ServerEP.tar.gz, PTMCli.tar.gz and PTMService.tar.gz from the source or get them from the release page.
Copy FileServer-TestSuite-ServerEP.tar.gz, PTMCli.tar.gz and PTMService.tar.gz to the folder where the dockerfile locates.
Copy the files in WindowsProtocolTestSuitesHelper/DockerFiles/Common to the folder where the dockerfile locates.
Use "sudo docker build -t xxx:version ." to build a local docker image via the dockerfile.

#Run the image
# Usage: This value can be specified as "RunTestCases", "RunPTMCli" or "RunPTMService". 
A test run will start if the Usage is set to "RunTestCases".
A test run initiated by PTMCli will start if the Usage is set to "RunPTMCli".
A PTMService instance will be deployed if the Usage is set to "RunPTMService".

The following variables can be specified if the Usage is set to "RunTestCases":
sudo docker run --hostname FS-CLI --network host --env Usage="RunTestCases" --env SutComputerName="node01.contoso.com" --env SutIPAddress="192.168.1.11" --env DomainName="contoso.com" --env AdminUserName="Administrator" --env PasswordForAllUsers="XXXX" --env Filter="TestCategory=BVT&TestCategory=SMB311" --env DryRun="" -v /home/test/ptfconfig:/data/fileserver windowsprotocoltestsuites:fileserver
# SutComputerName: Computer name of system under test (SUT). If SUT does not have a computer name, set the value to SUT's IP address.
# SutIPAddress: IP address or Host Name of SUT to establish connections.
# DomainName: Domain name where the SUT locates. If SUT is in WORKGROUP, set it to the value of SutComputerName. If SUT does not have a computer name, leave it blank.
# AdminUserName: Administrator user account name of the SUT.
# PasswordForAllUsers: Password for all the users listed as follows: AdminUserName, NonAdminUserName and GuestUserName. (To simplify the config, the 3 accounts use the same password.)
# Filter: Expression used to filter test cases. For example, "TestCategory=BVT&TestCategory=SMB311" will filter out test cases which have test category BVT and SMB311. 
# DryRun: If set as "y", just list all filtered test cases instead of running tests actually. Else if it's null or empty, the filtered test cases will be executed directly.

The following variables can be specified if the Usage is set to "RunPTMCli":
sudo docker run --hostname FS-CLI --network host --env Usage="RunPTMCli" --env Profile="FileServerBasic.ptm" --env Selected="false" --env Filter="TestCategory=BVT" --env Config="\"Common.SutComputerName=node01.contoso.com\" \"Common.SutIPAddress=192.168.1.11\"" --env ReportFormat="Plain" -v $(pwd):/data/fileserver windowsprotocoltestsuites:fileserver
# Profile: The file name of the PTM profile archive in the current directory on the host.
# Selected: When specified as "true", only the selected test cases will be executed. Otherwise, all the test cases in the profile will be executed.
# Filter: Specifies the filter expression of test cases to run. This parameter overrides the test cases in profile.
# Config: Specifies the configuration items which will override the values in profile. Each configuration should be in format {property_name}={property_value}, and multiple items should be separated by whitespace.
# ReportFormat: Specifies the report format. Valid values are: plain, json, xunit.

The following variables can be specified if the Usage is set to "RunPTMService":
sudo docker run --hostname FS-CLI --network host --env Usage="RunPTMService" --env HttpPort="80" --env HttpsPort="443" -v /home/test/ptfconfig:/data/fileserver -i windowsprotocoltestsuites:fileserver
# -i: Optional. The option can be specified if the user wants to interact with the PTMService instance running in the container.
# HttpPort: The HTTP port of the PTMService.
# HttpsPort: The HTTPS port of the PTMService.