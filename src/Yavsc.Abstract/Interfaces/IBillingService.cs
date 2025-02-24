namespace Yavsc.Services
{
  using System.Linq;
  using System.Threading.Tasks;
  using System.Collections.Generic;
    using Yavsc.Abstract.Workflow;

    public interface IBillingService
  {
    // TODO ensure a default value at using this:
    /// <summary>
    /// maps a command type name to a billing code, used to get bill assets
    /// </summary>
    /// <returns></returns>
    Dictionary<string,string> BillingMap { get; }

        /// <summary>
    /// Renvoye la facture associée à une clé de facturation,
    /// à partir du couple suivant :
    /// 
    /// * un code de facturation 
    ///   (identifiant associé à un type de demande du client)
    /// * un entier long identifiant la demande du client
    ///   (à une demande, on associe au maximum une seule facture)
    /// </summary>
    /// <param name="billingCode">Identifiant du type de facturation</param>
    /// <param name="queryId">Identifiant de la demande du client</param>
    /// <returns>La facture</returns>
    Task<IDecidableQuery> GetBillAsync(string billingCode, long queryId);
    
    /// <summary>
    /// Perfomer settings for the specified performer in the activity 
    /// </summary>
    /// <param name="activityCode">activityCode</param>
    /// <param name="userId">performer uid</param>
    /// <returns></returns>
    Task<IUserSettings> GetPerformersSettingsAsync(string activityCode, string userId);

  }
}
