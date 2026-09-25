
CONFIG=Debug
FRAMEWORK=net9.0
DESTDIR=/tmp/yavsc
APP_PATH=srv/www/yavsc
APP_FULL_PATH=$(DESTDIR)/$(APP_PATH)

include .env

all:
	dotnet build --nologo

clean:
	dotnet clean -c $(CONFIG)

brutal_clean:
	@find -name "obj" -exec rm -rf {} \;
	@find -name "bin" -exec rm -rf {} \;

src/Yavsc/bin/output/wwwroot:
	dotnet --project src/Yavsc.Org/Yavsc.Org.csproj publish

test:
	ASPNETCORE_ENVIRONMENT=Development dotnet test --report-xunit \
    --report-xunit-html \
		--results-directory "test-reports" \
    --report-xunit-html-filename "test-results.html"

test-postit:
	ASPNETCORE_ENVIRONMENT=Development dotnet test \
	  --report-xunit \
    --report-xunit-html \
		--results-directory "test-reports" \
    --report-xunit-html-filename "postit-test-results.html" \
	  --project src/PostIt/PostIt.Tests/PostIt.Tests.csproj

backend-tests: Blogs-backend-test Org-backend-test Api-backend-test
Api-backend-test:
Blogs-backend-test:
Org-backend-test:
%-backend-test:
	ASPNETCORE_ENVIRONMENT=Development dotnet test \
	  --report-xunit \
    --report-xunit-html \
		--results-directory "test-reports" \
    --report-xunit-html-filename "$*-backend-test-results.html" \
	  --project src/test/Yavsc.$*.Tests/Yavsc.$*.Tests.csproj

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

.PHONY: test install docker-image docker-build docker-run
