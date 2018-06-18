include ../versioning.mk
include ../dnx.mk

MSBUILD=msbuild

all: $(BINTARGETPATH)

msbuild-restore:
	$(MSBUILD) cli.csproj /t:Restore

check: run

run: project.lock.json
	ASPNET_ENV=$(ASPNET_ENV) dnx run --configuration=$(CONFIGURATION)

info:
	@echo $(PRJNAME)




