
VERSION=1.1
CONFIG=Debug
LDYDESTDIR=dist/web/$(CONFIG)
COPYUNCHANGED="false"
RSYNCCMD=rsync -ravu --chown=www-data:www-data
HOST_rsync_dev=totemdev.localdomain
DESTDIR_rsync_dev=/srv/www/totemdev
HOST_rsync_pre=totempre.localdomain
DESTDIR_rsync_pre=/srv/www/totempre
HOST_rsync_prod=totemprod.pschneider.fr
DESTDIR_rsync_prod=/srv/www/totemprod
DOCASSBS=NpgsqlBlogProvider.dll WorkFlowProvider.dll Yavsc.WebControls.dll ITContentProvider.dll NpgsqlMRPProviders.dll Yavsc.dll SalesCatalog.dll YavscModel.dll

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
	$(RSYNCCMD) dist/web/$(CONFIG)/ root@$(HOST):$(DESTDIR)
	ssh root@$(HOST) "service apache2 reload"

build: 
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

clean:
	xbuild /t:Clean
	find -name "StyleCop.Cache" -exec rm {} \;

distclean: clean
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

rsync_dev:

rsync_pre:

rsync_prod:

