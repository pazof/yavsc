
MAKEFILE_DIR=../../scripts/make
BASERESX=Resources/Yavsc.Attributes.Validation.Resources.resx \
		 Resources/Yavsc.Models.Messaging.Resources.resx \
		 Resources/Yavsc.Models.IT.Fixing.Bug.resx\
		 Resources/Yavsc.ChatHubLabels.resx
BASERESXGEN=$(BASERESX:.resx=.Designer.cs)
include $(MAKEFILE_DIR)/dnx.mk
include $(MAKEFILE_DIR)/versioning.mk

all: $(BASERESXGEN) $(BINTARGETPATH)

%.Designer.cs: %.resx
	strongresbuildercli -l -p -t -r "Yavsc.Abstract.Resources." $^

prepare_code: $(BASERESXGEN)
