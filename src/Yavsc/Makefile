DESTDIR=/srv/www/yavscpre
PRODDESTDIR=/srv/www/yavsc
HOSTING=localhost
HOSTADMIN=root
ASPNET_LOG_LEVEL=warn
SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
include $(MAKEFILE_DIR)/versioning.mk
include $(MAKEFILE_DIR)/dnx.mk

web: project.lock.json
	ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) ASPNET_ENV=$(ASPNET_ENV) dnx web --configuration=$(CONFIGURATION)

status:
ifeq ($(git_status),0)
		@echo Nothing to be done.
else
    @echo There are pending changes:
		@git status
endif

publish:
	ASPNET_ENV=$(ASPNET_ENV) dnu publish

deploy-pkg: publish

deploy: pushInPre pushInProd

pushInPre: cleanoutput bin/output/wwwroot/version
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel-pre stop
	ssh $(HOSTADMIN)@$(HOSTING) sudo rm -rf $(DESTDIR)/approot
	(cd bin/output && rsync -ravu ./ $(HOSTADMIN)@$(HOSTING):$(DESTDIR) >/dev/null)
	ssh $(HOSTADMIN)@$(HOSTING) sudo sync
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel-pre start

pushInProd: cleanoutput bin/output/wwwroot/version
ifeq ($(git_status),0)
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel stop
	ssh $(HOSTADMIN)@$(HOSTING) sudo rm -rf $(PRODDESTDIR)/approot
	(cd bin/output && rsync -ravu ./ $(HOSTADMIN)@$(HOSTING):$(PRODDESTDIR) >/dev/null)
	ssh $(HOSTADMIN)@$(HOSTING) sudo sync
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel start
else
	$(error EPRODANDGITSTATUS! Refus de pousser en production: des changements doivent être validés auprès du contrôle de versions.)
	git status
endif

