using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace MessageSender
{
    public class Program
    {
        public const string API_KEY = "AIzaSyDwPlu2ky9AoPwDvThoXxJwqiPq-Tc8p8k";
        public const string MESSAGE = "Hello, Xamarin!";

        static void Main(string[] args)
        {
            var reg_id= "eRSVBXTxiyk:APA91bE1NRs6LWBq9O3ACBV5g4B8D2ANwS9UjpPkwYFYWtv6NeTuSzBb6ZBhOFx_gbh9AbSgZ7uHETOPp26ahhJY74i55f7gULgaQR7-MV5CeaBYANfKcVgqQ5GTsb2zMCS_2MIUVy3Q";
            var jGcmData = new JObject();
            var jNotification = new JObject();
            jNotification.Add("body","a body");
            jNotification.Add("title",MESSAGE);
            jNotification.Add("icon","icon");
            jGcmData.Add("to",reg_id);
            jGcmData.Add("notification",jNotification);

/* "to" : "bk3RNwTe3H0:CI2k_HHwgIpoDKCIZvvDMExUdFQ3P1...",
    "notification" : {
      "body" : "great match!",
      "title" : "Portugal vs. Denmark",
      "icon" : "myicon"
    }
            var jData = new JObject();
    jData.Add("message", MESSAGE);
            jGcmData.Add("to", "/topics/global");
            jGcmData.Add("data", jData);
    */
            

            var url = new Uri("https://gcm-http.googleapis.com/gcm/send");
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    client.DefaultRequestHeaders.TryAddWithoutValidation(
                        "Authorization", "key=" + API_KEY);

                    Task.WaitAll(client.PostAsync(url,
                        new StringContent(jGcmData.ToString(), Encoding.UTF8, "application/json"))
                            .ContinueWith(response =>
                            {
                                Task.Run ( async () => {
                                    var content = await response.Result.Content.ReadAsStringAsync();
                                    Console.WriteLine(content);
                                });
                                Console.WriteLine(">Message sent: check the client device notification tray.");
                            }));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to send GCM message:");
                Console.Error.WriteLine(e.StackTrace);
            }
        }
    }
}
