// // NotWorking.cs
// /*
// paul  21/06/2018 10:16 20182018 6 21
// */
using System;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.OptionsModel;
using Xunit;
using Yavsc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Dnx.TestHost.TestAdapter;
using Xunit.Abstractions;

namespace yavscTests
{
    [Collection("Yavsc dropped intents")]
    [Trait("regres", "yes")]
    public class NotWorking : BaseTestContext
    {
        readonly SourceInformationProvider _sourceInfoProvider;
        readonly IOptions<LocalizationOptions> _localizationOptions;
        public NotWorking(
        SourceInformationProvider sourceInfoProvider,
        IOptions<LocalizationOptions> localizationOptions,
        ServerSideFixture serverFixture, ITestOutputHelper output
          ) : base(output, serverFixture)
        {
            _sourceInfoProvider = sourceInfoProvider;
            _localizationOptions = localizationOptions;
        }

        public void StringLocalizer()
        {
            // TODO build applicationEnvironment
            ResourceManagerStringLocalizerFactory strFact = new ResourceManagerStringLocalizerFactory
                (applicationEnvironment, _localizationOptions);
            IStringLocalizer stringLocalizer = strFact.Create(typeof(NotWorking));
        }

        public void NoDnxEnv()
        {

            IOptions<LocalizationOptions> localOptions = Activator.CreateInstance<IOptions<LocalizationOptions>>();
            ResourceManagerStringLocalizerFactory strFact = new ResourceManagerStringLocalizerFactory(applicationEnvironment, localOptions);
            IStringLocalizer stringLocalizer = strFact.Create(typeof(NotWorking));
        }
    }
}
