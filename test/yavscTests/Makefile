CONFIGURATION=Debug
BINTARGET=bin/$(CONFIGURATION)/dnx451/test.dll
SOURCE_DIR=../..
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/make
MSBUILD=msbuild
YAVSCSRC=../../src

all: test

include $(MAKEFILE_DIR)/dnx.mk

$(YAVSCSRC)/Yavsc/bin/$(CONFIGURATION)/dnx451/Yavsc.dll:
	make -C $(YAVSCSRC)/Yavsc

$(YAVSCSRC)/Yavsc.Abstract/bin/$(CONFIGURATION)/dnx451/Yavsc.Abstract.dll:
	make -C $(YAVSCSRC)/Yavsc.Abstract

$(YAVSCSRC)/Yavsc.Server/bin/$(CONFIGURATION)/dnx451/Yavsc.Server.dll:
	make -C $(YAVSCSRC)/Yavsc.Server

$(BINTARGET): project.lock.json $(YAVSCSRC)/Yavsc/bin/$(CONFIGURATION)/dnx451/Yavsc.dll $(YAVSCSRC)/Yavsc.Abstract/bin/$(CONFIGURATION)/dnx451/Yavsc.Abstract.dll $(YAVSCSRC)/Yavsc.Server/bin/$(CONFIGURATION)/dnx451/Yavsc.Server.dll
	dnu build --configuration $(CONFIGURATION)

non-regression: $(BINTARGET) 
	ASPNET_ENV=Development dnx test -maxthreads 1 -trait regression=non

all-tests: $(BINTARGET)
	ASPNET_ENV=Testing dnx test -maxthreads 1

regression: $(BINTARGET)
	ASPNET_ENV=Testing dnx test -maxthreads 1 -trait regression=oui

test: all-tests

testdev: $(BINTARGET) 
	ASPNET_ENV=Development dnx test -maxthreads 1 -trait dev=wip 

clean:
	rm -rf bin obj testingrepo

.PHONY: test
