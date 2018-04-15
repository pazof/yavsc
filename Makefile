
MAKE=make
SUBDIRS=Yavsc.Abstract Yavsc.Server Yavsc
VERSION=1.0.5-rc10

all: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@ VERSION=$(VERSION)

deploy:
	$(MAKE) -C Yavsc.Abstract deploy
	$(MAKE) -C Yavsc deploy

.PHONY: all $(SUBDIRS)

