using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Yavsc.Attributes.Validation;

namespace Yavsc.Models.Bank
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
        [YaStringLength(15)]
        public string BIC { get; set; }

        /// <summary> 
        /// Gets or sets the IBA. 
        /// </summary> 
        /// <value>The IBA.</value> 
        [DisplayName("Code IBAN")]
        [YaStringLength(33)]
        public string IBAN { get; set; }


        /// <summary> 
        /// Gets or sets the bank code. 
        /// </summary> 
        /// <value>The bank code.</value> 
        [DisplayName("Code Banque")]
        [YaStringLength(5)]
        public string BankCode { get; set; }

        /// <summary> 
        /// Gets or sets the wicket code. 
        /// </summary> 
        /// <value>The wicket code.</value> 
        [DisplayName("Code Guichet")]
        [YaStringLength(5)]
        public string WicketCode { get; set; }

        /// <summary> 
        /// Gets or sets the account number. 
        /// </summary> 
        /// <value>The account number.</value> 
        [DisplayName("Numéro de compte")]
        [YaStringLength(15)]
        public string AccountNumber { get; set; }

        /// <summary> 
        /// Gets or sets the banked key. 
        /// </summary> 
        /// <value>The banked key.</value> 
        [DisplayName("Clé RIB")]
        public int BankedKey { get; set; }

    }


}