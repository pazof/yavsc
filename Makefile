

MAKE=make
SUBDIRS=Yavsc.Abstract Yavsc cli
git_status := $(shell git status -s --porcelain |wc -l)
rc_num := $(shell cat rc-num.txt)
VERSION=1.0.5-rc$(rc_num)

all: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@ VERSION=$(VERSION)

Yavsc.Abstract-deploy:
	$(MAKE) -C Yavsc.Abstract deploy-pkg VERSION=$(VERSION)

Yavsc.Abstract-deploy: Yavsc
	$(MAKE) -C Yavsc deploy-pkg VERSION=$(VERSION)

Yavsc: Yavsc.Abstract-deploy
	make -C Yavsc restore

cli: Yavsc

.PHONY: all $(SUBDIRS)

