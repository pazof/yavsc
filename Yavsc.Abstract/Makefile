CONFIG=Release
VERSION=1.0.2
PRJNAME=Yavsc.Abstract
PKGFILENAME=$(PRJNAME).$(VERSION).nupkg
DESTPATH=.
PACKAGE=$(DESTPATH)/$(PKGFILENAME)

$(PACKAGE):
	nuget pack $(PRJNAME).nuspec -Version $(VERSION)

publish: $(PACKAGE)
	cp $(PACKAGE) ~/Nupkgs

clean:
	rm $(PACKAGE)


