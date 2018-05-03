include ../common.mk


# Useless, defaults to the directory name.
#
PRJNAME=Yavsc.Server

$(PACKAGE): $(BINARY)
	nuget pack $(PRJNAME).nuspec -Version $(VERSION) -Properties config=$(CONFIG)

clean:
	rm $(PACKAGE)

$(BINARY): project.lock.json
	dnu build --configuration $(CONFIG)

project.lock.json: project.json
	dnu restore

deploy-pkg: $(PACKAGE)
	cp $(PACKAGE) $(NUGETSOURCE)

.PHONY: $(BINARY)
