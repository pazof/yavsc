# syntax=docker/dockerfile:1
# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["/src", "/src"]
COPY "Directory.Packages.props" "Directory.Packages.props"

RUN dotnet nuget add source https://isn.pschneider.fr/v3/index.json --allow-insecure-connections

# Just to see if two lines above work
RUN dotnet nuget list source

RUN dotnet restore "/src/Yavsc/Yavsc.csproj"

# Copy source code and publish app

WORKDIR "/src/Yavsc"
RUN dotnet build "Yavsc.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Yavsc.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Yavsc.dll"]
