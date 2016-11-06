using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yavsc.Model.Bank
{
    public class BankIdentity
    {
        [Key(), DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        /// <summary> 
        /// Gets or sets the BI. 
        /// </summary> 
        /// <value>The BI.</value> 
        [DisplayName("Code BIC")]
        [StringLength(15)]
        public string BIC { get; set; }

        /// <summary> 
        /// Gets or sets the IBA. 
        /// </summary> 
        /// <value>The IBA.</value> 
        [DisplayName("Code IBAN")]
        [StringLength(33)]
        public string IBAN { get; set; }


        /// <summary> 
        /// Gets or sets the bank code. 
        /// </summary> 
        /// <value>The bank code.</value> 
        [DisplayName("Code Banque")]
        [StringLength(5)]
        public string BankCode { get; set; }

        /// <summary> 
        /// Gets or sets the wicket code. 
        /// </summary> 
        /// <value>The wicket code.</value> 
        [DisplayName("Code Guichet")]
        [StringLength(5)]
        public string WicketCode { get; set; }

        /// <summary> 
        /// Gets or sets the account number. 
        /// </summary> 
        /// <value>The account number.</value> 
        [DisplayName("Numéro de compte")]
        [StringLength(15)]
        public string AccountNumber { get; set; }

        /// <summary> 
        /// Gets or sets the banked key. 
        /// </summary> 
        /// <value>The banked key.</value> 
        [DisplayName("Clé RIB")]
        public int BankedKey { get; set; }

    }


}