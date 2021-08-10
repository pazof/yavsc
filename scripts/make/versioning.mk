
ifndef PRJNAME
PRJNAME := $(shell basename `pwd -P`)
endif
version := $(shell cat ../../version.txt)
MAKE=make
ISNSOURCE=$(HOME)/Nupkgs
VERSION=$(version)
CONFIGURATION=Debug

version-check:
ifndef version
	@echo no version number specification ... please, could you try and run 'echo 1.2.3 >  ../../version.txt' ?. 
else
	@echo 'Got version number : $(version)'
endif
