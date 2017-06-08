DESTDIR=/srv/www/yavscpre
PRODDESTDIR=/srv/www/yavsc
ASPNET_ENV=Development

git_status := $(shell git status -s --porcelain |wc -l)

status:
ifeq ($(git_status),0)
		@echo Nothing to be done.
else
		@git status
endif

default: pushInPre

watch:
	@ASPNET_ENV=$(ASPNET_ENV) dnx-watch web --configuration=Debug

run:
	@ASPNET_ENV=$(ASPNET_ENV) dnx web --configuration=Debug

clean:
	@rm -rf bin/Release
	@rm -rf bin/output

bin/Release:
	@dnu build --configuration=Release

bin/output: bin/Release
	@dnu publish

bin/output/wwwroot/version: bin/output
	@git log -1 --pretty=format:%h > bin/output/wwwroot/version

pushInPre: bin/output/wwwroot/version
	ssh root@localhost systemctl stop kestrel-pre
	ssh root@localhost rm -rf $(DESTDIR)/approot
	(cd bin/output && rsync -ravu ./ root@localhost:$(DESTDIR) >/dev/null)
	ssh root@localhost sync
	ssh root@localhost systemctl start kestrel-pre

pushInProd: bin/output/wwwroot/version
ifeq ($(git_status),0)
	@ssh root@localhost systemctl stop kestrel
	@ssh root@localhost rm -rf $(PRODDESTDIR)/approot
	@(cd bin/output && rsync -ravu ./ root@localhost:$(PRODDESTDIR) >/dev/null)
	@ssh root@localhost sync
	@ssh root@localhost systemctl start kestrel
else
	@echo Err! Refus de pousser en production: des changements doivent être validés auprès du contrôle de versions.
	@git status
endif
