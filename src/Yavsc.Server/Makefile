SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
include $(MAKEFILE_DIR)/versioning.mk
include $(MAKEFILE_DIR)/dnx.mk

all: $(BINTARGETPATH)

$(BINTARGETPATH): ../OAuth.AspNet.AuthServer/bin/$(CONFIGURATION)/OAuth.AspNet.AuthServer.dll ../Yavsc.Abstract/bin/$(CONFIGURATION)/Yavsc.Abstract.dll 

../OAuth.AspNet.AuthServer/bin/$(CONFIGURATION)/OAuth.AspNet.AuthServer.dll:
	make -C ../OAuth.AspNet.AuthServer 

../Yavsc.Abstract/bin/$(CONFIGURATION)/Yavsc.Abstract.dll:
	make -C ../Yavsc.Abstract

