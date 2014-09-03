// -----------------------------------------------------------------------------------------------------------------------------
    //   Initialisation du test Notification Hub
// -----------------------------------------------------------------------------------------------------------------------------

console.log("NodejsHub : Demarrage du test Notification Hub");

var NOTIFICATION_CONNECTION_STRING = "Endpoint=sb:...";
var NOTIFICATION_HUB_PATH = "NomDuHub";

var readline = require("readline"),
    prompt = readline.createInterface(process.stdin, process.stdout),
    azure = require("azure"),
    notification = azure.createNotificationHubService(NOTIFICATION_HUB_PATH, NOTIFICATION_CONNECTION_STRING);

console.log("Appuyer sur ENTER pour démarrer le test");
prompt.on('line', function (line) {
    console.log("Envoi d'un message vers Notification Hub");
    SendTemperatureNotification(notification, 10);
}).on('close', function () {
    console.log("NodejsHub : Fin du test Notification Hub");
    process.exit(0);
});

function SendTemperatureNotification(notification, temperature) {
    var toast = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
        "<toast>" +
        "<visual>" +
        "<binding template=\"ToastText04\">" +
            "<text id=\"1\">Alerte temperature</text>" +
            "<text id=\"2\">Derni&amp;re mesure : " + temperature + " °C</text>" +
            "</binding>" +
        "</visual>" +
        "</toast>";

    //http://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references

    //var optionsOrCallback = { 'headers:': 'content-type:application/atom+xml;type=entry;charset=utf-8' };
    //var optionsOrCallback = { 'headers:': 'content-type:charset=utf-8,content-length:' + toast.length };
    var optionsOrCallback = 3;

    notification.wns.send(null, toast, "wns/toast", optionsOrCallback, function (error) {
        if (error) {
            console.error(error);
        }

    });
}

