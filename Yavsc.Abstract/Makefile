CONFIGURATION=Release
PRJNAME=Yavsc.Abstract
BINTARGET=$(PRJNAME).dll
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
DESTPATH=.
PACKAGE=$(DESTPATH)/$(PKGFILENAME)
BINARY=bin/$(CONFIGURATION)//$(PRJNAME).dll
FRAMEWORKALIAS=net451
BINTARGETPATH=bin/$(CONFIGURATION)/$(FRAMEWORKALIAS)/$(BINTARGET)
NUGETSOURCE=$(HOME)/Nupkgs/

all: $(BINTARGETPATH)

$(BINTARGETPATH):
	dnu build


$(PACKAGE): $(BINARY)
	nuget pack $(PRJNAME).nuspec -Version $(VERSION) -Properties config=$(CONFIGURATION)

clean:
	rm $(PACKAGE)

$(BINARY): project.lock.json
	dnu build --configuration $(CONFIGURATION)

project.lock.json: project.json
	dnu restore

deploy-pkg: $(PACKAGE)
	cp $(PACKAGE) $(NUGETSOURCE)

.PHONY: $(PACKAGE)

