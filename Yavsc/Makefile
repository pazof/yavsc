DESTDIR=/srv/www/yavscpre
PRODDESTDIR=/srv/www/yavsc
ASPNET_ENV=Development
CONFIGURATION=Release
ASPNET_LOG_LEVEL=warn

all: bin/$(CONFIGURATION)

git_status := $(shell git status -s --porcelain |wc -l)

status:
ifeq ($(git_status),0)
		@echo Nothing to be done.
else
    @echo There are pending changes:
		@git status
endif

default: pushInPre

project.lock.json: project.json
	dnu restore

watch: project.lock.json
	ASPNET_ENV=$(ASPNET_ENV) ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) dnx-watch web --configuration=$(CONFIGURATION)

run:
	ASPNET_ENV=$(ASPNET_ENV) dnx web --configuration=$(CONFIGURATION)

clean:
	rm -rf bin obj

cleanoutput:
	rm -rf bin/$(CONFIGURATION)
	rm -rf bin/output

bin/$(CONFIGURATION): project.lock.json
	dnu build --configuration=$(CONFIGURATION)

bin/output: bin/$(CONFIGURATION)
	@dnu publish

bin/output/wwwroot/version: bin/output
	@git log -1 --pretty=format:%h > bin/output/wwwroot/version

pushInPre: cleanoutput bin/output/wwwroot/version
	ssh root@localhost systemctl stop kestrel-pre
	ssh root@localhost rm -rf $(DESTDIR)/approot
	(cd bin/output && rsync -ravu ./ root@localhost:$(DESTDIR) >/dev/null)
	ssh root@localhost sync
	ssh root@localhost systemctl start kestrel-pre

pushInProd: cleanoutput bin/output/wwwroot/version
ifeq ($(git_status),0)
	ssh root@localhost systemctl stop kestrel
	ssh root@localhost rm -rf $(PRODDESTDIR)/approot
	(cd bin/output && rsync -ravu ./ root@localhost:$(PRODDESTDIR) >/dev/null)
	ssh root@localhost sync
	ssh root@localhost systemctl start kestrel
else
	@echo Err! Refus de pousser en production: des changements doivent être validés auprès du contrôle de versions.
	git status
endif
