using IOTModel;
using SocketIOClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace IOTWebAPIFrontEnd.Controllers
{
    [RoutePrefix("")]
    public class SocketioDashboardController : Controller
    {
        // GET: Socket.io Dashboard

        [Route("")]
        public ActionResult Index()
        {
            return View();
        }

        [Route("NotifyFakeData")]
        public async Task<ActionResult> NotifyFakeData()
        {
            var socketioHubEndPoint = ConfigurationManager.AppSettings["socketioHubEndPoint"];
            var extension_socketio = @"/socket.io";
            var endpoint_full_url = string.Format("{0}{1}", socketioHubEndPoint, extension_socketio);
            
            var socket = new Client(endpoint_full_url);
            
            // register for 'connect' event with io server
            socket.On("connect", async (fn) =>
            {
                await Task.Factory.StartNew(async () =>
                {
                    for (int i = 0; i < 60; i++)
                    {
                        var measure = new Measure()
                        {
                            Humidity = GetRandomNumber(75, 90),
                            Temperature = GetRandomNumber(22, 25),
                            UtcDate = DateTime.UtcNow
                        };
                        var envelope = new Envelope();

                        if (socket != null)
                        {
                            if (socket.IsConnected)
                            {
                                envelope.Type = "meteo";
                                envelope.Measure = measure;
                                socket.Emit("msgtosrv", envelope);
                            }
                        }
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                });

            });

            socket.Connect();
            await Task.Delay(TimeSpan.FromSeconds(1));

            return new EmptyResult();
        }

        private static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
    }

}