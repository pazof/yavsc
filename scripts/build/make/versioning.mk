
ifndef PRJNAME
PRJNAME := $(shell basename `pwd -P`)
endif
SOLUTIONDIR=$(HOME)/workspace/yavsc
version := $(shell cat $(SOLUTIONDIR)/version.txt)
MAKE=make
NUGETSOURCE=$(HOME)/Nupkgs
VERSION=$(version)
CONFIGURATION=Debug
