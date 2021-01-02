SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/make
MSBUILD=msbuild

all: $(BINTARGETPATH)

include $(MAKEFILE_DIR)/dnx.mk

# makes version
include $(MAKEFILE_DIR)/versioning.mk

msbuild-restore:
	$(MSBUILD) $(PRJNAME).csproj /t:Restore

check: run

: project.lock.json
	@dnu build --configuration=$(CONFIGURATION) 

run: $(BINTARGETPATH)
	ASPNET_ENV=$(ASPNET_ENV) dnx --configuration=$(CONFIGURATION) run send monthly

info:
	@echo $(PRJNAME) : $(BINTARGETPATH) $(version)

# Due to NJsonSchema.CodeGeneration.CSharp package:
.PHONY: $(BINTARGETPATH)
