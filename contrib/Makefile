APP_PROJECTS=src/Org/Org.csproj src/Api/Api.csproj
SLNDIR=..

generated/%.service: template.service %.env .env
	envsubst < template.service > $@

copy-service-Org: $(DESTDIR)/etc/systemd/system/Org.service

$(DESTDIR)/etc/systemd/system/%.service: generated/%.service 
	sudo cp $^ $@
	sudo systemctl daemon-reload

build_publish: 
	for f in $(APP_PROJECTS); do \
		ASPNETCORE_ENV=$(CONFIGURATION) dotnet publish $(SLNDIR)/$$f ; \
	done

rmep: build_publish
	sudo systemctl stop yavsc-api
	sudo systemctl stop yavsc-org
	for f in Api Org; do
		pushd $(SLNDIR)/src/$$prj
		SOURCE_DIR=bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish/
		sudo cp -a $(SOURCE_DIR)/* $(DESTDIR) 
		sudo chown -R $(USER_AND_GROUP) $(DESTDIR)
		popd
	done
	sudo sync
	sudo systemctl start yavsc-org
	sudo systemctl start yavsc-api
	
showConfig: 
	@echo CONFIGURATION: $(CONFIGURATION)
	@echo DESTDIR: $(DESTDIR)

reinstall: copy-service-Org copy-service-Api rmep
