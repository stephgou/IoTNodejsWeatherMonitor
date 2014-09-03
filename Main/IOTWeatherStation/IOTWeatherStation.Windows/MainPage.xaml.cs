using Microsoft.AspNet.SignalR.Client;
using SocketIOClient.WinRT;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using WeatherStation.SensorTag;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace WeatherStation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly DataProvider _dataProvider;

        //socket.io
        private SocketIOClient.WinRT.Client _socketIOClient;

        public MainPage()
        {
            this.InitializeComponent();
#if DEBUG
            _dataProvider = new DataProvider(useFakeDataIfJeromeBonaldiEffect: true);
#else
            _dataProvider = new DataProvider(useFakeDataIfJeromeBonaldiEffect: false);
#endif
            _dataProvider.NewMeasureAvailable += OnNewMeasureAvailable;
        }

        private static void AQuoiSertCetteCallback(string socketEvent)
        {
            Debug.WriteLine(socketEvent);
        }

        private async void OnNewMeasureAvailable(object sender, MeasureEventArgs e)
        {
            if (_socketIOClient != null)
            {
                if (_socketIOClient.IsConnected)
                {
                var envelope = new Envelope();

                envelope.Type = "meteo";
                envelope.Measure = e.Measure;

                _socketIOClient.Emit("msgtosrv", envelope);

                }
            }

            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                string measureAsString = string.Format("Date = {0}, Temperature = {1} °C, Humidity = {2} %",
                    e.Measure.UtcDate.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"), e.Measure.Temperature, e.Measure.Humidity);
                measuresList.Items.Add(measureAsString);
                lastMeasure.Text = measureAsString;
            });
        }

        private async void btnStartSensor_Click(object sender, RoutedEventArgs e)
        {
            var endpoint = @"http://localhost:888";
            //var endpoint = @"http://xxx.azurewebsites.net";

            var extension_socketio = @"/socket.io";
            var endpoint_full_url =  endpoint + extension_socketio;

            _socketIOClient = new Client(endpoint_full_url);

            _socketIOClient.Opened += SocketOpened;
            _socketIOClient.Message += SocketMessage;
            _socketIOClient.SocketConnectionClosed += SocketConnectionClosed;
            _socketIOClient.Error += SocketError;

            #region async lambda handler
            _socketIOClient.On("connect", async (fn) =>
            {
                if (_socketIOClient.IsConnected)
                {
                    // Implémentation des namespaces à revoir dans Socketio4WinRT
                    //IEndPointClient ns = await _socketIOClient.ConnectEndpointAsync(socketio_namespace);
                    
                    try
                    {
                        CoreDispatcher dispatcher = this.Dispatcher;
                        await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {

                            TextStatusSocketio.Text = "Connexion Socket.io établie";
                        });
                        await _dataProvider.InitializeSensorAsync();

                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine(x.Message);
                        throw;
                    }
                }
            });
            #endregion 

            await _socketIOClient.ConnectAsync();
            
         }

        private void SocketError(object sender, ErrorEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void SocketConnectionClosed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void SocketMessage(object sender, MessageEventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void SocketOpened(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private async void btnStopSensor_Click(object sender, RoutedEventArgs e)
        {
            //stop sensor
            await _dataProvider.StopSensorMeasureAsync();

            //Socket.IO
            if (_socketIOClient != null)
            {
                _socketIOClient.Close();
                _socketIOClient.Dispose();
                _socketIOClient = null;
            }
        }
    }
}
