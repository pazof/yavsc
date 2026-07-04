using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Yavsc.Server.Models.Billing;

namespace Yavsc.Server.Services;

public static class FrontmatterParser
{
    public  const string YamlHeader = "---\n";
    public  const string YamlSeparator = "\n---";

    private static readonly IDeserializer _deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    public static FrontmatterOffreResult Parse(string markdown)
    {
        // Le contenu doit commencer par ----
        if (!markdown.TrimStart().StartsWith(YamlHeader, StringComparison.Ordinal))
            return new FrontmatterOffreResult { Corps = markdown };

        var parts = markdown.Substring(YamlHeader.Length).Trim().Split(new[] { YamlSeparator }, 2,
                           StringSplitOptions.None);
        if (parts.Length < 2)
            return new FrontmatterOffreResult { Corps = markdown };
        var yamlBlock = parts[0];
        var corps     = parts[1];
        var meta = _deserializer.Deserialize<FrontmatterOffreResult>(yamlBlock);
        meta.Corps = corps;
        return meta;
    }
}
