DESTDIR=/srv/www/yavsc
CONFIGURATION=Release

USER_AND_GROUP=www-data:www-data
SERVICE_PROD=yavsc
DOTNET_FRAMEWORK=net9.0

all:
	dotnet build

showConfig:
	@echo CONFIGURATION: $(CONFIGURATION)
	@echo DESTDIR: $(DESTDIR)
	@echo SERVICE_PROD: $(SERVICE_PROD)

publish:
	ASPNETCORE_ENV=$(CONFIGURATION) dotnet publish

install_service:
	sudo cp ../../contrib/yavsc.service /etc/systemd/system
	sudo systemctl enable yavsc

pushInProd: publish
	sudo systemctl stop $(SERVICE_PROD) 
	sudo cp -a bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish/* $(DESTDIR)
	sudo chown -R $(USER_AND_GROUP) $(DESTDIR)
	sudo sync
	sudo systemctl start $(SERVICE_PROD)

%.min.js: %.js
	jsmin < $^ > $@

%.min.css: %.css
	jsmin < $^ > $@
