# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet nuget add source https://isn.pschneider.fr/v3/index.json --allow-insecure-connections
RUN dotnet publish src/Yavsc.Org/Yavsc.Org.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Variables d'environnement avec valeurs par défaut
ARG POSTGRES_HOST=localhost
ARG POSTGRES_PORT=5432
ARG POSTGRES_DB=yavsc
ARG POSTGRES_USER=yavsc
ARG POSTGRES_PASSWORD

ENV POSTGRES_HOST=${POSTGRES_HOST}
ENV POSTGRES_PORT=${POSTGRES_PORT}
ENV POSTGRES_DB=${POSTGRES_DB}
ENV POSTGRES_USER=${POSTGRES_USER}
ENV POSTGRES_PASSWORD=${POSTGRES_PASSWORD}

# Chaîne de connexion construite depuis les variables
ENV ConnectionStrings__DefaultConnection=\
"Host=${POSTGRES_HOST};Port=${POSTGRES_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"

COPY --from=build /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "Yavsc.Org.dll"]
