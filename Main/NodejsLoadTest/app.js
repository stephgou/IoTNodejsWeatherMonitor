    // -----------------------------------------------------------------------------------------------------------------------------
    //   Initialisation du programme
    // -----------------------------------------------------------------------------------------------------------------------------

console.log("NodjsLoadTest : Demarrage du programme");

var io = require('socket.io-client-matchall'),
    moment = require('moment'),
    measure = require('scripts/measure').init();

var endpoint = 'http://localhost:888';
    // CTRL F5
//var endpoint = 'http://xxx.azurewebsites.net';

var namespace = '/iot-node-hub';
var endpoint_full_url = endpoint + namespace;

var urls = [endpoint_full_url], sockets = [];

var devices = [],
    timeouts = [],
    newUserInterval = 5000;

for (var i = 0; i < 20; i++) {
    var createTime = (i + 1) * newUserInterval;
    console.log('Creation des devices simules dans ' + createTime + 'ms');

    setTimeout(function () {
        //intervalle en milliseconds
        var sendInterval = 100;
        var disconnectTime = 20000;
        devices[i] = new SimulatedDevice(urls[0], sendInterval, disconnectTime);
    }, createTime);
};
console.log("Lancement du test de charge dans 10 secondes ...");

function SimulatedDevice(url, interval, disconnectTime) {
    var that = this, clicks = 0, isConnected;

    var client = io.connect(url, { 'sync disconnect on unload': true });
    client.on('msgtocli', function (data) {
        processReceivedData(data);
    });

    client.on('connect', function () {
        isConnected = true;
        console.log('Connecté à ' + url + ', Session ID=' + client.socket.sessionid);
        //console.log('send interval: ' + interval + ', disconnect after: ' + moment().add('milliseconds', disconnectTime).format());
        //console.log('Time: ' + moment().format());

        startSending();

        setTimeout(function () {
            disconnect();
        }, disconnectTime);
    });

    client.on('connect_failed', function (reason) {
        console.log('Connexion impossible ' + url + ": " + reason);
    });

    function disconnect() {
        console.log('Déconnexion de ' + url + ', session ID=' + client.socket.sessionid);
        isConnected = false;
        client.disconnect();
        client = null;
    }

    function processReceivedData(data) {
        if (data.type === 'init') {
            console.log('initialisation de la socke ' + data.message);
        }
        if (data.type === 'meteo') {
            console.log('Meteo: ' + data.message.temperature + ' - ' + data.message.humidity + ' - ' + data.message.utcdate);
        }
    }

    function send() {
        if (!isConnected) {
            return;
        }
        client.emit('msgtosrv', measure.buildMeteoMessage());
        console.log('Invocation of msgtosrv from ' + client.socket.sessionid + ' to ' + url + ' succeeded at ' + moment().format());
    }

    function startSending() {
        setInterval(function () {
            send();
        }, interval);
    }

    return {
        client: client
    };
};
