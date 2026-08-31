using System.ComponentModel.DataAnnotations;
using Yavsc.Models.Relationship;
using Yavsc.Models.Workflow;

namespace Yavsc.Tests.NonRegression;

public class PerformerCodeInputValidationTests
{
    [Fact]
    public void Validate_rejects_unknown_country_code()
    {
        var profile = CreateBaseProfile();
        profile.ExerciseCountryCode = "de";
        profile.SIREN = "123456789";

        var results = Validate(profile);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PerformerProfile.ExerciseCountryCode)));
    }

    [Fact]
    public void Validate_rejects_code_not_matching_country_rule()
    {
        var profile = CreateBaseProfile();
        profile.ExerciseCountryCode = "pt";
        profile.SIREN = "ABC123";

        var results = Validate(profile);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(PerformerProfile.SIREN)));
    }

    [Fact]
    public void Validate_accepts_country_specific_valid_codes()
    {
        var fr = CreateBaseProfile();
        fr.ExerciseCountryCode = "fr";
        fr.SIREN = "123456789";

        var en = CreateBaseProfile();
        en.ExerciseCountryCode = "en";
        en.SIREN = "AB12CD34";

        var pt = CreateBaseProfile();
        pt.ExerciseCountryCode = "pt";
        pt.SIREN = "501964843";

        Assert.Empty(Validate(fr));
        Assert.Empty(Validate(en));
        Assert.Empty(Validate(pt));
    }

    private static PerformerProfile CreateBaseProfile()
    {
        return new PerformerProfile
        {
            PerformerId = "perf-1",
            SIREN = "123456789",
            ExerciseCountryCode = "fr",
            OrganizationAddress = new Location
            {
                Address = "1 rue du Test",
                Latitude = 48.8566,
                Longitude = 2.3522,
            },
        };
    }

    private static List<ValidationResult> Validate(PerformerProfile profile)
    {
        var ctx = new ValidationContext(profile);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(profile, ctx, results, validateAllProperties: true);
        return results;
    }
}