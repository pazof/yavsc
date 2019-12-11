LIBS:=$(shell ls private/lib/*.dll)
MONO_PREFIX=/home/paul/mono46
DNX_USER_HOME=/home/paul/.dnx
DNXLIBS=Microsoft.Dnx.Host.Mono.dll Microsoft.Dnx.Host.dll Microsoft.Dnx.ApplicationHost.dll Microsoft.Dnx.Loader.dll Microsoft.Dnx.Compilation.Abstractions.dll Microsoft.Dnx.Compilation.CSharp.Abstractions.dll Microsoft.CodeAnalysis.dll Microsoft.CodeAnalysis.CSharp.dll Microsoft.Dnx.Compilation.CSharp.Common.dll Microsoft.Dnx.Compilation.CSharp.dll Microsoft.Dnx.Compilation.dll Microsoft.Dnx.Runtime.dll Microsoft.Dnx.Runtime.Internals.dll Microsoft.Extensions.PlatformAbstractions.dll System.Collections.Immutable.dll System.Reflection.Metadata.dll

DNXLIBFP:=$(addprefix $(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin/, $(DNXLIBS))

checklibs:
	ls $(DNXLIBFP)

test:
	make -C src/test

web:
	make -C src/Yavsc web

pushInProd:
	make -C src/Yavsc pushInProd

pushInPre:
	make -C src/Yavsc pushInPre

packages:
	make -C src/Yavsc.Abstract pack

findResources:
	find src -name "*.resx" |sort
 
prepare_all_code: 
	make -C  src/Yavsc.Abstract prepare_code 
	make -C  src/Yavsc.Server prepare_code 
	make -C  src/Yavsc prepare_code

bundle:
	#export MONO_PATH=$(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin/
	mkbundle --static $(DNXLIBS) src/Yavsc/bin/Debug/dnx451/Yavsc.dll src/Yavsc/bin/Debug/dnx451/pt/Yavsc.resources.dll src/Yavsc/bin/Debug/dnx451/en/Yavsc.resources.dll src/Yavsc.Server/bin/Debug/dnx451/Yavsc.Server.dll src/Yavsc.Server/bin/Debug/dnx451/en/Yavsc.Server.resources.dll src/Yavsc.Server/bin/Debug/dnx451/fr/Yavsc.Server.resources.dll src/Yavsc.Abstract/bin/Debug/dnx451/Yavsc.Abstract.dll src/Yavsc.Abstract/bin/Debug/dnx451/en/Yavsc.Abstract.resources.dll src/Yavsc.Abstract/bin/Debug/dnx451/pt/Yavsc.Abstract.resources.dll src/OAuth.AspNet.AuthServer/bin/Debug/dnx451/OAuth.AspNet.AuthServer.dll src/OAuth.AspNet.Token/bin/Debug/dnx451/OAuth.AspNet.Token.dll  $(LIBS) -L $(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin --machine-config $(MONO_PREFIX)/etc/mono/4.5/machine.config -o yavsc-dev

.PHONY: packages
