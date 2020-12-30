include versioning.mk


REPO_ROOT=../../../src

SUBDIRS=Yavsc Yavsc.Server Yavsc.Abstract OAuth.AspNet.AuthServer OAuth.AspNet.Token cli test

all: $(SUBDIRS)

Yavsc.Abstract:
	$(MAKE) -C $(REPO_ROOT)/Yavsc.Abstract VERSION=$(VERSION)

OAuth.AspNet.Token:
	$(MAKE) -C $(REPO_ROOT)/OAuth.AspNet.Token VERSION=$(VERSION)

OAuth.AspNet.AuthServer: OAuth.AspNet.Token
	$(MAKE) -C $(REPO_ROOT)/OAuth.AspNet.AuthServer VERSION=$(VERSION)

Yavsc.Server: Yavsc.Abstract
	$(MAKE) -C $(REPO_ROOT)/Yavsc.Server VERSION=$(VERSION)

Yavsc: Yavsc.Server OAuth.AspNet.AuthServer OAuth.AspNet.Token
	make -C $(REPO_ROOT)/Yavsc VERSION=$(VERSION)

Yavsc-deploy-pkg: Yavsc
	make -C $(REPO_ROOT)/Yavsc deploy-pkg

Yavsc.Server-deploy-pkg: Yavsc.Server
	make -C $(REPO_ROOT)/Yavsc.Server deploy-pkg

Yavsc.Abstract-deploy-pkg: Yavsc.Abstract
	make -C $(REPO_ROOT)/Yavsc.Abstract deploy-pkg

cli-deploy-pkg: cli check
	make -C $(REPO_ROOT)/cli deploy-pkg

cli: Yavsc-deploy-pkg Yavsc.Server-deploy-pkg Yavsc.Abstract-deploy-pkg
	make -C $(REPO_ROOT)/cli 

undoLocalYavscNugetDeploy:
	rm -rf ../../../packages/Yavsc.Abstract
	rm -rf ../../../packages/Yavsc.Server
	rm -rf ../../../packages/Yavsc
	rm -rf ~/.dnx/packages/Yavsc.Abstract
	rm -rf ~/.dnx/packages/Yavsc.Server
	rm -rf ~/.dnx/packages/Yavsc

check: cli
	make -C $(REPO_ROOT)/cli check
	make -C $(REPO_ROOT)/test 

test: 
	make -C $(REPO_ROOT)/test

pushInPre: 
	make -C $(REPO_ROOT)/Yavsc pushInPre

pushInProd:
	make -C $(REPO_ROOT)/Yavsc pushInProd

deploy-pkgs: Yavsc-deploy-pkg Yavsc.Server-deploy-pkg Yavsc.Abstract-deploy-pkg cli-deploy-pkg

memo:
	vim ~/TODO.md

rc-num:
	@echo echo 1-alpha1  < $<  ^ $^  @ $@

clean: 
	for subdir in $(SUBDIRS) ; do \
		make -C $(REPO_ROOT)/$${subdir} clean ; \
	done

watch:
	make -C $(REPO_ROOT)/Yavsc watch

.PHONY: all $(SUBDIRS)

