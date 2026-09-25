DESTDIR=
DOTNET_FRAMEWORK=net10.0
APP_PROJECT_NAMES=Org Blogs Api
SLNDIR=..
APP_INSTALL_ENV=preprod
APP_INSTALL_PREFIX=preprod-

include .$(APP_INSTALL_ENV).env

reinstall: copy-binaries
	@for project in $(APP_PROJECT_NAMES); do \
		echo "🚀 Starting $(APP_INSTALL_PREFIX)yavsc$${project}" ; \
		sudo systemctl start $(APP_INSTALL_PREFIX)yavsc$${project} ; done
	@echo "💯 backend started"

reinstall-prod:
	make reinstall APP_INSTALL_ENV= APP_INSTALL_PREFIX= APP_INSTALL_ENV=production

generated/$(APP_INSTALL_PREFIX)yavscOrg.service:
generated/$(APP_INSTALL_PREFIX)yavscBlogs.service:
generated/$(APP_INSTALL_PREFIX)yavscApi.service:

generated/$(APP_INSTALL_PREFIX)yavsc%.service: generated/ template.service $(SLNDIR)/.env
	@cat template.service | APP_NAME="$*" \
	DESTDIR="$(DESTDIR)" \
	HTTP_HOST="$(HTTP_HOST)" \
	HTTP_PORT="$($*_PORT)" \
	APPDIR="$(APPDIR)" \
	APP_SYSLOG_ID="$(APP_INSTALL_PREFIX)yavsc$*" \
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

generate-service-units: generated/$(APP_INSTALL_PREFIX)yavscOrg.service generated/$(APP_INSTALL_PREFIX)yavscBlogs.service generated/$(APP_INSTALL_PREFIX)yavscApi.service

# --- Vhosts nginx (générés par envsubst, à l'image des unités systemd) ---
#   make generate-nginx                                      -> generated/nginx/preprod-yavsc{Org,Blogs,Api}
#   make generate-nginx APP_INSTALL_ENV=production APP_INSTALL_PREFIX= -> prod (yavsc/api/blogs.pschneider.fr)
#
# HTTP/3 (QUIC) termine au edge nginx ; Kestrel reste en h1/h2 backend.
# Les vhosts générés vont dans generated/nginx/ (gitignoré, comme generated/*.service).
NGINX_TEMPLATE := template.vhost
NGINX_OUT_DIR  := generated/nginx
NGINX_HOSTS    := Org Blogs Api
DOMAIN_ROOT    ?= pschneider.fr
# Schéma backend Kestrel : https en prod (Kestrel écoute HTTPS, cert Let's
# Encrypt), http en preprod (Kestrel écoute HTTP simple, cf. .preprod.env).
BACKEND_SCHEME ?= $(if $(filter production,$(APP_INSTALL_ENV)),https,http)
# sous-domaine par hôte : Org -> yavsc, Api -> api, Blogs -> blogs
nginx_subdomain = $(if $(filter Org,$1),yavsc,$(if $(filter Api,$1),api,$(if $(filter Blogs,$1),blogs,$1)))
# domaine complet : <prefix><subdomain>.<DOMAIN_ROOT>
#   prod    Org -> yavsc.pschneider.fr
#   preprod Org -> preprod-yavsc.pschneider.fr
nginx_domain = $(APP_INSTALL_PREFIX)$(call nginx_subdomain,$1).$(DOMAIN_ROOT)

$(NGINX_OUT_DIR)/$(APP_INSTALL_PREFIX)yavsc%: $(NGINX_OUT_DIR) $(NGINX_TEMPLATE) .$(APP_INSTALL_ENV).env
	@cat $(NGINX_TEMPLATE) \
	  | DOMAIN="$(call nginx_domain,$*)" \
	    BACKEND_SCHEME="$(BACKEND_SCHEME)" \
	    HTTP_HOST="$(HTTP_HOST)" \
	    BACKEND_PORT="$($*_PORT)" \
	    QUIC_OPTS="$(if $(filter Org,$*),reuseport,)" \
	  envsubst '$$DOMAIN $$BACKEND_SCHEME $$HTTP_HOST $$BACKEND_PORT $$QUIC_OPTS' > $@
	@echo Created nginx vhost: $@

generate-nginx: $(addprefix $(NGINX_OUT_DIR)/$(APP_INSTALL_PREFIX)yavsc,$(NGINX_HOSTS))

generate-nginx-prod:
	make generate-nginx APP_INSTALL_ENV=production APP_INSTALL_PREFIX=

$(NGINX_OUT_DIR):
	@mkdir -p $@

install-service-%: /etc/systemd/system/$(APP_INSTALL_PREFIX)yavsc%.service
	sudo systemctl daemon-reload
	sudo systemctl enable $(APP_INSTALL_PREFIX)yavsc$*

install-services: install-service-Org install-service-Blogs install-service-Api

copy-binaries: build_publish_Org build_publish_Blogs build_publish_Api stop-services
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
		sudo cp -a $${BINDIR}/* $(DESTDIR)$(APPDIR) ; \
	done
	@sudo chown -R $(USER_AND_GROUP) $(DESTDIR)$(APPDIR)
	@sync

$(DESTDIR)/etc/systemd/system/$(APP_INSTALL_PREFIX)yavsc%.service:  generated/$(APP_INSTALL_PREFIX)yavsc%.service
	sudo cp $^ $@
	sudo chown root:root $@

build_publish_%: clean_publish_dir_%
	@ASPNETCORE_ENV=$(CONFIGURATION) dotnet publish $(SLNDIR)/src/Yavsc.$*/Yavsc.$*.csproj

clean_publish_dir_%:
	@rm -rf $(SLNDIR)/src/Yavsc.$*/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish

install: copy-binaries copy-services
	@sudo systemctl daemon-reload
	@for project in $(APP_PROJECT_NAMES); \
	do \
	  echo "🖒 enable $(APP_INSTALL_PREFIX)yavsc$${project}" ; \
		sudo systemctl enable $(APP_INSTALL_PREFIX)yavsc$${project} ; \
	  echo "🚀 start $(APP_INSTALL_PREFIX)yavsc$${project}" ; \
		sudo systemctl start $(APP_INSTALL_PREFIX)yavsc$${project} ; \
	done

stop-services:
	@for project in $(APP_PROJECT_NAMES); \
	do \
	  echo "🔥 Stopping $(APP_INSTALL_PREFIX)yavsc$${project}" ; \
		sudo systemctl stop $(APP_INSTALL_PREFIX)yavsc$${project} ; \
	done

$(SLNDIR)/src/Yavsc.Org/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish: build_publish
$(SLNDIR)/src/Yavsc.Blogs/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish: build_publish
$(SLNDIR)/src/Yavsc.Api/bin/$(CONFIGURATION)/$(DOTNET_FRAMEWORK)/publish: build_publish

showConfig:
	@echo CONFIGURATION: $(CONFIGURATION)
	@echo APPDIR: $(APPDIR)

showApiLogs:
	@sudo journalctl -u yavscApi.service -S "2 min ago" | tee yavscApi.log

showOrgLogs:
	@sudo journalctl -u yavscOrg.service -S "2 min ago" | tee yavscOrg.log

showBlogsLogs:
	@sudo journalctl -u yavscBlogs.service -S "2 min ago" | tee yavscBlogs.log

clean:
	@rm -rf generated

generated/:
	@mkdir -p $@
