
VERSION=1.1
CONFIG=Debug
LDYDESTDIR=build/web/$(CONFIG)
COPYUNCHANGED="false"

HOST_rsync_local=localhost
DESTDIR_rsync_local=/srv/www/yavsc

HOST_rsync_test=localhost
DESTDIR_rsync_test=/srv/www/lua

HOST_rsync_pre=lua.localdomain
DESTDIR_rsync_pre=/srv/www/yavsc

HOST_rsync_prod=lua.localdomain
DESTDIR_rsync_prod=/srv/www/lua

DOCASSBS=NpgsqlBlogProvider.dll WorkFlowProvider.dll Yavsc.WebControls.dll ITContentProvider.dll NpgsqlMRPProviders.dll Yavsc.dll SalesCatalog.dll YavscModel.dll

RSYNCCMD=rsync -ravu --chown=www-data:www-data

all: deploy

ddir:
	mkdir -p $(LDYDESTDIR)

deploy: ddir build
	rm -rf $(LYDESTDIR)
	xbuild /p:Configuration=$(CONFIG) /p:SkipCopyUnchangedFiles=$(COPYUNCHANGED) /p:DeployDir=../$(LDYDESTDIR) /t:Deploy web/Web.csproj
	mv $(LDYDESTDIR)/Web.config $(LDYDESTDIR)/Web.config.new


rsync_% : HOST = $(HOST_$@)

rsync_% : DESTDIR = $(DESTDIR_$@)

rsync_% : deploy
	echo "!Deploying to $(HOST)!"
	$(RSYNCCMD) build/web/$(CONFIG)/ root@$(HOST):$(DESTDIR)
	ssh root@$(HOST) apachectl restart

build: 
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

clean:
	xbuild /t:Clean
	find -name "StyleCop.Cache" -exec rm {} \;
	rm -rf $(LDYDESTDIR)

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

rsync_local:

rsync_test:

rsync_pre:

rsync_prod:

bigrsync: rsync_test rsync_local rsync_pre rsync_prod


