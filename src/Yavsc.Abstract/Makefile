SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
BASERESX=Resources/Yavsc.Attributes.Validation.Resources.resx
BASERESXGEN=$(BASERESX:.resx=.Designer.cs)
include $(MAKEFILE_DIR)/versioning.mk
include $(MAKEFILE_DIR)/dnx.mk

all: $(BINTARGETPATH)

%.Designer.cs: %.resx
	strotygen -l -p -t -r "Yavsc.Abstract.Resources." $^

prepare_code: $(BASERESXGEN)
