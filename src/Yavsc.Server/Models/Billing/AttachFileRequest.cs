namespace Yavsc.Models.Billing
{
    /// <summary>
    /// Corps de requête pour attacher un fichier de l'espace perso à un
    /// devis (fournisseur) ou à une demande (client), par référence à son
    /// identifiant <see cref="Yavsc.Models.Blog.UploadedFile"/>.
    /// </summary>
    public class AttachFileRequest
    {
        /// <summary>Identifiant du fichier référencé.</summary>
        public long FileId { get; set; }
    }
}