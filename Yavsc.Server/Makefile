CONFIG=Release
PRJNAME=Yavsc.Server

PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
DESTPATH=.
PACKAGE=$(DESTPATH)/$(PKGFILENAME)
BINARY=bin/$(CONFIG)/net451/$(PRJNAME).dll
NUGETSOURCE=$(HOME)/Nupkgs/

$(PACKAGE): $(BINARY)
	nuget pack $(PRJNAME).nuspec -Version $(VERSION) -Properties config=$(CONFIG)

clean:
	rm $(PACKAGE)

$(BINARY): project.lock.json
	dnu build --configuration $(CONFIG)

project.lock.json: project.json
	dnu restore

deploy: $(PACKAGE)
	cp $(PACKAGE) $(NUGETSOURCE)
