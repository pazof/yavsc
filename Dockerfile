# syntax=docker/dockerfile:1
# Learn about building .NET container images:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/README.md
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
ARG TARGETARCH=amd64

WORKDIR /source

# Copy project file and restore as distinct layers
COPY --link src src
COPY --link Directory.Packages.props .

RUN dotnet nuget add source https://isn.pschneider.fr/v3/index.json
RUN dotnet restore -a $TARGETARCH src/Yavsc/Yavsc.csproj

# Copy source code and publish app
RUN dotnet publish -a $TARGETARCH --no-restore -o /app src/Yavsc/Yavsc.csproj

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine
WORKDIR /app
COPY --link --from=build /app .
USER $APP_UID
ENTRYPOINT ["./Yavsc"]

