include versioning.mk

SUBDIRS=Yavsc Yavsc.Server Yavsc.Abstract cli

all: deploy-pkgs

Yavsc.Abstract:
	$(MAKE) -C Yavsc.Abstract VERSION=$(VERSION)

Yavsc.Server: Yavsc.Abstract
	$(MAKE) -C Yavsc.Server VERSION=$(VERSION)

Yavsc: Yavsc.Server
	make -C Yavsc VERSION=$(VERSION)

Yavsc-deploy-pkg: Yavsc
	make -C Yavsc deploy-pkg

Yavsc.Server-deploy-pkg: Yavsc.Server
	make -C Yavsc.Server deploy-pkg

Yavsc.Abstract-deploy-pkg: Yavsc.Abstract
	make -C Yavsc.Abstract deploy-pkg

cli-deploy-pkg: cli check
	make -C cli deploy-pkg

cli: Yavsc-deploy-pkg Yavsc.Server-deploy-pkg Yavsc.Abstract-deploy-pkg
	make -C cli restore
	make -C cli

undoLocalYavscNugetDeploy:
	rm -rf ~/.dnx/packages/Yavsc.Abstract.$(VERSION).nupkg
	rm -rf ~/.dnx/packages/Yavsc.Server.$(VERSION).nupkg
	rm -rf ~/.dnx/packages/Yavsc.$(VERSION).nupkg

check: cli
	make -C cli check

deploy-pkgs: Yavsc-deploy-pkg Yavsc.Server-deploy-pkg Yavsc.Abstract-deploy-pkg cli-deploy-pkg

memo:
	vim ~/TODO.md

rc-num:
	@echo echo 1-alpha1  < $<  ^ $^  @ $@

.PHONY: all $(SUBDIRS)

