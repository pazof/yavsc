# Common defs
# assumes SOLUTIONDIR already defined
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
dnx=dnx

# OBS SUBDIRS=Yavsc.Server Yavsc.Abstract Yavsc cli
#

# Git commit hash, in order to not publish some uncrontrolled code in production environment
#

git_status := $(shell git status -s --porcelain |wc -l)

rc-num.txt-check:
ifndef rc_num
	@echo no rc num ... please, could you try and run 'make rc-num.txt' ?. 
else
	@echo 'Got rc num : $(rc_num)'
endif


restore:
	touch project.json
	$(dnu) restore

project.lock.json: project.json
	$(dnu) restore

watch: project.lock.json
	ASPNET_ENV=$(ASPNET_ENV) ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) dnx-watch web --configuration=$(CONFIGURATION)

clean:
	rm -rf bin obj

cleanoutput:
	rm -rf bin/$(CONFIGURATION)
	rm -rf bin/output

$(BINTARGETPATH): project.lock.json rc-num.txt-check
	$(dnu) build --configuration=$(CONFIGURATION)

# Default target, from one level sub dirs

bin/output:
	@$(dnu) publish

bin/output/wwwroot/version: bin/output
	@git log -1 --pretty=format:%h > bin/output/wwwroot/version

$(NUGETSOURCE)/$(PKGFILENAME): $(BINTARGETPATH) $(SOLUTIONDIR)/rc-num.txt
ifeq ($(git_status),0)
	nuget pack $(PRJNAME).nuspec -Version $(VERSION) -Properties config=$(CONFIGURATION) -OutputDirectory $(NUGETSOURCE)
else
	$(error Please, commit your changes before publishing your NuGet packages)
endif

deploy-pkg: $(NUGETSOURCE)/$(PKGFILENAME)

.PHONY: rc-num.txt-check $(BINTARGETPATH)

.DEFAULT_GOAL :=


