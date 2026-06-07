# Utilisation de votre image globale pour le build et l'exécution
FROM pazof/yavsc-build-env:latest AS build-env
WORKDIR /src


# 1. Copie de la solution
COPY *.sln ./
COPY Directory.*.props ./

# 2. Copie de l'intégralité des fichiers de projets (.csproj) pour la restauration
COPY src/Yavsc.Org/*.csproj ./src/Yavsc.Org/
COPY src/Yavsc.Abstract/*.csproj ./src/Yavsc.Abstract/
COPY src/Yavsc.Server/*.csproj ./src/Yavsc.Server/
COPY src/Yavsc.Web/*.csproj ./src/Yavsc.Web/
COPY src/Yavsc.Api/*.csproj ./src/Yavsc.Api/
COPY src/Yavsc.Blogs/*.csproj ./src/Yavsc.Blogs/
COPY src/cli/*.csproj ./src/cli/
COPY test/yavscTests/*.csproj ./test/yavscTests/

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

# 6. Compilation du serveur Web principal
RUN dotnet build src/Yavsc.Org/Yavsc.Org.csproj -c Release --no-restore -clp:ErrorsOnly

# 7. Génération de l'APK Android (Forçage de l'identifiant de runtime mobile -r)
RUN dotnet build src/PostIt/PostIt.Android/PostIt.Android.csproj -c Release -f net10.0-android -r android-arm64 --no-restore -clp:ErrorsOnly

# Définition du répertoire d'exécution par défaut
CMD ["bash"]
