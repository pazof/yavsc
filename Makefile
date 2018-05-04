include versioning.mk

all: deploy-pkgs

Yavsc.Abstract:
	$(MAKE) -C Yavsc.Abstract VERSION=$(VERSION)

Yavsc.Server: Yavsc.Abstract
	$(MAKE) -C Yavsc.Server VERSION=$(VERSION)

Yavsc: Yavsc.Server
	make -C Yavsc restore
	make -C Yavsc VERSION=$(VERSION)

Yavsc.Server-deploy-pkg:
	make -C Yavsc.Server deploy-pkg

Yavsc.Abstract-deploy-pkg:
	make -C Yavsc.Abstract deploy-pkg

cli-deploy-pkg:
	make -C cli deploy-pkg

cli: Yavsc.Server-deploy-pkg Yavsc.Abstract-deploy-pkg
	make -C cli restore
	make -C cli

deploy-pkgs: cli-deploy-pkg

memo:
	vim ~/TODO.md


rc-num:
	@echo echo 1-alpha1  < $<  ^ $^  @ $@


.PHONY: all $(SUBDIRS)


