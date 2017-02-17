

namespace ZicMoove.Interfaces
{
    public enum PayMethod
    {
        Immediate,
        Delayed
    }

    public interface IPlatform
	{
        void OpenWeb (string Uri);
        
        // TODO Better
		string GCMStatusMessage { get; }
        
        void AddAccount();
        
        void RevokeAccount(string userName);

        void Pay(double amount, PayMethod method, string paymentName);

    }
}

