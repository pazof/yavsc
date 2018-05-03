include common.mk

$(BINTARGETPATH): 
	dnu build

build: project.
	$(DNU) build


