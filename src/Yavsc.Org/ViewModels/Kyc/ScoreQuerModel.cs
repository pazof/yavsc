// Yavsc.ViewModels.Kyc/ScoreQueryModel.cs
namespace Yavsc.ViewModels.Kyc
{
    using System.ComponentModel.DataAnnotations;

    public class ScoreQueryModel
    {
        [EmailAddress]
        public string SubjectEmail { get; set; }

        public string ExternalToken { get; set; }
    }
}
