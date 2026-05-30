
CONFIG=Debug
FRAMEWORK=net9.0
DESTDIR=/tmp/yavsc
APP_PATH=srv/www/yavsc
APP_FULL_PATH=$(DESTDIR)/$(APP_PATH)

all:
	dotnet build --nologo

clean: 
	dotnet clean

src/Yavsc/bin/output/wwwroot:
	dotnet --project src/Yavsc.Org/Yavsc.Org.csproj publish

test:
	dotnet test

watch:
	dotnet watch -p:Configuration=$(CONFIG) --project src/Yavsc/Yavsc.csproj

src/Yavsc.Abstract/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.Abstract.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Abstract/Yavsc.Abstract.csproj

src/Yavsc.Server/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.Server.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Server/Yavsc.Server.csproj

src/Yavsc/bin/$(CONFIG)/$(FRAMEWORK)/Yavsc.dll:
	dotnet build -p:Configuration=$(CONFIG) --project src/Yavsc.Org/Yavsc.Org.csproj

$(DESTDIR): 
	mkdir $(DESTDIR)

install: $(DESTDIR)
	dotnet publish src/Yavsc.Org/Yavsc.Org.csproj -c Release -o $(APP_FULL_PATH)
	dotnet publish src/Api/Api.csproj -c Release -o $(APP_FULL_PATH)
	sudo chown -R www-data:www-data $(APP_FULL_PATH)

docker-image:
	docker build .

docker-build:
	docker compose up --build

docker-run:
	docker run -d -p 5000:5000 --name yavsc yavsc


.PHONY: 
