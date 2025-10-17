FROM mcr.microsoft.com/dotnet/sdk:8.0
# Environment variables with default values which could be overridden
ENV Usage=RunTestCases
# ENV Usage=RunPTMCli
# ENV Usage=RunPTMService
ENV SutComputerName=node01.contoso.com
ENV SutIPAddress=192.168.1.11
ENV DomainName=contoso.com
ENV AdminUserName=Administrator
ENV PasswordForAllUsers=Password01!
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
ENV TestSuiteName=FileServer
ENV TestSuitePackageName=FileServer-TestSuite-ServerEP.tar.gz

COPY ./FileServer-TestSuite-ServerEP.tar.gz /
COPY ./PTMCli.tar.gz /
COPY ./PTMService.tar.gz /
COPY ./entrypoint.sh /
COPY ./runTestCases.ps1 /
COPY ./runPTMCli.ps1 /
COPY ./runPTMService.ps1 /
COPY ./config /root/.ssh/
COPY ./configSshClient.ps1 /
RUN apt update && apt install -y openssh-server
RUN mkdir -p /opt/fileserver
RUN tar -xzvf /FileServer-TestSuite-ServerEP.tar.gz -C /opt/fileserver
RUN mkdir -p /opt/ptmcli
RUN tar -xzvf /PTMCli.tar.gz -C /opt/ptmcli
RUN mkdir -p /opt/ptmservice
RUN tar -xzvf /PTMService.tar.gz -C /opt/ptmservice
RUN chmod +x entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]
