using Yavsc.Server;
using GoogleTranslateNET;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.OptionsModel;
using Microsoft.AspNet.Localization;
using System.Linq;

namespace Yavsc.Services.GoogleApis
{
    public class  Translator : ITranslator
    {
        GoogleTranslate _gg;
        ILogger _logger;

        public Translator(ILoggerFactory loggerFactory, 
        IOptions<RequestLocalizationOptions> rqLocOptions, 
        IOptions<GoogleAuthSettings> gSettings)
        {
             _gg = new GoogleTranslate(gSettings.Value.ApiKey);
             _logger = loggerFactory.CreateLogger<Translator>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        static Language GetLanguageFromCountryCode(string lang)
        {
            switch (lang) {
                case "fr":
                case "fr-FR":
                case "French":
                return Language.French;
              
                case "en":
                case "en-GB":
                case "en-US":
                case "English":
                return Language.English;

                case "pt":
                case "br":
                return Language.Portuguese;
            }
            return Language.Automatic;
        }

        public string[] Translate(string slang, string dlang, string[] text)
        {
            var destinationLanguage = GetLanguageFromCountryCode(dlang);
            if (destinationLanguage == Language.Unknown)
            throw new Exception ("destinationLanguage == Language.Unknown");
            var sourceLanguage = GetLanguageFromCountryCode(slang);
            var gResult = _gg.Translate(sourceLanguage, destinationLanguage, text);
            return gResult.Select(tr => tr.TranslatedText).ToArray();
        }
    }
}