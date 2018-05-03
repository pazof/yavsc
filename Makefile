

all: Yavsc

Yavsc.Abstract:
	$(MAKE) -C Yavsc.Abstract VERSION=$(VERSION)

Yavsc.Server: Yavsc.Abstract
	$(MAKE) -C Yavsc.Server VERSION=$(VERSION)

Yavsc: Yavsc.Server
	make -C Yavsc restore
	make -C Yavsc VERSION=$(VERSION)

cli-deploy-pkg:
	make -C cli deploy-pkg VERSION=$(VERSION)

cli: Yavsc

memo:
		vim ~/TODO.md

.PHONY: all $(SUBDIRS)

