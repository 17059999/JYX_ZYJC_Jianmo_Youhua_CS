FROM mcr.microsoft.com/dotnet/sdk:8.0
# Environment variables with default values which could be overridden
ENV ServerComputerName="SutComputer"
ENV UserName="Administrator"
ENV Password="Password01!"
ENV DomainName=""
ENV ShareName="Test"
ENV QueryPath="file://SutComputer/Test/"
ENV QueryText="test"
# By default, the container will list all the available test cases contained in the binaries
# You can override the Filter like "TestCategory=BVT"
ENV Filter=""
ENV DryRun="y"

RUN apt-get update -y
RUN apt-get install -y xmlstarlet
COPY ./MS-WSP-TestSuite-ServerEP.tar.gz /
COPY ./entrypoint.sh /
RUN mkdir -p /opt/ms-wsp
RUN tar -xzvf /MS-WSP-TestSuite-ServerEP.tar.gz -C /opt/ms-wsp
RUN chmod +x entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]
