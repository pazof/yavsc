using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Yavsc.Server.Models.Billing;

namespace Yavsc.Server.Services;

public static class FrontmatterParser
{
    public const string YamlHeader = "---";
    public const string YamlSeparator = "---";

    private static readonly IDeserializer _deserializer =
        new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    public static FrontmatterOffreResult Parse(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var trimmed = markdown.TrimStart();

        if (!trimmed.StartsWith(YamlHeader, StringComparison.Ordinal))
            return new FrontmatterOffreResult { Corps = markdown };

        var afterOpeningMarker = trimmed[YamlHeader.Length..];
        var parts = afterOpeningMarker
            .Split(new[] { "\r\n---", "\n---" }, 2, StringSplitOptions.None);

        if (parts.Length < 2)
            return new FrontmatterOffreResult { Corps = markdown };

        var yamlBlock = parts[0].Trim();
        var corps = parts[1].TrimStart();
        var meta = _deserializer.Deserialize<FrontmatterOffreResult>(yamlBlock) ?? new FrontmatterOffreResult();
        meta.Corps = corps;
        return meta;
    }
}
