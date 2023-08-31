FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY ~/Build/* ./

ENTRYPOINT ["dotnet", "JevilNet.dll"]