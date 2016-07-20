using BookAStar.Model.Auth.Account;

namespace BookAStar
{
    public interface IPlatform
	{
        void OpenWeb (string Uri);
        
        // TODO Better

		string GCMStatusMessage { get; }

        bool EnablePushNotifications (bool enable);
        
        void AddAccount();
        
        void RevokeAccount(string userName);

        GoogleCloudMobileDeclaration GetDeviceInfo();

       // void RegisterThisDevice();
        
    }
}

