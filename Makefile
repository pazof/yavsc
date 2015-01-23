
VERSION=1.1
CONFIG=Debug
DESTDIR=build/web/$(CONFIG)
COPYUNCHANGED="false"
PREPRODHOSTDIR=localhost:/srv/www/yavsc
PRODHOSTDIR=localhost:/srv/www/lua
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

rsync-preprod: deploy
	rsync -ravu build/web/$(CONFIG)/ root@$(PREPRODHOSTDIR)

rsync-prod: deploy
	rsync -ravu build/web/$(CONFIG)/ root@$(PRODHOSTDIR)

sourcepkg:
	git archive --format=tar --prefix=yavsc-$(CONFIG)/ $(CONFIG) | bzip2 > yavsc-$(CONFIG).tar.bz2

debug: build
	(cd web; export MONO_OPTIONS=--debug; xsp4 --port 8080)

