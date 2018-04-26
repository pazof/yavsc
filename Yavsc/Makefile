PRJNAME=Yavsc
DESTDIR=/srv/www/yavscpre
PRODDESTDIR=/srv/www/yavsc
ASPNET_ENV=Development
CONFIGURATION=Release
ASPNET_LOG_LEVEL=info #warn
HOSTING=localhost
HOSTADMIN=root
FRAMEWORKALIAS=dnx451
BINTARGET=$(PRJNAME).dll
BINTARGETPATH=bin/$(CONFIGURATION)/$(FRAMEWORKALIAS)/$(BINTARGET)

PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
DESTPATH=.
PACKAGE=$(DESTPATH)/$(PKGFILENAME)
NUGETSOURCE=$(HOME)/Nupkgs/

all: $(BINTARGETPATH)

$(BINTARGETPATH):
	dnu build

deploy: clean pushInPre pushInProd

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

run: project.lock.json
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
	@echo EPRODANDGITSTATUS! Refus de pousser en production: des changements doivent être validés auprès du contrôle de versions.
	git status
endif


$(PACKAGE): $(BINTARGETPATH)
	nuget pack $(PRJNAME).nuspec -Version $(VERSION) -Properties config=$(CONFIGURATION)

clean:
	rm $(PACKAGE)

$(BINARY): project.lock.json
	dnu build --configuration $(CONFIGURATION)

project.lock.json: project.json
	dnu restore

deploy-pkg: $(PACKAGE)
	cp $(PACKAGE) $(NUGETSOURCE)

.PHONY: $(PACKAGE) $(BINTARGETPATH)


