var IOTMonitor;
(function (IOTMonitor) {
    var SocketioDashboardViewModel = (function () {
        function SocketioDashboardViewModel(jQuerySelector) {
            this.jQuerySelector = jQuerySelector;
            this.connectionState = ko.observable("Disconnected");
            this.innerTemperatureData = [];
            this.innerHumidityData = [];
            this.temperatureData = ko.observable({
                labels: ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                datasets: [
                {
                    fillColor: "rgba(220,220,220,0.5)",
                    strokeColor: "rgba(220,220,220,1)",
                    pointColor: "rgba(220,220,220,1)",
                    pointStrokeColor: "#000",
                    data: []
                }]
            });

            this.humidityData = ko.observable({
                labels: ["", "", "", "", "", "", "", "", "", "", "", "", "", "", ""],
                datasets: [
                {
                    fillColor: "rgba(151,187,205,0.5)",
                    strokeColor: "rgba(151,187,205,1)",
                    pointColor: "rgba(151,187,205,1)",
                    pointStrokeColor: "#000",
                    data: []
                }]
            });
        }

        SocketioDashboardViewModel.prototype.initialize = function () {

            var endpoint = 'http://localhost:888';
            //var endpoint = 'http://xxx.azurewebsites.net:80'; // si IOTWebAPIFrontEnd lancé en local
            //var endpoint = 'http://xxx.azurewebsites.net';
            //var namespace = '/iot-node-hub';
            // Implémentation des namespaces à revoir dans Socketio4WinRT

            var endpoint_full_url = endpoint;// + namespace;
            var disconnectTime = 200000;

            var self = this;

            console.log('Connexion du portail sur ' + endpoint_full_url);
            var socket = io.connect(endpoint_full_url, { 'connect timeout': 2000 });
            socket.on('connect', function () {
                isConnected = true;
                self.connectionState("Connected");

                console.log('Connecté à ' + endpoint_full_url + ', Session ID=' + socket.sessionid);

                setTimeout(function () {
                    disconnect();
                }, disconnectTime);
            });

            socket.on('connect_failed', function (reason) {
                console.log('Connexion impossible ' + endpoint_full_url + ": " + reason);
            });

            socket.on('msgtocli', function (data) {
                processReceivedData(data);
            });

            function disconnect() {
                console.log('Déconnexion de ' + endpoint_full_url + ', session ID=' + socket.sessionid);
                isConnected = false;
                self.connectionState("Disconnected");
                socket.disconnect();
                client = null;
            }

            function processReceivedData(data) {
                if (data.type === 'init') {
                    console.log('initialisation de la socket ' + data.message);
                }
                if (data.type === 'meteo') {
                    console.log('Meteo: ' + data.message.temperature + ' - ' + data.message.humidity + ' - ' + data.message.utcDate);
                    self.onNewMeasureAvailable(data.message);
                }
            }

            var elementToBind = $(this.jQuerySelector).get(0);
            if (elementToBind) {
                ko.applyBindings(this, elementToBind);
            }
            this.initCharts();
        };

        SocketioDashboardViewModel.prototype.onNewMeasureAvailable = function (measure) {
            var self = this;

            console.log(JSON.stringify(measure));

            if (self.temperatureData().datasets[0].data.length == 15) {
                self.temperatureData().datasets[0].data.shift();
            }
            self.temperatureData().datasets[0].data.push(measure.temperature);

            if (self.humidityData().datasets[0].data.length == 15) {
                self.humidityData().datasets[0].data.shift();
            }
            self.humidityData().datasets[0].data.push(measure.humidity);

            self.initCharts();
        };

        SocketioDashboardViewModel.prototype.initCharts = function () {
            var temperatureCanvasCtx = $(this.jQuerySelector + " #temperatureCanvas").get(0).getContext("2d");
            var humidityCanvasCtx = $(this.jQuerySelector + " #humidityCanvas").get(0).getContext("2d");

            var optionsTemp = {
                animation: false,
                scaleOverride: true,
                scaleSteps: 5,
                scaleStepWidth: 10,
                scaleStartValue: 0
            };

            var temperatureLine = new Chart(temperatureCanvasCtx).Line(this.temperatureData(), optionsTemp);

            var optionsHumidity = {
                animation: false,
                scaleOverride: true,
                scaleSteps: 10,
                scaleStepWidth: 10,
                scaleStartValue: 0
            };

            var humidityLine = new Chart(humidityCanvasCtx).Line(this.humidityData(), optionsHumidity);


        };

        return SocketioDashboardViewModel;
    })();

    IOTMonitor.SocketioDashboardViewModel = SocketioDashboardViewModel;
})(IOTMonitor || (IOTMonitor = {}));