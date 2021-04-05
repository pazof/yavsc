SOURCE_DIR=../..
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/make
BASERESX=Resources/Yavsc.Models.Relationship.HyperLink.resx \
  Resources/Yavsc.Models.Streaming.LiveFlow.resx
BASERESXGEN=$(BASERESX:.resx=.Designer.cs)

include $(MAKEFILE_DIR)/dnx.mk
include $(MAKEFILE_DIR)/versioning.mk

default: all

$(BINTARGETPATH): ../OAuth.AspNet.AuthServer/bin/$(CONFIGURATION)/OAuth.AspNet.AuthServer.dll \
  ../Yavsc.Abstract/bin/$(CONFIGURATION)/Yavsc.Abstract.dll prepare_code

../OAuth.AspNet.AuthServer/bin/$(CONFIGURATION)/OAuth.AspNet.AuthServer.dll:
	make -C ../OAuth.AspNet.AuthServer 

../Yavsc.Abstract/bin/$(CONFIGURATION)/Yavsc.Abstract.dll:
	make -C ../Yavsc.Abstract

%.Designer.cs: %.resx
	strongresbuildercli -l -p -t -r "Yavsc.Server.Resources." $^

prepare_code: $(BASERESXGEN)

all: $(BINTARGETPATH) 
