
VERSION=1.1
CONFIG=Debug
DESTDIR=build/web/$(CONFIG)
COPYUNCHANGED="false"

all: deploy
	
ddir:
	mkdir -p $(DESTDIR)

deploy: ddir build
	xbuild /p:Configuration=$(CONFIG) /p:SkipCopyUnchangedFiles=$(COPYUNCHANGED) /p:DeployDir=../$(DESTDIR) /t:Deploy web/Web.csproj
	rm -rf $(DESTDIR)/obj
	mv $(DESTDIR)/Web.config $(DESTDIR)/Web.config.new

rsync: rsync-preprod rsync-local

build:
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

clean:
	xbuild /t:Clean
	rm -rf $(DESTDIR)

rsync-preprod: deploy
	rsync -ravu build/web/$(CONFIG)/ root@lua.localdomain:/srv/httpd/luapre

rsync-local:
	rsync -ravu build/web/$(CONFIG)/ root@localhost:/srv/www/yavsc

sourcepkg:
	git archive --format=tar --prefix=yavsc-$(CONFIG)/ $(CONFIG) | bzip2 > yavsc-$(CONFIG).tar.bz2

debug: build
	(cd web; export MONO_OPTIONS=--debug; xsp4 --port 8080)

