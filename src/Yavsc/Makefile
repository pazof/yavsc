DESTDIR=/srv/www/yavscpre
PRODDESTDIR=/srv/www/yavsc
HOSTING=localhost
ASPNET_LOG_LEVEL=debug
SOURCE_DIR=$(HOME)/workspace/yavsc
MAKEFILE_DIR=$(SOURCE_DIR)/scripts/build/make
BASERESX= Resources/Yavsc.Models.IT.Fixing.Resources.resx \
	Resources/Yavsc.ViewComponents.CommentViewComponent.resx \
	Resources/Yavsc.ViewModels.FrontOffice.PerformerProfileViewModel.resx \
	Resources/Yavsc.ViewModels.EnrolerViewModel.resx \
	Resources/Yavsc.YavscLocalisation.resx
BASERESXGEN=$(BASERESX:.resx=.Designer.cs)
MCS_OPTIONS=-debug
MONO_OPTIONS="--debug"

include $(MAKEFILE_DIR)/dnx.mk
include $(MAKEFILE_DIR)/versioning.mk

MINJS=wwwroot/js/bootstrap.min.js      wwwroot/js/input-lib.min.js             wwwroot/js/jquery.signalR-2.2.1.min.js  wwwroot/js/quill.min.js wwwroot/js/chat.min.js           wwwroot/js/jquery-2.2.4.min.js          wwwroot/js/jquery-ui.min.js             wwwroot/js/showdown.min.js wwwroot/js/comment.min.js        wwwroot/js/jquery-3.2.1.min.js          wwwroot/js/md-helpers.min.js            wwwroot/js/site.min.js wwwroot/js/dropzone.min.js       wwwroot/js/jquery-migrate-3.0.0.min.js  wwwroot/js/parallax.min.js              wwwroot/js/str.min.js wwwroot/js/google-geoloc.min.js  wwwroot/js/jquery.min.js                wwwroot/js/paypalbutton.min.js          wwwroot/js/to-markdown.min.js

MINCSS=wwwroot/css/coiffure.min.css  wwwroot/css/dev.min.css           wwwroot/css/freespeech.min.css  wwwroot/css/yavsc.min.css wwwroot/css/default.min.css   wwwroot/css/font-awesome.min.css  wwwroot/css/lua.min.css         wwwroot/css/yavscpre.min.css wwwroot/css/clear/site.min.css  wwwroot/css/main/bootstrap.min.css  wwwroot/css/main/jquery-ui.min.css   wwwroot/css/main/site.min.css wwwroot/css/dark/site.min.css   wwwroot/css/main/dropzone.min.css   wwwroot/css/main/quill.snow.min.css

web: project.lock.json
	MCS_OPTIONS=$(MCS_OPTIONS) MONO_OPTIONS=$(MONO_OPTIONS) ASPNET_LOG_LEVEL=$(ASPNET_LOG_LEVEL) ASPNET_ENV=$(ASPNET_ENV) dnx web --configuration=$(CONFIGURATION) |tee web.log

nweb:
	MONO_PATH=$(MONO_PATH):~/.dnx/runtimes/dnx-mono.1.0.0-rc1-update2/bin ~/.dnx/runtimes/dnx-mono.1.0.0-rc1-update2/bin/ndnx web

showConfig:
	@echo HOSTING: $(USER)@$(HOSTING)
	@echo ENV: $(ASPNET_ENV)
	@echo CONFIGURATION: $(CONFIGURATION)
	@echo DESTDIR: $(DESTDIR)
	@echo PRODDESTDIR: $(PRODDESTDIR)

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
	sudo service kestrel-pre stop
	sudo rm -rf $(DESTDIR)/approot
	sudo cp -a bin/output/* $(DESTDIR)
	sudo cp ../../yavscd /usr/local/bin/yavscd-pre
	sudo sync
	sudo service kestrel-pre start

pushInProd: cleanoutput bin/output/wwwroot/version
ifeq ($(git_status),0)
	sudo service kestrel stop
	sudo rm -rf $(PRODDESTDIR)/approot
	sudo cp -a bin/output/* $(PRODDESTDIR) 
	sudo cp ../../yavscd /usr/local/bin/yavscd
	sudo sync
	sudo service kestrel start
else
	$(error EPRODANDGITSTATUS! Refus de pousser en production: des changements doivent être validés auprès du contrôle de versions.)
endif

cleanPublish: pushInPre pushInProd 
	rm -rf bin/output/

%.min.js: %.js
	jsmin < $^ > $@

%.min.css: %.css
	jsmin < $^ > $@

%.Designer.cs: %.resx
	strongresbuildercli -l -p -t -r "Yavsc.Resources." $^

prepare_code: $(BASERESXGEN)

web_rebuild: 
	make -C ../Yavsc.Abstract
	make -C ../Yavsc.Server
	make bin/Debug/dnx451/Yavsc.dll

minJs: $(MINJS)

minCss: $(MINCSS)

minify: minCss minJs

prepare_mvc_templates: $(SOURCE_DIR)/scripts/configure/tools/mvc-code-generators/new-templates/ViewGenerator/Create.cshtml \
	$(SOURCE_DIR)/scripts/configure/tools/mvc-code-generators/new-templates/ViewGenerator/Delete.cshtml \
	$(SOURCE_DIR)/scripts/configure/tools/mvc-code-generators/new-templates/ViewGenerator/Edit.cshtml \
	$(SOURCE_DIR)/scripts/configure/tools/mvc-code-generators/new-templates/ViewGenerator/List.cshtml \
	$(SOURCE_DIR)/scripts/configure/tools/mvc-code-generators/new-templates/ViewGenerator/Details.cshtml
	cp $^ $(SOURCE_DIR)/packages/Microsoft.Extensions.CodeGenerators.Mvc/1.0.0-rc1-final/Templates/ViewGenerator/

$(BINTARGETPATH): $(BASERESXGEN) project.lock.json

