SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
include $(MAKEFILE_DIR)/versioning.mk
include $(MAKEFILE_DIR)/dnx.mk

default: all

$(BINTARGETPATH): ../OAuth.AspNet.AuthServer/bin/$(CONFIGURATION)/OAuth.AspNet.AuthServer.dll ../Yavsc.Abstract/bin/$(CONFIGURATION)/Yavsc.Abstract.dll prepare_code

../OAuth.AspNet.AuthServer/bin/$(CONFIGURATION)/OAuth.AspNet.AuthServer.dll:
	make -C ../OAuth.AspNet.AuthServer 

../Yavsc.Abstract/bin/$(CONFIGURATION)/Yavsc.Abstract.dll:
	make -C ../Yavsc.Abstract

Resources/Yavsc.Models.IT.Fixing.Bug.Designer.cs: Resources/Yavsc.Models.IT.Fixing.Bug.resx
	strotygen -p -t -l -r "Yavsc.Server.Resources." $^

prepare_code: Resources/Yavsc.Models.IT.Fixing.Bug.Designer.cs

all: $(BINTARGETPATH)
