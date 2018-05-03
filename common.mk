# Common defs

MAKE=make
FRAMEWORK=dnx451
rc_num := $(shell cat rc-num.txt)
VERSION=1.0.5-rc$(rc_num)
CONFIGURATION=Release
PRJNAME := $(shell basename `pwd -P` .dll)
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
BINARY=bin/$(CONFIGURATION)/$(FRAMEWORK)/$(PRJNAME).dll
NUGETSOURCE=$(HOME)/Nupkgs/
ASPNET_ENV=Development
ASPNET_LOG_LEVEL=Debug
# nuget package destination, at generation time
HOSTING=localhost
HOSTADMIN=root
FRAMEWORKALIAS=dnx451
BINTARGET=$(PRJNAME).dll
BINTARGETPATH=bin/$(CONFIGURATION)/$(FRAMEWORKALIAS)/$(BINTARGET)

# OBS SUBDIRS=Yavsc.Server Yavsc.Abstract Yavsc cli
#

# Git commit hash, in order to not publish some uncrontrolled code in production environment
#

git_status := $(shell git status -s --porcelain |wc -l)

all: build

rc-num.txt: 
ifndef rc-num	
	@echo Generating rc-num.txt with: $(shell cat ../rc-num.txt || echo 1) > $@
	@echo $(shell cat ../rc-num.txt || echo 1) > $@
rc_num := $(shell cat rc-num.txt)
else 
	@echo 'Got rc num : $(rc_num)'	
endif

rc-num.txt-check: rc-num.txt
ifndef rc_num
	@echo no rc num ... please, run 'make rc-num.txt' before. 
else
	@echo 'Got rc num : $(rc_num)'
endif

restore:
	touch project.json
	dnu restore

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

$(BINTARGETPATH): project.lock.json
	dnu build --configuration=$(CONFIGURATION)

build: $(BINTARGETPATH)

bin/output:
	@dnu publish

bin/output/wwwroot/version: bin/output
	@git log -1 --pretty=format:%h > bin/output/wwwroot/version

$(PKGFILENAME): $(BINARY) rc-num.txt
	nuget pack $(PRJNAME).nuspec -Version $(VERSION) -Properties config=$(CONFIGURATION)

$(NUGETSOURCE)/$(PRJNAME):
	mkdir -P $^

deploy-pkg: $(PKGFILENAME)
	cp $(PKGFILENAME) $(NUGETSOURCE)

.PHONY: rc-num.txt-check
