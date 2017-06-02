DESTDIR=/srv/www/yavscpre
PRODDESTDIR=/srv/www/yavsc

default: pushInPre

watch:
	ASPNET_ENV=Development dnx-watch web --configuration=Debug

cleanRelease:
	rm -rf bin/Release

cleanoutput: 
	rm -rf bin/output

clean: cleanoutput cleanRelease
	gulp clean

bin/Release:
	dnu build --configuration=Release

bin/output: bin/Release
	dnu publish

bin/output/wwwroot/version: bin/output
	git log -1 --pretty=format:%h > bin/output/wwwroot/version

pushInPre: bin/output/wwwroot/version
	ssh root@localhost systemctl stop kestrel-pre
	ssh root@localhost rm -rf $(DESTDIR)/approot
	(cd bin/output && rsync -ravu ./ root@localhost:$(DESTDIR) >/dev/null)
	ssh root@localhost sync
	ssh root@localhost systemctl start kestrel-pre

push: bin/output/wwwroot/version
	ssh root@localhost systemctl stop kestrel
	ssh root@localhost rm -rf $(PRODDESTDIR)/approot
	(cd bin/output && rsync -ravu ./ root@localhost:$(PRODDESTDIR) >/dev/null)
	ssh root@localhost sync
	ssh root@localhost systemctl start kestrel
