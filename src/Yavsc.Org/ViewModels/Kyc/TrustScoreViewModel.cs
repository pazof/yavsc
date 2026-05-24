// Yavsc.ViewModels.Kyc/TrustScoreView.cs
namespace Yavsc.ViewModels.Kyc
{
    /// <summary>
    /// Ce que le CONSULTANT voit. Jamais d'identité, jamais de détail de déclaration.
    /// </summary>
    public class TrustScoreView
    {
        public int Score { get; set; }           // ex: 0–100
        public string Category { get; set; }     // "Fiable" / "Prudence" / "Risque"
        public int DeclarationCount { get; set; } // combien de déclarations agrégées
        public DateTime LastUpdated { get; set; }
    }

    
}