using BookAStar.Model.Auth.Account;
using Xamarin.Forms;
using Yavsc.Models.Identity;

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

        IGCMDeclaration GetDeviceInfo();

        TAnswer InvokeApi<TAnswer>(string method, object arg);

        object InvokeApi(string method, object arg);

        View CreateMarkdownView(string markdown);
    }
}

