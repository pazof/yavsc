CONFIGURATION=Debug
BINTARGET=bin/$(CONFIGURATION)/dnx451/test.dll
SOURCE_DIR=../..
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/make
MSBUILD=msbuild

all: test

include $(MAKEFILE_DIR)/dnx.mk

../Yavsc/bin/$(CONFIGURATION)/dnx451/Yavsc.dll:
	make -C ../Yavsc

../Yavsc.Abstract/bin/$(CONFIGURATION)/dnx451/Yavsc.Abstract.dll:
	make -C ../Yavsc.Abstract

../Yavsc.Server/bin/$(CONFIGURATION)/dnx451/Yavsc.Server.dll:
	make -C ../Yavsc.Server

$(BINTARGET): project.lock.json ../Yavsc/bin/$(CONFIGURATION)/dnx451/Yavsc.dll ../Yavsc.Abstract/bin/$(CONFIGURATION)/dnx451/Yavsc.Abstract.dll ../Yavsc.Server/bin/$(CONFIGURATION)/dnx451/Yavsc.Server.dll
	dnu build --configuration $(CONFIGURATION)

non-regression: $(BINTARGET) 
	ASPNET_ENV=Development dnx test -maxthreads 1 -trait regression=non

regression: $(BINTARGET)
	ASPNET_ENV=Development dnx test -maxthreads 1 -trait regression=oui

test: non-regression

testdev: $(BINTARGET) 
	ASPNET_ENV=Development dnx test -maxthreads 1 -trait dev=wip 

clean:
	rm -rf bin obj testingrepo

.PHONY: test
