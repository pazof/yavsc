
MAKE=make
SUBDIRS=Yavsc.Abstract Yavsc

all: $(SUBDIRS)

$(SUBDIRS):
	$(MAKE) -C $@

.PHONY: all $(SUBDIRS)

