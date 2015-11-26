
VERSION=1.1
CONFIG=Debug
LDYDESTDIR=dist/web/$(CONFIG)
COPYUNCHANGED="false"

HOST_rsync_dev=lua.pschneider.fr
DESTDIR_rsync_dev=/srv/www/yavscdev

HOST_rsync_pre=lua.pschneider.fr
DESTDIR_rsync_pre=/srv/www/yavscpre

HOST_rsync_prod=lua.pschneider.fr
DESTDIR_rsync_prod=/srv/www/yavsc

HOST_rsync_yavsc=lua.pschneider.fr
DESTDIR_rsync_yavsc=/srv/www/yavsc

HOST_rsync_lua=lua.pschneider.fr
DESTDIR_rsync_lua=/srv/www/lua

HOST_rsync_totemdev=lua.pschneider.fr
DESTDIR_rsync_totemdev=/srv/www/totemdev

HOST_rsync_totempre=lua.pschneider.fr
DESTDIR_rsync_totempre=/srv/www/totempre

HOST_rsync_totemprod=lua.pschneider.fr
DESTDIR_rsync_totemprod=/srv/www/totemprod

DOCASSBS=NpgsqlBlogProvider.dll WorkFlowProvider.dll Yavsc.WebControls.dll ITContentProvider.dll NpgsqlMRPProviders.dll Yavsc.dll SalesCatalog.dll YavscModel.dll

RSYNCCMD=rsync -ravu --chown=www-data:www-data

all: deploy

ddir:
	mkdir -p $(LDYDESTDIR)

deploy: ddir build
	rm -rf $(LYDESTDIR)
	xbuild /p:Configuration=$(CONFIG) /p:SkipCopyUnchangedFiles=$(COPYUNCHANGED) /p:DeployDir=../$(LDYDESTDIR) /t:Deploy web/Yavsc.csproj

rsync_% : HOST = $(HOST_$@)

rsync_% : DESTDIR = $(DESTDIR_$@)

rsync_% : deploy
	echo "!Deploying to $(HOST) using $(CONFIG) config!"
	$(RSYNCCMD) dist/web/$(CONFIG)/ root@$(HOST):$(DESTDIR)

build: 
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

clean:
	xbuild /p:Configuration=$(CONFIG)  /t:Clean Yavsc.sln
	find -name "StyleCop.Cache" -exec rm {} \;

distclean: clean
	rm -rf $(LDYDESTDIR)

sourcepkg:
	git archive --format=tar --prefix=yavsc-$(CONFIG)/ $(CONFIG) | bzip2 > yavsc-$(CONFIG).tar.bz2

start_xsp: build
	(cd web; export MONO_OPTIONS=--debug; xsp4 --port 8080)

xmldoc: $(patsubst %,web/bin/%,$(DOCASSBS))
	mdoc-update $^ $(patsubst %.dll,-i%.xml,$^) --out web/xmldoc

htmldoc: xmldoc
	(cd web; monodocs2html -o htmldoc xmldoc)

docdeploy-prod: htmldoc
	rsync -ravu web/htmldoc root@$(PRODHOSTDIR)

rsync_lua: CONFIG = Lua

rsync_pre: CONFIG = YavscPre

rsync_prod: CONFIG = Yavsc

rsync_totempre: CONFIG = TotemPre

rsync_totemprod: CONFIG = TotemProd

nuget_restore:
	for prj in ITContentProvider NpgsqlBlogProvider NpgsqlContentProvider NpgsqlMRPProviders Presta SalesCatalog TestAPI web WebControls yavscclient yavscModel; do nuget restore "$${prj}/packages.config" -SolutionDirectory . ; done

nuget_update:
	for prj in ITContentProvider NpgsqlBlogProvider NpgsqlContentProvider NpgsqlMRPProviders Presta SalesCatalog TestAPI web WebControls yavscclient yavscModel; do nuget update "$${prj}/packages.config"  ; done



