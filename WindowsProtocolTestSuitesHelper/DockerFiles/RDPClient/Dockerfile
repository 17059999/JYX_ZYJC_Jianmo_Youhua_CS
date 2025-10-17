FROM mcr.microsoft.com/dotnet/sdk:8.0
# Environment variables with default values which could be overridden
ENV Usage=RunTestCases
# ENV Usage=RunPTMCli
# ENV Usage=RunPTMService
ENV SutName=node01.contoso.com
ENV SutUserName=Administrator
ENV SutUserPassword=Password01!
ENV ServerPort=3389
ENV ServerIPAddress=192.168.1.12
ENV ServerUserName=Administrator
ENV ServerUserPassword=Password01!
# By default, the container will list all the available test cases contained in the binaries
# You can override the Filter like "TestCategory=BVT"
ENV Filter=""
ENV DryRun="y"

ENV Profile=""
ENV Selected="false"
ENV Config=""
ENV ReportFormat="Plain"

ENV HttpPort=80
ENV HttpsPort=443
ENV TestSuiteName=RDPClient
ENV TestSuitePackageName=RDP-TestSuite-ClientEP.tar.gz

COPY ./RDP-TestSuite-ClientEP.tar.gz /
COPY ./PTMCli.tar.gz /
COPY ./PTMService.tar.gz /
COPY ./entrypoint.sh /
COPY ./runTestCases.ps1 /
COPY ./runPTMCli.ps1 /
COPY ./runPTMService.ps1 /
COPY ./updateHosts.ps1 /
COPY ./config /root/.ssh/
COPY ./configSshClient.ps1 /
RUN apt update && apt install -y openssh-server
# install these to cater for System.Drawing.Common being Windows specific
RUN apt install -y libc6-dev
RUN apt install -y libgdiplus

RUN mkdir -p /opt/rdpclient
RUN tar -xzvf /RDP-TestSuite-ClientEP.tar.gz -C /opt/rdpclient
RUN mkdir -p /opt/ptmcli
RUN tar -xzvf /PTMCli.tar.gz -C /opt/ptmcli
RUN mkdir -p /opt/ptmservice
RUN tar -xzvf /PTMService.tar.gz -C /opt/ptmservice
RUN chmod +x entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]
