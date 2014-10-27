

CONFIG=Release
DESTDIR=build/web/$(CONFIG)
COPYUNCHANGED="false"

all: build deploy
	
ddir:
	mkdir -p $(DESTDIR)

deploy: ddir build
	xbuild /p:Configuration=$(CONFIG) /p:SkipCopyUnchangedFiles=$(COPYUNCHANGED) /p:DeployDir=../$(DESTDIR) /t:Deploy web/Web.csproj
	rm -rf $(DESTDIR)/obj

rsync: rsync-preprod rsync-local

build:
	xbuild /p:Configuration=$(CONFIG) /t:Build Yavsc.sln

clean:
	xbuild /t:Clean
	rm -rf $(DESTDIR)

rsync-preprod: deploy
	rsync -ravu build/web/$(CONFIG)/ root@lavieille.localdomain:/srv/httpd/luapre

rsync-local:
	rsync -ravu build/web/$(CONFIG)/ root@localhost:/srv/www/yavsc

sourcepkg:
	git archive --format=tar --prefix=yavsc-1.1/ 1.1 | bzip2 > yavsc-1.1.tar.bz2

