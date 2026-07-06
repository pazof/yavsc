# =====================================================================
# Dockerfile multi-stage pour Yavsc
# =====================================================================
#
# Image de base : pazof/yavsc-build-env:<tag>, construite depuis le repo
# sibling dotnet-android-build-image (Debian 12 + .NET 10 + Android SDK 36
# + workload .NET Android). Le tag est bumpé en lockstep ici, dans
# Dockerfile.backend, et dans docker-compose.yaml.
#
# Stages :
#   build-env      (par défaut) — restore + build + publish, sert aussi
#                  pour extraire l'APK Android via
#                  .github/workflows/docker-publish-android.yml
#   publish-org    — copie minimale du publish Yavsc.Org
#   publish-api    — copie minimale du publish Yavsc.Api
#   publish-blogs  — copie minimale du publish Yavsc.Blogs
#   web-runtime    — image ASP.NET pour le front web (port 5000)
#   api-runtime    — image ASP.NET pour l'API REST (port 5002)
#   blogs-runtime  — image ASP.NET pour le backend blogs (port 5004)
#
# docker-compose.yaml cible les stages web-runtime / api-runtime /
# blogs-runtime via `target:`. Le stage par défaut reste build-env
# pour la compat avec le workflow APK.
# =====================================================================

# syntax=docker/dockerfile:1.7
ARG BUILD_ENV_TAG=debian12-dotnet10-android36-v1
FROM pazof/yavsc-build-env:${BUILD_ENV_TAG} AS build-env
WORKDIR /src

# (1) Manifest + fichiers de projet pour le cache de restore
COPY *.sln ./
COPY Directory.*.props ./
COPY src/Yavsc.Org/*.csproj ./src/Yavsc.Org/
COPY src/Yavsc.Abstract/*.csproj ./src/Yavsc.Abstract/
COPY src/Yavsc.Server/*.csproj ./src/Yavsc.Server/
COPY src/Yavsc.Api/*.csproj ./src/Yavsc.Api/
COPY src/Yavsc.Blogs/*.csproj ./src/Yavsc.Blogs/
COPY src/cli/*.csproj ./src/cli/
COPY src/Yavsc.Org.Tests/*.csproj ./src/Yavsc.Org.Tests/
COPY src/PostIt/PostIt/*.csproj ./src/PostIt/PostIt/
COPY src/PostIt/PostIt.Android/*.csproj ./src/PostIt/PostIt.Android/
COPY src/PostIt/PostIt.Browser/*.csproj ./src/PostIt/PostIt.Browser/
COPY src/PostIt/PostIt.Desktop/*.csproj ./src/PostIt/PostIt.Desktop/

# (2) Tout le code source
COPY . .

# (3) Source NuGet interne (Letsencrypt, certificat auto-signé côté
# serveur, justifié par build privé).
RUN dotnet nuget add source https://isn.pschneider.fr/api/v3/index.json --allow-insecure-connections

# (4) Restore
RUN dotnet restore

# (5) Build des trois services web principaux
RUN dotnet build src/Yavsc.Org/Yavsc.Org.csproj -c Release --no-restore -clp:ErrorsOnly
RUN dotnet build src/Yavsc.Api/Yavsc.Api.csproj -c Release --no-restore -clp:ErrorsOnly
RUN dotnet build src/Yavsc.Blogs/Yavsc.Blogs.csproj -c Release --no-restore -clp:ErrorsOnly

# (6) Build APK Android (utilisé par le workflow docker-publish-android).
ARG ANDROID_TARGET_RID=android-arm64
RUN dotnet build src/PostIt/PostIt.Android/PostIt.Android.csproj -c Release --no-restore -clp:ErrorsOnly -r ${ANDROID_TARGET_RID}

# (7) Publish des trois services web (les artifacts sont consommés par
# les stages publish-org / publish-api / publish-blogs ci-dessous).
RUN mkdir -p /app/publish
RUN dotnet publish src/Yavsc.Org/Yavsc.Org.csproj -c Release --no-build -o /app/publish/Yavsc.Org
RUN dotnet publish src/Yavsc.Api/Yavsc.Api.csproj -c Release --no-build -o /app/publish/Yavsc.Api
RUN dotnet publish src/Yavsc.Blogs/Yavsc.Blogs.csproj -c Release --no-build -o /app/publish/Yavsc.Blogs

# Stage par défaut : on laisse l'image utilisable comme build-env
# (utilisé par docker-publish-android.yml qui extrait l'APK).
CMD ["bash"]

# ---------------------------------------------------------------------
# Stages publish-<service> : isole la copie du publish pour ne pas
# embarquer tout le code source dans l'image runtime.
# ---------------------------------------------------------------------

FROM build-env AS publish-org
COPY --from=build-env /app/publish/Yavsc.Org/ /publish/

FROM build-env AS publish-api
COPY --from=build-env /app/publish/Yavsc.Api/ /publish/

FROM build-env AS publish-blogs
COPY --from=build-env /app/publish/Yavsc.Blogs/ /publish/

# ---------------------------------------------------------------------
# Stages runtime : images ASP.NET minimales.
#
# appsettings-org.json est injecté via BuildKit secret mount
# (`docker build --secret id=yavsc_appsettings`). En production le
# secret est fourni par le store CI ; en dev local c'est le fichier
# src/Yavsc.Org/appsettings-org.json passé via docker-compose.yaml.
#
# Pour activer HTTPS en prod : monter /etc/letsencrypt en volume et
# renseigner Kestrel:Certificates dans appsettings-org.json. Voir
# CONTRIBUTING.md.
# ---------------------------------------------------------------------

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS web-runtime
WORKDIR /app
COPY --from=publish-org /publish/ ./
RUN --mount=type=secret,id=yavsc_appsettings,dst=/run/secrets/yavsc_appsettings \
    cp /run/secrets/yavsc_appsettings /app/appsettings-org.json \
    && chmod 0644 /app/appsettings-org.json
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget --quiet --spider http://localhost:5000/ || exit 1
ENTRYPOINT ["dotnet", "Yavsc.Org.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api-runtime
WORKDIR /app
COPY --from=publish-api /publish/ ./
RUN --mount=type=secret,id=yavsc_appsettings,dst=/run/secrets/yavsc_appsettings \
    cp /run/secrets/yavsc_appsettings /app/appsettings-org.json \
    && chmod 0644 /app/appsettings-org.json
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5002
EXPOSE 5002
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget --quiet --spider http://localhost:5002/ || exit 1
ENTRYPOINT ["dotnet", "Yavsc.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS blogs-runtime
WORKDIR /app
COPY --from=publish-blogs /publish/ ./
RUN --mount=type=secret,id=yavsc_appsettings,dst=/run/secrets/yavsc_appsettings \
    cp /run/secrets/yavsc_appsettings /app/appsettings-org.json \
    && chmod 0644 /app/appsettings-org.json
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5004
EXPOSE 5004
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD wget --quiet --spider http://localhost:5004/ || exit 1
ENTRYPOINT ["dotnet", "Yavsc.Blogs.dll"]
