LIBS:=$(shell ls private/lib/*.dll)
MONO_PREFIX=/home/paul/mono46
DNX_USER_HOME=/home/paul/.dnx
DNXLIBS=Microsoft.Dnx.Host.Mono.dll Microsoft.Dnx.Host.dll Microsoft.Dnx.ApplicationHost.dll Microsoft.Dnx.Loader.dll Microsoft.Dnx.Compilation.Abstractions.dll Microsoft.Dnx.Compilation.CSharp.Abstractions.dll Microsoft.CodeAnalysis.dll Microsoft.CodeAnalysis.CSharp.dll Microsoft.Dnx.Compilation.CSharp.Common.dll Microsoft.Dnx.Compilation.CSharp.dll Microsoft.Dnx.Compilation.dll Microsoft.Dnx.Runtime.dll Microsoft.Dnx.Runtime.Internals.dll Microsoft.Extensions.PlatformAbstractions.dll System.Collections.Immutable.dll System.Reflection.Metadata.dll
DNXLIBFP:=$(addprefix $(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin/, $(DNXLIBS))
CONFIG=Debug

git_status := $(shell git status -s --porcelain |wc -l)

clean: 
	rm -f src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/Yavsc.Abstract.dll src/OAuth.AspNet.Token/bin/$(CONFIG)/dnx451/OAuth.AspNet.Token.dll src/OAuth.AspNet.AuthServer/bin/$(CONFIG)/dnx451/OAuth.AspNet.AuthServer.dll src/Yavsc.Server/bin/$(CONFIG)/dnx451/Yavsc.Server.dll src/Yavsc/bin/$(CONFIG)/dnx451/Yavsc.dll 

checklibs:
	ls $(DNXLIBFP)

updatedeps:
	cp src/Yavsc/bin/output/approot/packages/*/*/lib/*net451*/*.dll private/lib/
	cp src/Yavsc/bin/output/approot/packages/*/*/lib/*dnx451*/*.dll private/lib/

test:
	make -C src/test

web:
	make -C src/Yavsc web

pushInPre: yavscd
	make -C src/Yavsc pushInPre

pushInProd: yavscd
	make -C src/Yavsc pushInProd

packages:
	make -C src/Yavsc.Abstract pack

findResources:
	find src -name "*.resx" |sort
 
prepare_all_code: 
	make -C  src/Yavsc.Abstract prepare_code 
	make -C  src/Yavsc.Server prepare_code 
	make -C  src/Yavsc prepare_code

src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/Yavsc.Abstract.dll: prepare_all_code
	make -C src/Yavsc.Abstract CONFIGURATION=$(CONFIG)

src/OAuth.AspNet.Token/bin/$(CONFIG)/dnx451/OAuth.AspNet.Token.dll: prepare_all_code
	make -C src/OAuth.AspNet.Token CONFIGURATION=$(CONFIG)

src/OAuth.AspNet.AuthServer/bin/$(CONFIG)/dnx451/OAuth.AspNet.AuthServer.dll: prepare_all_code
	make -C  src/OAuth.AspNet.AuthServer CONFIGURATION=$(CONFIG)

src/Yavsc.Server/bin/$(CONFIG)/dnx451/Yavsc.Server.dll: src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/Yavsc.Abstract.dll prepare_all_code
	make -C  src/Yavsc.Server CONFIGURATION=$(CONFIG)

src/Yavsc/bin/$(CONFIG)/dnx451/Yavsc.dll: src/Yavsc.Server/bin/$(CONFIG)/dnx451/Yavsc.Server.dll src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/Yavsc.Abstract.dll src/OAuth.AspNet.AuthServer/bin/$(CONFIG)/dnx451/OAuth.AspNet.AuthServer.dll src/OAuth.AspNet.Token/bin/$(CONFIG)/dnx451/OAuth.AspNet.Token.dll
	make -C  src/Yavsc CONFIGURATION=$(CONFIG)

yavscd: src/Yavsc/bin/$(CONFIG)/dnx451/Yavsc.dll src/Yavsc.Server/bin/$(CONFIG)/dnx451/Yavsc.Server.dll src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/Yavsc.Abstract.dll src/OAuth.AspNet.AuthServer/bin/$(CONFIG)/dnx451/OAuth.AspNet.AuthServer.dll src/OAuth.AspNet.Token/bin/$(CONFIG)/dnx451/OAuth.AspNet.Token.dll
	mkbundle --static $(DNXLIBS) src/Yavsc/bin/$(CONFIG)/dnx451/Yavsc.dll src/Yavsc/bin/$(CONFIG)/dnx451/pt/Yavsc.resources.dll src/Yavsc/bin/$(CONFIG)/dnx451/en/Yavsc.resources.dll src/Yavsc.Server/bin/$(CONFIG)/dnx451/Yavsc.Server.dll src/Yavsc.Server/bin/$(CONFIG)/dnx451/en/Yavsc.Server.resources.dll src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/Yavsc.Abstract.dll src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/en/Yavsc.Abstract.resources.dll src/Yavsc.Abstract/bin/$(CONFIG)/dnx451/pt/Yavsc.Abstract.resources.dll src/OAuth.AspNet.AuthServer/bin/$(CONFIG)/dnx451/OAuth.AspNet.AuthServer.dll src/OAuth.AspNet.Token/bin/$(CONFIG)/dnx451/OAuth.AspNet.Token.dll  $(LIBS) -L $(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin --machine-config $(MONO_PREFIX)/etc/mono/4.5/machine.config -o yavscd
	strip yavscd

version-increment-patch:
	scripts/version.sh $$(cat version.txt) patch > version.txt


.PHONY: packages
