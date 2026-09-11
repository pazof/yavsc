using Microsoft.EntityFrameworkCore;
using Yavsc.Models;
using Yavsc.Models.Workflow;
using Yavsc.Server.Services;

namespace Yavsc.Org.Tests;

public class DictionnaireMetierTests
{
    [Fact]
    public void DictionnaireMetier_and_TermeMetier_can_be_constructed()
    {
        var dictionary = new DictionnaireMetier
        {
            Id = 1,
            Nom = "Droit",
            Langue = "fr",
            DomaineActiviteCode = "Droit"
        };

        var term = new TermeMetier
        {
            Id = 2,
            DictionnaireMetierId = dictionary.Id,
            DictionnaireMetier = dictionary,
            Mot = "contrat",
            Definition = "Accord de volontés",
            Langue = "fr",
            StatutValidation = StatutValidationTerme.Propose,
            ProposeParId = "user-1"
        };

        Assert.Equal("Droit", dictionary.DomaineActiviteCode);
        Assert.Equal("contrat", term.Mot);
        Assert.Equal(StatutValidationTerme.Propose, term.StatutValidation);
    }

    [Fact]
    public async Task DictionnaireMetier_moderation_flow_allows_propose_validate_and_reject()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);
        context.Activities.Add(new Activity
        {
            Code = "Droit",
            Name = "Droit",
            ParentCode = null,
            Description = "Domaine de référence",
            Hidden = false,
            Forms = new List<CommandForm>()
        });

        context.DictionnaireMetier.Add(new DictionnaireMetier
        {
            Nom = "Droit civil",
            Langue = "fr",
            DomaineActiviteCode = "Droit"
        });
        await context.SaveChangesAsync();

        var service = new DictionnaireMetierModerationService(context);

        var proposed = await service.ProposerTermAsync(1, "contrat", "Accord de volontés", "fr", "user-proposer");
        Assert.Equal(StatutValidationTerme.Propose, proposed.StatutValidation);

        var validated = await service.ValiderTermAsync(proposed.Id, "user-moderator");
        Assert.Equal(StatutValidationTerme.Valide, validated.StatutValidation);
        Assert.Equal("user-moderator", validated.ValideParId);

        var rejected = await service.RejeterTermAsync(1, "user-moderator");
        Assert.Equal(StatutValidationTerme.Rejete, rejected.StatutValidation);
    }
}
