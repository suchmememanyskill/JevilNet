FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY build/* ./

ENTRYPOINT ["dotnet", "JevilNet.dll"]