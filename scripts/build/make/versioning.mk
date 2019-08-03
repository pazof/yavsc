
ifndef PRJNAME
PRJNAME := $(shell basename `pwd -P`)
endif
SOLUTIONDIR=$(HOME)/workspace/yavsc
version := $(shell cat $(SOLUTIONDIR)/version.txt)
MAKE=make
NUGETSOURCE=$(HOME)/Nupkgs
VERSION=$(version)
CONFIGURATION=Debug

version-check:
ifndef version
	@echo no version number specification ... please, could you try and run 'echo 1.2.3 > $(SOLUTIONDIR)version.txt' ?. 
else
	@echo 'Got version number : $(version)'
endif


