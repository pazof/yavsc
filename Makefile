
MAKE=make
SUBDIRS=Yavsc.Abstract Yavsc cli
VERSION=1.0.5-rc12

all: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@ VERSION=$(VERSION)

deploy-pkgs:
	$(MAKE) -C Yavsc.Abstract deploy-pkg VERSION=$(VERSION)
	$(MAKE) -C Yavsc deploy-pkg VERSION=$(VERSION)

.PHONY: all $(SUBDIRS)

