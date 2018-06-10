include versioning.mk

SUBDIRS=Yavsc Yavsc.Server Yavsc.Abstract cli

all: deploy-pkgs

Yavsc.Abstract:
	$(MAKE) -C Yavsc.Abstract VERSION=$(VERSION)

Yavsc.Server: Yavsc.Abstract
	$(MAKE) -C Yavsc.Server VERSION=$(VERSION)

Yavsc: 
	make -C Yavsc restore
	make -C Yavsc VERSION=$(VERSION)

Yavsc-deploy-pkg:
	make -C Yavsc deploy-pkg

Yavsc.Server-deploy-pkg:
	make -C Yavsc.Server deploy-pkg

Yavsc.Abstract-deploy-pkg: Yavsc
	make -C Yavsc.Abstract deploy-pkg

cli-deploy-pkg:
	make -C cli deploy-pkg

Yavsc.Abstract-deploy-pkg: Yavsc.Abstract

Yavsc-Server: Yavsc.Abstract-deploy-pkg

Yavsc.Server-deploy-pkg: Yavsc-Server

cli: Yavsc.Abstract-deploy-pkg Yavsc.Server-deploy-pkg
	make -C cli restore
	make -C cli

cli-deploy-pkg: cli

deploy-pkgs: Yavsc-deploy-pkg cli-deploy-pkg 

memo:
	vim ~/TODO.md


rc-num:
	@echo echo 1-alpha1  < $<  ^ $^  @ $@


.PHONY: all $(SUBDIRS)

