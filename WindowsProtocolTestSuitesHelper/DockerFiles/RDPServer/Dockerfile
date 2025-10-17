FROM mcr.microsoft.com/dotnet/sdk:8.0
# Environment variables with default values which could be overridden 
# By default, the container will list all the available test cases contained in the binaries
# You can override the Filter like "TestCategory=BVT"
ENV Filter=""
ENV DryRun="y"

RUN apt-get update -y
RUN apt-get install -y xmlstarlet
COPY ./RDP-TestSuite-ServerEP.tar.gz /
COPY ./entrypoint.sh /
RUN mkdir -p /opt/rdpserver
RUN tar -xzvf /RDP-TestSuite-ServerEP.tar.gz -C /opt/rdpserver
RUN chmod +x entrypoint.sh
ENTRYPOINT ["/entrypoint.sh"]