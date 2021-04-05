# Common defs
#

ifndef PRJNAME
PRJNAME := $(shell basename `pwd -P`)
endif
FRAMEWORK=dnx451
ASPNET_ENV=Development
ASPNET_LOG_LEVEL=Debug
HOSTING=localhost
HOSTADMIN=root
FRAMEWORKALIAS=dnx451
# nuget package destination, at generation time
BINTARGET=$(PRJNAME).dll
BINTARGETPATH=bin/$(CONFIGURATION)/$(FRAMEWORKALIAS)/$(BINTARGET)
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
dnu=dnu
ifndef NUGETSOURCE
NUGETSOURCE=$(HOME)/Nupkgs
endif

# OBS SUBDIRS=Yavsc.Server Yavsc.Abstract Yavsc cli
#

# Git commit hash, in order to not publish some uncrontrolled code in production environment
#

git_status := $(shell git status -s --porcelain |wc -l)

all: $(BINTARGETPATH)

fixSystemXML:
	@# fixing package id reference case, to System.Xml, from package NJsonSchema.CodeGeneration.CSharp
	@sed 's/System.XML/System.Xml/' project.lock.json > project.lock.json.new && mv project.lock.json.new project.lock.json

restore:
	touch project.json
	$(dnu) restore --ignore-failed-sources

project.lock.json: project.json
	$(dnu) restore --ignore-failed-sources

watch: project.lock.json
	MONO_OPTIONS=--debug MONO_MANAGED_WATCHER=enabled ASPNET_ENV=$(ASPNET_ENV) ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) dnx-watch web --configuration=$(CONFIGURATION)

clean:
	rm -rf bin obj
	rm project.lock.json

cleanoutput:
	rm -rf bin/$(CONFIGURATION)
	rm -rf bin/output

$(BINTARGETPATH): project.lock.json rc-num.txt-check
	$(dnu) build --configuration=$(CONFIGURATION)

# Default target, from one level sub dirs

bin/output:
	@$(dnu) publish

bin/output/wwwroot/version: bin/output
	@echo $(version) > bin/output/wwwroot/version

pack: $(BINTARGETPATH) ../../version.txt
	@nuget pack $(PRJNAME).nuspec -Version $(VERSION) -Properties config=$(CONFIGURATION) -OutputDirectory bin 

push: pack
	@nuget push bin/$(PRJNAME).$(VERSION).nupkg $(NUGETSOURCEAPIKEY) -src $(NUGETSOURCE) 

.PHONY: rc-num.txt-check 

# .DEFAULT_GOAL := $(BINTARGETPATH)
