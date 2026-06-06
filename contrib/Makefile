APP_PROJECT_NAMES=Api Org Blogs
APP_PROJECTS=src/Yavsc.Org/Yavsc.Org.csproj src/Yavsc.Api/Yavsc.Api.csproj
SLNDIR=..
DESTDIR=/srv/www/yavsc/
USER_AND_GROUP=www-data:www-data
CONFIGURATION=Release
DOTNET_FRAMEWORK=net10.0

include $(SLNDIR)/.env

generated/:
	@mkdir -p $@

generated/Api.service:
generated/Org.service:
generated/Blogs.service:

generated/yavsc%.service: generated/ template.service $(SLNDIR)/.env
	@cat template.service | APP_NAME="$*" \
	DESTDIR="$(DESTDIR)" \
	APP_SYSLOG_ID="yavsc$*" \
	POSTGRES_HOST="$(POSTGRES_HOST)" \
	POSTGRES_PORT="$(POSTGRES_PORT)" \
	POSTGRES_DB="$(POSTGRES_DB)" \
	POSTGRES_USER="$(POSTGRES_USER)" \
	POSTGRES_PASSWORD="$(POSTGRES_PASSWORD)" \
	ANTHROPIC_API_KEY="$(ANTHROPIC_API_KEY)" \
	ANTHROPIC_MAX_TOKENS="$(ANTHROPIC_MAX_TOKENS)" \
	envsubst > $@
	@echo Created service file: $@


copy-services: copy-service-Org copy-service-Api copy-service-Blogs
copy-service-Org: /etc/systemd/system/yavscOrg.service
copy-service-Api: /etc/systemd/system/yavscApi.service
copy-service-Blogs: /etc/systemd/system/yavscBlogs.service

copy-binaries:
	for project in $(APP_PROJECT_NAMES); \
	do \
		sudo cp -a $(SLNDIR)/src/Yavsc.$${project}/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish/* $(DESTDIR) ; \
	done
	sudo chown -R $(USER_AND_GROUP) $(DESTDIR)

/etc/systemd/system/yavsc%.service: generated/yavsc%.service 
	sudo cp $^ $@
	sudo chown root:root $@
	sudo systemctl daemon-reload
	
build_publish: 
	for f in $(APP_PROJECTS); do \
		ASPNETCORE_ENV=$(CONFIGURATION) dotnet publish $(SLNDIR)/$$f ; \
	done

install: build_publish copy-services copy-binaries
	sudo systemctl daemon-reload
	for project in $(APP_PROJECT_NAMES); \
	do \
		sudo systemctl enable yavsc$${project} ; \
		sudo systemctl start yavsc$${project} ; \
	done
	
reinstall: build_publish copy-services copy-binaries
	for project in $(APP_PROJECT_NAMES); \
	do \
		sudo systemctl stop yavsc$${project} ; \
	done
	for project in $(APP_PROJECT_NAMES); \
	do \
		sudo systemctl start yavsc$${project} ; \
	done
	
showConfig: 
	@echo CONFIGURATION: $(CONFIGURATION)
	@echo DESTDIR: $(DESTDIR)

clean:
	rm -rf generated

.PHONY: build_publish mep showConfig copy-service-Api copy-service-Org copy-service-Blogs reinstall clean
