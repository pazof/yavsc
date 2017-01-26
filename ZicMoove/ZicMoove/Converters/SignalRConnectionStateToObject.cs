using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BookAStar.Converters
{
    public class SignalRConnectionStateToObject<T> : IValueConverter
    {
        public T ConnectedObject { set; get; }

        public T ConnectingObject { set; get; }

        public T DisconnectedObject { set; get; }

        public T ReconnectingObject { set; get; }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            switch ((ConnectionState)value) {
                case ConnectionState.Connected: return ConnectedObject;
                case ConnectionState.Connecting: return ConnectingObject;
                case ConnectionState.Disconnected: return DisconnectedObject;
                case ConnectionState.Reconnecting: return ReconnectingObject;
            }
            Debug.Assert(false);
            return null;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
