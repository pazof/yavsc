using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Yavsc.Models.Blog;

namespace Yavsc.Models.Billing
{
    /// <summary>
    /// Pièce jointe d'une demande (NominativeServiceCommand), attachée
    /// **par référence** à un fichier de l'espace de stockage personnel
    /// du client (<see cref="UploadedFile"/>). Le fichier reste dans
    /// l'espace perso du client (pas de duplication) ; seul le lien est
    /// persisté. Le téléchargement est servi par le host Blogs, qui
    /// autorise l'appelant (client ou fournisseur de la demande) via
    /// cette table.
    /// </summary>
    [PrimaryKey(nameof(FileId), nameof(CommandId))]
    public class QueryAttachedFile
    {
        /// <summary>
        /// Identifiant du fichier référencé (partie de la clé composite).
        /// </summary>
        public long FileId { get; set; }

        [ForeignKey("FileId")]
        public virtual UploadedFile File { get; set; }

        /// <summary>
        /// Identifiant de la demande à laquelle le fichier est rattaché
        /// (partie de la clé composite).
        /// </summary>
        public long CommandId { get; set; }

        [ForeignKey("CommandId")]
        public virtual NominativeServiceCommand Command { get; set; }
    }
}