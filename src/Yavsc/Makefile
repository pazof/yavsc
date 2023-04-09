DESTDIR=/srv/www/yavsc
HOSTING=localhost
ASPNET_LOG_LEVEL=debug
SOURCE_DIR=../..
USER=www-data
HOSTING_PRE=yavsv.pschneider.fr
CONFIGURATION=Debug
SERVICE_PRE=Yavsc

all:
	dotnet build

showConfig:
	@echo HOSTING_PRE: $(USER)@$(HOSTING_PRE)
	@echo CONFIGURATION: $(CONFIGURATION)
	@echo DESTDIR: $(DESTDIR)
	@echo SERVICE_PRE: $(SERVICE_PRE)

publish:
	ASPNETCORE_ENV=$(CONFIGURATION) dotnet publish

pushInPre: publish
	sudo service $(SERVICE_PRE) stop
	sudo cp -a bin/Debug/net7.0/publish/* $(DESTDIR)
	sudo chown -R $(USER) $(DESTDIR)
	sudo sync
	sudo service $(SERVICE_PRE) start

%.min.js: %.js
	jsmin < $^ > $@

%.min.css: %.css
	jsmin < $^ > $@

%.Designer.cs: %.resx
	strongresbuildercli -l -p -t -r "Yavsc.Resources." $^
