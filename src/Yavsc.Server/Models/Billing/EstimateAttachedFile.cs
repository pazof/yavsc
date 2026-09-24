using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models.Blog;

namespace Yavsc.Models.Billing
{
    /// <summary>
    /// Pièce jointe d'un devis, attachée **par référence** à un fichier
    /// de l'espace de stockage personnel du fournisseur
    /// (<see cref="UploadedFile"/>). Le fichier reste dans l'espace perso
    /// du fournisseur (pas de duplication) ; seul le lien est persisté.
    /// Le téléchargement est servi par le host Blogs, qui autorise
    /// l'appelant (fournisseur ou client du devis) via cette table.
    /// </summary>
    [PrimaryKey(nameof(FileId), nameof(EstimateId))]
    public class EstimateAttachedFile
    {
        /// <summary>
        /// Identifiant du fichier référencé (partie de la clé composite).
        /// </summary>
        public long FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual UploadedFile File { get; set; }

        /// <summary>
        /// Identifiant du devis auquel le fichier est rattaché
        /// (partie de la clé composite).
        /// </summary>
        public long EstimateId { get; set; }

        [ForeignKey("EstimateId")]
        public virtual Estimate Estimate { get; set; }
    }
}