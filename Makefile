
CONFIG=Debug
FRAMEWORK=net8.0

clean: 
	dotnet clean

src/Yavsc/bin/output/wwwroot:
	dotnet --project src/Yavsc/Yavsc.csproj publish

test:
	dotnet test

web:
	dotnet watch -p:Configuration=$(CONFIG) --project src/Yavsc/Yavsc.csproj

src/Yavsc.Abstract/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.Abstract.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Abstract/Yavsc.Abstract.csproj

src/Yavsc.Server/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.Server.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Server/Yavsc.Server.csproj

src/Yavsc/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc/Yavsc.csproj

.PHONY: 
