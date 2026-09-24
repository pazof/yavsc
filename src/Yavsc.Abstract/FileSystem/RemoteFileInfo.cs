namespace Yavsc.ViewModels
{
    public class RemoteFileInfo
    {
        public string Name { get; set; }

        public long Size { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime LastModified { get; set; }

        /// <summary>
        /// Identifiant de la ligne <c>UploadedFile</c> associée, quand le
        /// fichier a été téléversé via <c>api/v1/fs</c> (donc référencé en
        /// base). <c>null</c> pour un fichier présent sur disque sans ligne
        /// de métadonnées (fichier antérieur à la fonctionnalité, ou non
        /// encore backfillé). C'est cette valeur que le client lourd
        /// (PostIt) envoie pour attacher le fichier à un devis/demande.
        /// </summary>
        public long? Id { get; set; }

    }

}
