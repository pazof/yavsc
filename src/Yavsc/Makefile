DESTDIR=/srv/www/yavscpre
PRODDESTDIR=/srv/www/yavsc
HOSTING=localhost
HOSTADMIN=root
ASPNET_LOG_LEVEL=debug
SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
BASERESXGEN=Resources/Yavsc.ChatHub.Designer.cs Resources/Yavsc.ViewComponents.CommentViewComponent.Designer.cs Resources/Yavsc.ViewModels.FrontOffice.PerformerProfileViewModel.Designer.cs Resources/Yavsc.YavscLocalisation.Designer.cs 
include $(MAKEFILE_DIR)/versioning.mk
include $(MAKEFILE_DIR)/dnx.mk

MINJS=wwwroot/js/bootstrap.min.js      wwwroot/js/input-lib.min.js             wwwroot/js/jquery.signalR-2.2.1.min.js  wwwroot/js/quill.min.js wwwroot/js/chat.min.js           wwwroot/js/jquery-2.2.4.min.js          wwwroot/js/jquery-ui.min.js             wwwroot/js/showdown.min.js wwwroot/js/comment.min.js        wwwroot/js/jquery-3.2.1.min.js          wwwroot/js/md-helpers.min.js            wwwroot/js/site.min.js wwwroot/js/dropzone.min.js       wwwroot/js/jquery-migrate-3.0.0.min.js  wwwroot/js/parallax.min.js              wwwroot/js/str.min.js wwwroot/js/google-geoloc.min.js  wwwroot/js/jquery.min.js                wwwroot/js/paypalbutton.min.js          wwwroot/js/to-markdown.min.js

MINCSS=wwwroot/css/coiffure.min.css  wwwroot/css/dev.min.css           wwwroot/css/freespeech.min.css  wwwroot/css/yavsc.min.css wwwroot/css/default.min.css   wwwroot/css/font-awesome.min.css  wwwroot/css/lua.min.css         wwwroot/css/yavscpre.min.css wwwroot/css/clear/site.min.css  wwwroot/css/main/bootstrap.min.css  wwwroot/css/main/jquery-ui.min.css   wwwroot/css/main/site.min.css wwwroot/css/dark/site.min.css   wwwroot/css/main/dropzone.min.css   wwwroot/css/main/quill.snow.min.css

web: project.lock.json
	ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) ASPNET_ENV=$(ASPNET_ENV) dnx web --configuration=$(CONFIGURATION)

status:
ifeq ($(git_status),0)
		@echo Nothing to be done.
else
    @echo There are pending changes:
		@git status
endif

publish:
	ASPNET_ENV=$(ASPNET_ENV) dnu publish

deploy-pkg: publish

deploy: pushInPre pushInProd

pushInPre: cleanoutput bin/output/wwwroot/version
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel-pre stop
	ssh $(HOSTADMIN)@$(HOSTING) sudo rm -rf $(DESTDIR)/approot
	(cd bin/output && rsync -ravu ./ $(HOSTADMIN)@$(HOSTING):$(DESTDIR) >/dev/null)
	ssh $(HOSTADMIN)@$(HOSTING) sudo sync
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel-pre start

pushInProd: cleanoutput bin/output/wwwroot/version
ifeq ($(git_status),0)
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel stop
	ssh $(HOSTADMIN)@$(HOSTING) sudo rm -rf $(PRODDESTDIR)/approot
	(cd bin/output && rsync -ravu ./ $(HOSTADMIN)@$(HOSTING):$(PRODDESTDIR) >/dev/null)
	ssh $(HOSTADMIN)@$(HOSTING) sudo sync
	ssh $(HOSTADMIN)@$(HOSTING) sudo service kestrel start
else
	$(error EPRODANDGITSTATUS! Refus de pousser en production: des changements doivent être validés auprès du contrôle de versions.)
	git status
endif

%.min.js: %.js
	jsmin < $^ > $@

%.min.css: %.css
	jsmin < $^ > $@

%.Designer.cs: %.resx
	strotygen -l -p -t -r "Yavsc.Resources." $^

prepare_code: $(BASERESXGEN)

web_rebuild: 
	make -C ../Yavsc.Abstract
	make -C ../Yavsc.Server
	make bin/Debug/dnx451/Yavsc.dll

minJs: $(MINJS)

minCss: $(MINCSS)

minify: minCss minJs

