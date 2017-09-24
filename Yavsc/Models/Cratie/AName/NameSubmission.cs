using System.ComponentModel.DataAnnotations;

namespace Yavsc.Models.Cratie.AName
{
    public class NameSubmission : Submission
    {
        [RegularExpression(@"[a-zA-Z]+", ErrorMessage = "Nom invalide (seules les lettres de l'alphabet sont autorisées).", ErrorMessageResourceName = "EInvalidName")]

        public string FirstChoice {get; set;} 
        [RegularExpression(@"[a-zA-Z]+", ErrorMessage = "Nom invalide (seules les lettres de l'alphabet sont autorisées).", ErrorMessageResourceName = "EInvalidName")]
        public string SecondChoice {get; set;} 
        [RegularExpression(@"[a-zA-Z]+", ErrorMessage = "Nom invalide (seules les lettres de l'alphabet sont autorisées).", ErrorMessageResourceName = "EInvalidName")]
        public string ThirdChoice {get; set;} 
    }
}