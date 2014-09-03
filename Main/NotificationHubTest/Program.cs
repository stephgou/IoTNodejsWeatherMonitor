using Microsoft.ServiceBus.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNotificationHub
{
    class Program
    {
        private static async Task SendNotificationAsync()
        {
            NotificationHubClient hub =
                NotificationHubClient.CreateClientFromConnectionString("Endpoint=sb:...", "NomDuHub");

            var toast = @"<toast launch=""{&quot;type&quot;:&quot;toast&quot;,&quot;alertType&quot;:&quot;humidity&quot;,&quot;alertValue&quot;:&quot;90%&quot;}"">"
                        + @"<visual>"
                            + @"<binding template=""ToastText02"">"
                                + @"<text id=""1"">Alerte humidité</text>"
                                + @"<text id=""2"">Dernière mesure : 90%</text>"
                            + @"</binding>"
                        + @"</visual>"
                    + @"</toast>";

            await hub.SendWindowsNativeNotificationAsync(toast);
        }

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Press <ENTER> to send a push notification");
                Console.ReadLine();

                SendNotificationAsync().Wait();
            }
        }
    }
}
