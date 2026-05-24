// Yavsc.ViewModels.Kyc/ModerationQueueItem.cs
namespace Yavsc.ViewModels.Kyc
{
    using System.Collections.Generic;
    using Yavsc.Models.Kyc;

    /// <summary>
    /// Ce que le modérateur voit dans sa queue.
    /// Toujours pseudonymisé — jamais d'identité réelle.
    /// </summary>
    public class ModerationQueueItem
    {
        public long DeclarationId { get; set; }

        /// <summary>
        /// Les 8 premiers caractères du TokenHash + "…"
        /// Suffisant pour distinguer les sujets en queue,
        /// insuffisant pour identifier quoi que ce soit.
        /// </summary>
        public string SubjectTokenHash { get; set; }

        public string Content { get; set; }

        public DeclarationSentiment Sentiment { get; set; }

        public ModerationStatus Status { get; set; }

        /// <summary>
        /// Descriptions des patterns regex qui ont matché.
        /// Pas les excerpts — le modérateur voit le texte complet de toute façon.
        /// </summary>
        public List<string> FlagDescriptions { get; set; } = new();

        /// <summary>
        /// True si au moins un pattern Blocking a matché —
        /// permet un affichage prioritaire dans l'UI.
        /// </summary>
        public bool HasBlockingFlag { get; set; }
    }
}