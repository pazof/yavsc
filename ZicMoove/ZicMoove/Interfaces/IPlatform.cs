using ZicMoove.Model.Auth.Account;
using System;
using Xamarin.Forms;
using Yavsc.Models.Identity;

namespace ZicMoove.Interfaces
{
    public interface IPlatform
	{
        void OpenWeb (string Uri);
        
        // TODO Better
		string GCMStatusMessage { get; }

        bool EnablePushNotifications (bool enable);
        
        void AddAccount();
        
        void RevokeAccount(string userName);
    }
}

