
MAKE=make
SUBDIRS=Yavsc.Abstract Yavsc.Server Yavsc
VERSION=1.0.5-rc10

all: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@ VERSION=$(VERSION)

deploy:
	$(MAKE) -C Yavsc.Abstract deploy VERSION=$(VERSION)
	$(MAKE) -C Yavsc deploy-pkg VERSION=$(VERSION)

.PHONY: all $(SUBDIRS)

