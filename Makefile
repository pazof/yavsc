

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
	mkbundle --deps $(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin/Microsoft.Dnx.Host.Mono.dll $(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin/Microsoft.Dnx.Loader.dll $(DNX_USER_HOME)/runtimes/dnx-mono.1.0.0-rc1-update2/bin/Microsoft.Dnx.Host.dll src/Yavsc/bin/Debug/dnx451/Yavsc.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Hosting/1.0.0-rc1-final/lib/dnx451/Microsoft.AspNet.Hosting.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Authorization/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Authorization.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.DependencyInjection.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.DependencyInjection.Abstractions.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.Logging.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.Logging.Abstractions.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.OptionsModel/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.OptionsModel.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.Configuration.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.Configuration.Abstractions.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.Primitives/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.Primitives.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.Configuration.Binder/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.Configuration.Binder.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Mvc.Core/6.0.0-rc1-final/lib/net451/Microsoft.AspNet.Mvc.Core.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Mvc.Abstractions/6.0.0-rc1-final/lib/net451/Microsoft.AspNet.Mvc.Abstractions.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Http.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Http.Abstractions.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Http.Features/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Http.Features.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.WebEncoders.Core/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.WebEncoders.Core.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Routing/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Routing.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Http.Extensions/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Http.Extensions.dll src/Yavsc/bin/output/approot/packages/Microsoft.Net.Http.Headers/1.0.0-rc1-final/lib/net451/Microsoft.Net.Http.Headers.dll src/Yavsc/bin/output/approot/packages/Microsoft.Extensions.MemoryPool/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.MemoryPool.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.FileProviders.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.FileProviders.Abstractions.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Hosting.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Hosting.Abstractions.dll src/Yavsc/bin/output/approot/packages/System.Diagnostics.DiagnosticSource/4.0.0-beta-23516/lib/portable-net45+win8+wp8+wpa81/System.Diagnostics.DiagnosticSource.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Mvc.ViewFeatures/6.0.0-rc1-final/lib/net451/Microsoft.AspNet.Mvc.ViewFeatures.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Html.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Html.Abstractions.dll src/Yavsc/bin/output/approot/packages/Microsoft.AspNet.Mvc.Formatters.Json/6.0.0-rc1-final/lib/net451/Microsoft.AspNet.Mvc.Formatters.Json.dll packages/Newtonsoft.Json/6.0.1/lib/net45/Newtonsoft.Json.dll packages/Microsoft.AspNet.JsonPatch/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.JsonPatch.dll packages/Microsoft.AspNet.Diagnostics.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Diagnostics.Abstractions.dll packages/Microsoft.AspNet.Antiforgery/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Antiforgery.dll packages/Microsoft.AspNet.DataProtection/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.DataProtection.dll packages/Microsoft.AspNet.DataProtection.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.DataProtection.Abstractions.dll packages/Microsoft.AspNet.Cryptography.Internal/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.Cryptography.Internal.dll packages/Microsoft.Extensions.WebEncoders/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.WebEncoders.dll packages/Microsoft.AspNet.WebUtilities/1.0.0-rc1-final/lib/net451/Microsoft.AspNet.WebUtilities.dll packages/Microsoft.AspNet.Mvc.DataAnnotations/6.0.0-rc1-final/lib/net451/Microsoft.AspNet.Mvc.DataAnnotations.dll packages/Microsoft.Extensions.Localization.Abstractions/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.Localization.Abstractions.dll packages/Microsoft.Extensions.Localization/1.0.0-rc1-final/lib/net451/Microsoft.Extensions.Localization.dll src/Yavsc.Server/bin/Debug/net451/Yavsc.Server.dll src/Yavsc.Abstract/bin/Debug/net451/Yavsc.Abstract.dll -o yavsc
	

.PHONY: packages
