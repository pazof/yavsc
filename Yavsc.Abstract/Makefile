CONFIG=Release
VERSION=1.0.5-rc5
PRJNAME=Yavsc.Abstract
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
DESTPATH=.
PACKAGE=$(DESTPATH)/$(PKGFILENAME)
BINARY=bin/$(CONFIG)/net45/Yavsc.Abstract.dll
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
