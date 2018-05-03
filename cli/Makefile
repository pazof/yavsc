include ../common.mk

MSBUILD=msbuild


all: build

build: project.lock.json
	ASPNET_ENV=$(ASPNET_ENV) ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) dnu build --configuration=$(CONFIGURATION)

restore:
	touch project.json
	dnu restore

msbuild-restore:
	$(MSBUILD) cli.csproj /t:Restore

project.lock.json: project.json
	dnu restore

run: project.lock.json
	ASPNET_ENV=Development ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) dnx run --configuration=$(CONFIGURATION)

deploy-pkg: $(PACKAGE)
	cp $(PACKAGE) $(NUGETSOURCE)

.PHONY: $(BINARY)

