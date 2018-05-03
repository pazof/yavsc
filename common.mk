# Common defs

MAKE=make
FRAMEWORK=dnx451
rc_num := $(shell cat rc-num.txt)
VERSION=1.0.5-rc$(rc_num)
CONFIGURATION=Release
PRJNAME := $(shell basename `pwd -P` .dll)
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
PACKAGE=$(DESTPATH)/$(PKGFILENAME)
BINARY=bin/$(CONFIG)/$(FRAMEWORK)/$(PRJNAME).dll
NUGETSOURCE=$(HOME)/Nupkgs/
ASPNET_ENV=Development
ASPNET_LOG_LEVEL=Debug
# nuget package destination, at generationi time
DESTPATH=.

# OBS SUBDIRS=Yavsc.Server Yavsc.Abstract Yavsc cli
#

# Git commit hash, in order to not publish some uncrontrolled code in production environment
#

git_status := $(shell git status -s --porcelain |wc -l)

all: build

rc-num.txt: 
ifndef rc-num	
	@echo $(shell cat ../rc-num.txt || echo 1) > $@
else 
	@echo 'Got rc num : $(rc_num)'	
endif

rc-num.txt-check: rc-num.txt
ifndef rc_num
	@echo no rc num ... please, run 'make reinit-rc-num.txt' before. 
else
	@echo 'Got rc num : $(rc_num)'
endif

clean-rc-num.txt:
	rm rc-num.txt	

reinit-rc-num.txt: clean-rc-num.txt rc-num.txt

.PHONY: rc-num.txt-check

