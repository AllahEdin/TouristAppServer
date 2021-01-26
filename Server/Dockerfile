FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /app
COPY *.csproj ./
RUN dotnet restore Server.csproj
COPY . ./
RUN dotnet publish Server.csproj -c Release -o out
FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /app/out .
CMD ["dotnet", "Server.dll"]