
VERSION=1.1
CONFIG=Debug
DESTDIR=build/web/$(CONFIG)
COPYUNCHANGED="false"

HOST_rsync_local=localhost
DESTDIR_rsync_local=/srv/www/yavsc

HOST_rsync_test=localhost
DESTDIR_rsync_test=/srv/www/lua

HOST_rsync_preprod=lua.localdomain
DESTDIR_rsync_preprod=/srv/www/yavsc

HOST_rsync_prod=lua.localdomain
DESTDIR_rsync_prod=/srv/www/lua

DOCASSBS=NpgsqlBlogProvider.dll WorkFlowProvider.dll Yavsc.WebControls.dll ITContentProvider.dll NpgsqlMRPProviders.dll Yavsc.dll SalesCatalog.dll YavscModel.dll

RSYNCCMD=rsync -ravu --chown=www-data:www-data

all: deploy

ddir:
	mkdir -p $(DESTDIR)

deploy: ddir build
	rm -rf $(DESTDIR)
	xbuild /p:Configuration=$(CONFIG) /p:SkipCopyUnchangedFiles=$(COPYUNCHANGED) /p:DeployDir=../$(DESTDIR) /t:Deploy web/Web.csproj
	mv $(DESTDIR)/Web.config $(DESTDIR)/Web.config.new


rsync_% : HOST = $(HOST_$@)

rsync_% : DESTDIR = $(DESTDIR_$@)

rsync_% : 
	echo "!Deploying to $(HOST)!"
	$(RSYNCCMD) build/web/$(CONFIG)/ root@$(HOST):$(DESTDIR)
	ssh root@$(HOST) apachectl restart

rsync: rsync_test

build: 
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

clean:
	xbuild /t:Clean
	rm -rf $(DESTDIR)


allrsync: rsync_local rsync_test rsync_preprod rsync_prod

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


