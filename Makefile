
VERSION=1.1
CONFIG=Debug
LDYDESTDIR=dist/web/$(CONFIG)
COPYUNCHANGED="false"
SHELL=/bin/bash


HOST_rsync_Debug=lua.pschneider.fr
DESTDIR_rsync_Debug=/srv/www/yavscdev

HOST_rsync_Release=lua.pschneider.fr
DESTDIR_rsync_Release=/srv/www/yavscpre

HOST_rsync_Lua=lua.pschneider.fr
DESTDIR_rsync_Lua=/srv/www/lua

HOST_rsync_YavscPre=lua.pschneider.fr
DESTDIR_rsync_YavscPre=/srv/www/yavscpre

HOST_rsync_Yavsc=lua.pschneider.fr
DESTDIR_rsync_Yavsc=/srv/www/yavsc

HOST_rsync_TotemPre=lua.pschneider.fr
DESTDIR_rsync_TotemPre=/srv/www/totempre

HOST_rsync_TotemProd=lua.pschneider.fr
DESTDIR_rsync_TotemProd=/srv/www/totemprod

DOCASSBS=NpgsqlBlogProvider.dll WorkFlowProvider.dll Yavsc.WebControls.dll ITContentProvider.dll NpgsqlMRPProviders.dll Yavsc.dll SalesCatalog.dll YavscModel.dll

RSYNCCMD=rsync -ravu --chown=www-data:www-data

all: deploy

ddir:
	mkdir -p $(LDYDESTDIR)

.PHONY: build
build : 
	echo "Building $(CONFIG) ..."
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

.PHONY: deploy
deploy: ddir build
	rm -rf $(LYDESTDIR)
	xbuild /p:Configuration=$(CONFIG) /p:SkipCopyUnchangedFiles=$(COPYUNCHANGED) /p:DeployDir=../$(LDYDESTDIR) /t:Deploy web/Yavsc.csproj


build_%: CONFIG = "$(subst build_,,$@)" 

build_%: echo "Building $(CONFIG) ..."
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

rsync_% : CONFIG = $(subst rsync_,,$@)

rsync_% : HOST = $(HOST_rsync_$(CONFIG))

rsync_% : DESTDIR = $(DESTDIR_rsync_$(CONFIG))


rsync_% : 
	make deploy CONFIG=$(CONFIG)
	if [[ "x$(HOST)" == "x" ]]; then echo "no host given, aborting"; exit 1; fi
	echo "!Deploying to $(HOST) using $(CONFIG) config!"
	$(RSYNCCMD) dist/web/$(CONFIG)/ root@$(HOST):$(DESTDIR)


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

rsync_Lua: 

rsync_Debug: 

rsync_Release: 

rsync_YavscPre:

rsync_yavsc:

rsync_TotemProd:

nuget_restore:
	for prj in ITContentProvider NpgsqlBlogProvider NpgsqlContentProvider NpgsqlMRPProviders Presta SalesCatalog TestAPI web WebControls yavscclient yavscModel; do nuget restore "$${prj}/packages.config" -SolutionDirectory . ; done

nuget_update:
	for prj in ITContentProvider NpgsqlBlogProvider NpgsqlContentProvider NpgsqlMRPProviders Presta SalesCatalog TestAPI web WebControls yavscclient yavscModel; do nuget update "$${prj}/packages.config"  ; done

syncall: rsync_Debug rsync_Release rsync_YavscPre rsync_Lua rsync_Yavsc rsync_TotemPre rsync_TotemProd





