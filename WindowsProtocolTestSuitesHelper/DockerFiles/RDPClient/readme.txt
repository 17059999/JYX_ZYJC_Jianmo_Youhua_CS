The folder /home/test/ptfconfig includes the ptfconfig files and SSH private key named "id_rsa" which will be mounted to the docker image, and the generated test result (.trx) will also be copied to this path.

#Build the image
Build RDP-TestSuite-ClientEP.tar.gz, PTMCli.tar.gz and PTMService.tar.gz from the source or get them from the release page.
Copy RDP-TestSuite-ClientEP.tar.gz, PTMCli.tar.gz and PTMService.tar.gz to the folder where the dockerfile is located.
Copy the files in WindowsProtocolTestSuitesHelper/DockerFiles/Common to the folder where the dockerfile is located.
Use "sudo docker build -t xxx:version ." to build a local docker image via the dockerfile.

#Run the image
# Usage: This value can be specified as "RunTestCases", "RunPTMCli" or "RunPTMService". 
A test run will start if the Usage is set to "RunTestCases".
A test run initiated by PTMCli will start if the Usage is set to "RunPTMCli".
A PTMService instance will be deployed if the Usage is set to "RunPTMService".

The following variables can be specified if the Usage is set to "RunTestCases":
sudo docker run --hostname FS-CLI --network host --env Usage="RunTestCases" --env SutName="node01.contoso.com" --env SutUserName="Administrator" --env SutUserPassword="XXXX" --env ServerIPAddress="192.168.1.12" --env ServerUserName="Administrator" --env ServerUserPassword="XXXX" --env Filter="TestCategory=BVT&TestCategory=SMB311" --env DryRun="" -v /home/test/ptfconfig:/data/rdpclient windowsprotocoltestsuites:rdpclient
# SutName: Computer name of system under test (SUT). If SUT does not have a computer name, set the value to SUT's IP address.
# SutUserName: The User Id of SUT which should have administrator privileges.
# SutUserPassword: The logon password of SUTUserName.
# ServerIPAddress: IP address the localhost (RDP server).
# ServerUserName: The user name of local host (RDP server).
# ServerUserPassword: The logon password of "ServerUserName".
# Filter: Expression used to filter test cases. For example, "TestCategory=BVT&TestCategory=SMB311" will filter out test cases which have test category BVT and SMB311. 
# DryRun: If set as "y", just list all filtered test cases instead of running tests actually. Else if it's null or empty, the filtered test cases will be executed directly.

The following variables can be specified if the Usage is set to "RunPTMCli":
sudo docker run --hostname FS-CLI --network host --env Usage="RunPTMCli" --env Profile="FileServerBasic.ptm" --env Selected="false" --env Filter="TestCategory=BVT" --env Config="\"RDP.ServerPort=3389\" \"RDP.Security.Protocol=TLS\"" --env ReportFormat="Plain" --env ServerIPAddress="192.168.1.12" -v $(pwd):/data/rdpclient windowsprotocoltestsuites:rdpclient
# Profile: The file name of the PTM profile archive in the current directory on the host.
# Selected: When specified as "true", only the selected test cases will be executed. Otherwise, all the test cases in the profile will be executed.
# Filter: Specifies the filter expression of test cases to run. This parameter overrides the test cases in profile.
# Config: Specifies the configuration items which will override the values in profile. Each configuration should be in format {property_name}={property_value}, and multiple items should be separated by whitespace.
# ReportFormat: Specifies the report format. Valid values are: plain, json, xunit.
# ServerIPAddress: IP address the localhost (RDP server).

The following variables can be specified if the Usage is set to "RunPTMService":
sudo docker run --hostname FS-CLI --network host --env Usage="RunPTMService" --env HttpPort="80" --env HttpsPort="443" --env ServerIPAddress="192.168.1.12" -v /home/test/ptfconfig:/data/rdpclient -i windowsprotocoltestsuites:rdpclient
# -i: Optional. The option can be specified if the user wants to interact with the PTMService instance running in the container.
# HttpPort: The HTTP port of the PTMService.
# HttpsPort: The HTTPS port of the PTMService.
# ServerIPAddress: IP address the localhost (RDP server).