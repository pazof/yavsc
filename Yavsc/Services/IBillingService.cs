namespace Yavsc.Services
{
    using System.Linq;
    using System.Threading.Tasks;
    using Yavsc.Abstract.Workflow;

    public interface IBillingService
    {
        /// <summary>
        /// Renvoye la facture associée à une clé de facturation:
        /// le couple suivant :
        /// 
        /// * un code de facturation 
        ///   (identifiant associé à un type de facturation d'un flux de travail)
        /// * un entier long identifiant la demande du client
        ///   (à une demande, on associe au maximum une seule facture)
        /// </summary>
        /// <param name="billingCode">Identifiant du type de facturation</param>
        /// <param name="queryId">Identifiant de la demande du client</param>
        /// <returns>La facture</returns>
         Task<INominativeQuery> GetBillAsync(string billingCode, long queryId);
         Task<IQueryable<ISpecializationSettings>>  GetPerformersSettingsAsync(string activityCode);
         
         Task<ISpecializationSettings> GetPerformerSettingsAsync(string activityCode, string userId);

    }
}