
MAKE=make
SUBDIRS=Yavsc.Abstract Yavsc

all: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@

deploy:
	$(MAKE) -C Yavsc.Abstract deploy
	$(MAKE) -C Yavsc deploy

.PHONY: all $(SUBDIRS)


