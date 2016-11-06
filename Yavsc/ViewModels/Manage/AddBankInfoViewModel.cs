namespace Yavsc.ViewModels.Manage
{
    using Model.Bank;
    public class AddBankInfoViewModel
    {
        public BankIdentity Data{get; private set; }
        public AddBankInfoViewModel(BankIdentity data)
        {
            this.Data = data;
        }

        public bool IsValid { get {
            return ByIbanBIC || ByAccountNumber ;
        }}
        public bool ByIbanBIC { get { 
            return (Data.BIC != null && Data.IBAN != null) ;
        }}
        public bool ByAccountNumber { 
            get { 
                return (Data.BankCode != null && Data.WicketCode != null && Data.AccountNumber != null && Data.BankedKey >0);
            }
        }
    }
}