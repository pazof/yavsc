

MSBUILD=msbuild
MONO=mono
CONFIGURATION=Debug
BINTYPE=exe
PRJNAME := $(shell basename `pwd -P`)

rc_num := $(shell cat ../../rc-num.txt)
VERSION=1.0.5-rc$(rc_num)

BINTARGET=$(PRJNAME).$(BINTYPE)
BINTARGETPATH=bin/$(CONFIGURATION)/$(BINTARGET)
NUGETSOURCE=$(HOME)/Nupkgs
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg

