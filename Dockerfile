# Utilisation de votre image globale pour le build et l'exécution
FROM pazof/yavsc-build-env:debian12-dotnet10-android36-v1 AS build-env
WORKDIR /src


# 1. Copie de la solution
COPY *.sln ./
COPY Directory.*.props ./

# 2. Copie de l'intégralité des fichiers de projets (.csproj) pour la restauration
COPY src/Yavsc.Org/*.csproj ./src/Yavsc.Org/
COPY src/Yavsc.Abstract/*.csproj ./src/Yavsc.Abstract/
COPY src/Yavsc.Server/*.csproj ./src/Yavsc.Server/
COPY src/Yavsc.Api/*.csproj ./src/Yavsc.Api/
COPY src/Yavsc.Blogs/*.csproj ./src/Yavsc.Blogs/
COPY src/cli/*.csproj ./src/cli/
COPY src/Yavsc.Org.Tests/*.csproj ./src/Yavsc.Org.Tests/

# Projets associés à PostIt
COPY src/PostIt/PostIt/*.csproj ./src/PostIt/PostIt/
COPY src/PostIt/PostIt.Android/*.csproj ./src/PostIt/PostIt.Android/
COPY src/PostIt/PostIt.Browser/*.csproj ./src/PostIt/PostIt.Browser/
COPY src/PostIt/PostIt.Desktop/*.csproj ./src/PostIt/PostIt.Desktop/

# 4. Copie de l'intégralité du code source
COPY . .

# 3. Restauration des dépendances avec vos workloads actifs
RUN dotnet nuget add source https://isn.pschneider.fr/v3/index.json --allow-insecure-connections

# 4. Restauration des dépendances pour tous les projets
RUN dotnet restore

# 5. Copie de la totalité du code source (filtrée par votre .dockerignore)
COPY . .
# Définir une variable d'architecture par défaut (ex: android-arm64)
ARG ANDROID_TARGET_RID=android-arm64

#  6. Compilation du serveur Web principal, Injecter l'option -r avec la variable
RUN dotnet build src/Yavsc.Org/Yavsc.Org.csproj -c Release --no-restore -clp:ErrorsOnly
RUN dotnet build src/Yavsc.Api/Yavsc.Api.csproj -c Release --no-restore -clp:ErrorsOnly
RUN dotnet build src/Yavsc.Blogs/Yavsc.Blogs.csproj -c Release --no-restore -clp:ErrorsOnly

# 6. Compilation du serveur Web principal Injecter l'option -r avec la variable
RUN dotnet build src/PostIt/PostIt.Android/PostIt.Android.csproj -c Release --no-restore -clp:ErrorsOnly -r ${ANDROID_TARGET_RID}

# 7. Publication des artefacts pour les images runtime (Dockerfile.runtime*)
#    Le repertoire /app/publish/<project>/ est copie tel quel dans l'image
#    runtime via --from=build-env. Les appsettings ne sont PAS publies ici :
#    ils sont fournis au runtime soit via BuildKit secret mount, soit via
#    volume monte par docker-compose.
RUN mkdir -p /app/publish
RUN dotnet publish src/Yavsc.Org/Yavsc.Org.csproj -c Release --no-build -o /app/publish/Yavsc.Org
RUN dotnet publish src/Yavsc.Api/Yavsc.Api.csproj -c Release --no-build -o /app/publish/Yavsc.Api
RUN dotnet publish src/Yavsc.Blogs/Yavsc.Blogs.csproj -c Release --no-build -o /app/publish/Yavsc.Blogs

# Définition du répertoire d'exécution par défaut
CMD ["bash"]
