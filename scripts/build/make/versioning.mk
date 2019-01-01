
ifndef PRJNAME
PRJNAME := $(shell basename `pwd -P`)
endif
SOLUTIONDIR=$(HOME)/workspace/yavsc
rc_num := $(shell cat $(SOLUTIONDIR)/rc-num.txt)
MAKE=make
NUGETSOURCE=$(HOME)/Nupkgs
VERSION=1.0.5-rc$(rc_num)
CONFIGURATION=Debug
