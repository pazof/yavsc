APP_PROJECT_NAMES=Api Org Blogs
SLNDIR=..

include $(SLNDIR)/.env
include .env

generated/:
	@mkdir -p $@

generated/yavscApi.service:
generated/yavscOrg.service:
generated/yavscBlogs.service:

generated/yavsc%.service: generated/ template.service $(SLNDIR)/.env
	@cat template.service | APP_NAME="$*" \
	HTTP_HOST="$(HTTP_HOST)" \
	HTTP_PORT="$*_$(HTTP_PORT)" \
	BASEAPPDIR="$(BASEAPPDIR)" \
	APP_SYSLOG_ID="yavsc$*" \
	POSTGRES_HOST="$(POSTGRES_HOST)" \
	POSTGRES_PORT="$(POSTGRES_PORT)" \
	POSTGRES_DB="$(POSTGRES_DB)" \
	POSTGRES_USER="$(POSTGRES_USER)" \
	POSTGRES_PASSWORD="$(POSTGRES_PASSWORD)" \
	ANTHROPIC_API_KEY="$(ANTHROPIC_API_KEY)" \
	ANTHROPIC_MAX_TOKENS="$(ANTHROPIC_MAX_TOKENS)" \
	ASPNETCORE_Smtp__Host="$(ASPNETCORE_Smtp__Host)" \
	ASPNETCORE_Smtp__Port="$(ASPNETCORE_Smtp__Port)" \
	ASPNETCORE_Smtp__SenderName=$(ASPNETCORE_Smtp__SenderName) \
	ASPNETCORE_Smtp__SenderEmail="$(ASPNETCORE_Smtp__SenderEmail)" \
	ASPNETCORE_Smtp__UserName="$(ASPNETCORE_Smtp__UserName)" \
	ASPNETCORE_Smtp__Password="$(ASPNETCORE_Smtp__Password)" \
	envsubst > $@
	@echo Created service file: $@


copy-services: copy-service-Org copy-service-Api copy-service-Blogs
copy-service-Org: /etc/systemd/system/yavscOrg.service
copy-service-Api: /etc/systemd/system/yavscApi.service
copy-service-Blogs: /etc/systemd/system/yavscBlogs.service

copy-binaries: build_publish_Org build_publish_Api build_publish_Blogs stop-services 
	@for project in $(APP_PROJECT_NAMES); \
		do LCAPI=$$(echo $${project}|tr [:upper:] [:lower:]) ; \
		echo "$${project} -> $${LCAPI}" ; \
		BINDIR="$(SLNDIR)/src/Yavsc.$${project}/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish" ; \
		echo BINDIR: $${BINDIR} ; \
		rm -f $${BINDIR}/appsettings-$${LCAPI}-dist.json ; \
		mv -f $${BINDIR}/appsettings-$${LCAPI}.json \
		   $${BINDIR}/appsettings-$${LCAPI}-dist.json ; \
		rm -f $${BINDIR}/appsettings-$${LCAPI}.json ; \
		sync ; \
		ls $${BINDIR}/appsettings* 2>&1 ; \
		sudo cp -a $${BINDIR}/* $(BASEAPPDIR) ; \
	done
	@sudo chown -R $(USER_AND_GROUP) $(BASEAPPDIR)

/etc/systemd/system/yavsc%.service: generated/yavsc%.service 
	sudo cp $^ $@
	sudo chown root:root $@

build_publish_%: clean_publish_dir_%
	@ASPNETCORE_ENV=$(CONFIGURATION) dotnet publish $(SLNDIR)/src/Yavsc.$*/Yavsc.$*.csproj

clean_publish_dir_%:
	@rm -rf $(SLNDIR)/src/Yavsc.$*/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish

install: build_publish copy-binaries copy-services 
	@sudo systemctl daemon-reload
	@for project in $(APP_PROJECT_NAMES); \
	do \
		sudo systemctl enable yavsc$${project} ; \
		sudo systemctl start yavsc$${project} ; \
	done
	
reinstall: copy-binaries
	@sync
	@for project in $(APP_PROJECT_NAMES); do \
		sudo systemctl start yavsc$${project} ; done

stop-services:
	@for project in $(APP_PROJECT_NAMES); \
	do \
		sudo systemctl stop yavsc$${project} ; \
	done

$(SLNDIR)/src/Yavsc.Org/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish: build_publish
$(SLNDIR)/src/Yavsc.Blogs/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish: build_publish
$(SLNDIR)/src/Yavsc.Api/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish: build_publish

showConfig: 
	@echo CONFIGURATION: $(CONFIGURATION)
	@echo BASEAPPDIR: $(BASEAPPDIR)

clean:
	@rm -rf generated

.PHONY: build_publish mep showConfig copy-service-Api copy-service-Org copy-service-Blogs reinstall clean
