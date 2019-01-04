SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
include $(MAKEFILE_DIR)/versioning.mk
include $(MAKEFILE_DIR)/dnx.mk

MSBUILD=msbuild

all: $(BINTARGETPATH)

msbuild-restore:
	$(MSBUILD) cli.csproj /t:Restore

check: run

project.lock.json: 
	dnu restore
	@# fixing package id reference case, to System.Xml, from package NJsonSchema.CodeGeneration.CSharp
	sed 's/System.XML/System.Xml/' project.lock.json > project.lock.json.new && mv project.lock.json.new project.lock.json

run: $(BINTARGETPATH)
	ASPNET_ENV=$(ASPNET_ENV) dnx --configuration=$(CONFIGURATION) run send monthly

info:
	@echo $(PRJNAME)

# Due to NJsonSchema.CodeGeneration.CSharp package:
.PHONY: project.lock.json
