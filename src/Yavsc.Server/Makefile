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

%.Designer.cs: Resources/%.resx
	strotygen -l -p -t -r "Yavsc.Server.Resources." $^

prepare_code: Resources/Yavsc.Models.IT.Fixing.Bug.Designer.cs Yavsc.Models.Relationship.HyperLink.Designer.cs

all: $(BINTARGETPATH)
