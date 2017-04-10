namespace Yavsc.Helpers
{
    using Models.Bank;
    public static class BankInfoHelpers
    {
        public static bool IsValid(this BankIdentity info) { 
            return ByIbanBIC(info) || ByAccountNumber(info) ;
        }
        public static bool ByIbanBIC(this BankIdentity info) {
            return (info.BIC != null && info.IBAN != null) ;
        }
        public static bool ByAccountNumber(this BankIdentity info){ 

                return (info.BankCode != null && info.WicketCode != null && info.AccountNumber != null && info.BankedKey >0);

        }
    }
}
