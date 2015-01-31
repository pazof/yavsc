
VERSION=1.1
CONFIG=Debug
DESTDIR=build/web/$(CONFIG)
COPYUNCHANGED="false"
LOCALHOSTDIR=localhost:/srv/www/lua
TESTHOSTDIR=localhost:/srv/www/yavsc
PREPRODHOSTDIR=lua.localdomain:/srv/www/yavsc
PRODHOSTDIR=lua.localdomain:/srv/www/lua
DOCASSBS=NpgsqlBlogProvider.dll WorkFlowProvider.dll Yavsc.WebControls.dll ITContentProvider.dll NpgsqlMRPProviders.dll Yavsc.dll SalesCatalog.dll YavscModel.dll

all: deploy
	
ddir:
	mkdir -p $(DESTDIR)

deploy: ddir build
	xbuild /p:Configuration=$(CONFIG) /p:SkipCopyUnchangedFiles=$(COPYUNCHANGED) /p:DeployDir=../$(DESTDIR) /t:Deploy web/Web.csproj
	rm -rf $(DESTDIR)/obj
	mv $(DESTDIR)/Web.config $(DESTDIR)/Web.config.new
	 

rsync: rsync-preprod

build: 
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

clean:
	xbuild /t:Clean
	rm -rf $(DESTDIR)

rsync-local: deploy
	rsync -ravu build/web/$(CONFIG)/ root@$(LOCALHOSTDIR)

rsync-test: deploy
	rsync -ravu build/web/$(CONFIG)/ root@$(TESTHOSTDIR)

rsync-preprod: deploy
	rsync -ravu build/web/$(CONFIG)/ root@$(PREPRODHOSTDIR)

rsync-prod: deploy
	rsync -ravu build/web/$(CONFIG)/ root@$(PRODHOSTDIR)

rsync-all: rsync-local rsync-test rsync-preprod rsync-prod

sourcepkg:
	git archive --format=tar --prefix=yavsc-$(CONFIG)/ $(CONFIG) | bzip2 > yavsc-$(CONFIG).tar.bz2

debug: build
	(cd web; export MONO_OPTIONS=--debug; xsp4 --port 8080)

xmldoc: $(patsubst %,web/bin/%,$(DOCASSBS))
	mdoc-update $^ $(patsubst %.dll,-i%.xml,$^) --out web/xmldoc

htmldoc: xmldoc
	(cd web; monodocs2html -o htmldoc xmldoc)

docdeploy-prod: htmldoc
	rsync -ravu web/htmldoc root@$(PRODHOSTDIR)


