using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;

public class ContactService
{
    public async Task<IEnumerable<Contact>> GetDeviceContactsAsync()
    {
        // 1. Ensure the platform supports MAUI Essentials APIs
        if (DeviceInfo.Current.Platform == DevicePlatform.Unknown)
        {
            throw new PlatformNotSupportedException("MAUI Essentials is not available on this platform.");
        }

        try
        {
            // 2. Request runtime permission (Required for Android & iOS)
            var status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            if (status != PermissionStatus.Granted)
            {
                // Permission denied by user
                return Array.Empty<Contact>();
            }

            // 3. Fetch all contacts
            var contactsEnumerable = await Contacts.Default.GetAllAsync();
            return contactsEnumerable ?? Array.Empty<Contact>();
        }
        catch (Exception ex)
        {
            // Handle cross-platform exceptions or logs here
            System.Diagnostics.Debug.WriteLine($"Error fetching contacts: {ex.Message}");
            return Array.Empty<Contact>();
        }
    }
}
