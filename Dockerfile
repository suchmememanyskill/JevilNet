FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY build/* ./

ENTRYPOINT ["dotnet", "JevilNet.dll"]