include ../versioning.mk
include ../dnx.mk

MSBUILD=msbuild

all: $(BINTARGETPATH)

msbuild-restore:
	$(MSBUILD) cli.csproj /t:Restore

check: run

project.lock.json: project.json
	$(dnu) restore
	sed 's/System.XML/System.Xml/' project.lock.json > project.lock.json.new && mv project.lock.json.new project.lock.json

run: project.lock.json
	ASPNET_ENV=$(ASPNET_ENV) dnx run --configuration=$(CONFIGURATION)

info:
	@echo $(PRJNAME)




