
PRJNAME := $(shell basename `pwd -P`)
SOLUTIONDIR=$(HOME)/workspace/yavsc
rc_num := $(shell cat $(SOLUTIONDIR)/rc-num.txt)
DESTDIR=$(SOLUTIONDIR)/build
MAKE=make
VERSION=1.0.5-rc$(rc_num)
CONFIGURATION=Release
# nuget package destination, at generation time
NUGETSOURCE=$(HOME)/Nupkgs
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg


