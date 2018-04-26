
MAKE=make
SUBDIRS=Yavsc.Abstract Yavsc cli
VERSION=1.0.5-rc11

all: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@ VERSION=$(VERSION)

deploy-pkgs:
	$(MAKE) -C Yavsc.Abstract deploy VERSION=$(VERSION)
	$(MAKE) -C Yavsc deploy-pkg VERSION=$(VERSION)

.PHONY: all $(SUBDIRS)

