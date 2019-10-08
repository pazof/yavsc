SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
BASERESX=Resources/Yavsc.Attributes.Validation.Resources.resx \
		 Resources/Yavsc.Models.Messaging.Resources.resx \
		 Resources/Yavsc.Models.IT.Fixing.Bug.resx\
		 Resources/Yavsc.ChatHub.resx
BASERESXGEN=$(BASERESX:.resx=.Designer.cs)
include $(MAKEFILE_DIR)/versioning.mk
include $(MAKEFILE_DIR)/dnx.mk

all: $(BASERESXGEN) $(BINTARGETPATH)

%.Designer.cs: %.resx
	strotygen -l -p -t -r "Yavsc.Abstract.Resources." $^

prepare_code: $(BASERESXGEN)
