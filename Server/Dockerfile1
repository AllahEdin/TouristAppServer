FROM mcr.microsoft.com/dotnet/aspnet:3.1
EXPOSE 5000
COPY bin/Release/netcoreapp3.1/publish/ App/
WORKDIR /App
CMD ["dotnet", "Server.dll"]