
include common.mk

all: Yavsc

$(SUBDIRS):
	$(MAKE) -C $@ VERSION=$(VERSION)

Yavsc.Abstract:
	$(MAKE) -C Yavsc.Abstract VERSION=$(VERSION)

%-deploy-pkg:
	$(MAKE) -C $(basename $@ -deploy-pkg) deploy-pkg VERSION=$(VERSION)

Yavsc.Server: Yavsc.Abstract

Yavsc: Yavsc.Server Yavsc.Server-deploy-pkg
	make -C Yavsc restore
	make -C Yavsc VERSION=$(VERSION)

cli-deploy-pkg:
	make -C cli deploy-pkg VERSION=$(VERSION)

cli: Yavsc

%:
	make -C $@

memo:
		vim ~/TODO.md

.PHONY: all $(SUBDIRS)

