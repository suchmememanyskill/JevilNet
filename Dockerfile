FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY /tmp/Build/* ./

ENTRYPOINT ["dotnet", "JevilNet.dll"]