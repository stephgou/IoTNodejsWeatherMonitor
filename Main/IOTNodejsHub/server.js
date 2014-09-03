    // -----------------------------------------------------------------------------------------------------------------------------
    //   Initialisation du service
    // -----------------------------------------------------------------------------------------------------------------------------

console.log("NodejsHub :  Demarrage du service");

var port = process.env.port || 888;
var NOTIFICATION_CONNECTION_STRING = "Endpoint=sb:...";
var NOTIFICATION_HUB_PATH = "NomduHub";

console.log("Port :  " + port);

    // Import des modules http, path, socket.io, http, puis création du serveur http et démarrage de l'écoute des requêtes sur socket.io

var fs = require('fs'),
    path = require('path'),
    // Create an HTTP server so that we can listen for, and respond to
    // incoming HTTP requests. This requires a callback that can be used
    // to handle each incoming request.
    http = require('http').createServer(handler),
    io = require('socket.io').listen(http, { origins: '*:*' }),
    azure = require("azure"),
    notification = azure.createNotificationHubService(NOTIFICATION_HUB_PATH, NOTIFICATION_CONNECTION_STRING),
    hub = require('scripts/hub');

    // start listening for HTTP/socket.io connections
http.listen(port);

    // -----------------------------------------------------------------------------------------------------------------------------
    // Fonction liée au traitement des requêtes sur le serveur HTTP
    // -----------------------------------------------------------------------------------------------------------------------------
function handler(request, response) {

    // When dealing with CORS (Cross-Origin Resource Sharing)
    // requests, the client should pass-through its origin (the
    // requesting domain). We should either echo that or use *
    // if the origin was not passed.
    var origin = (request.headers.origin || "*");

    // Check to see if this is a security check by the browser to
    // test the availability of the API for the client. If the
    // method is OPTIONS, the browser is check to see to see what
    // HTTP methods (and properties) have been granted to the
    // client.
    if (request.method.toUpperCase() === "OPTIONS") {

        // Echo back the Origin (calling domain) so that the
        // client is granted access to make subsequent requests
        // to the API.
        response.writeHead(
            "204",
            "No Content",
            {
                "access-control-allow-origin": origin,
                "access-control-allow-methods": "GET, POST, PUT, DELETE, OPTIONS",
                "access-control-allow-headers": "content-type, accept, X-Requested-With",
                "content-type": "text/html; charset=utf-8",
                "access-control-max-age": 10, // Seconds.
                "content-length": 0
            }
        );

        // End the response - we're not sending back any content.
        return (response.end());
    }



    fs.readFile(path.resolve(__dirname, 'index.html'),
    function (err, data) {
        if (err) {
            response.writeHead(500);
            return response.end('Error loading index.html');
        }
        // Send the headers back. Notice that even though we
        // had our OPTIONS request at the top, we still need
        // echo back the ORIGIN in order for the request to
        // be processed on the client.
        response.writeHead(
            "200",
            "OK",
            {
                "Access-Control-Allow-Origin": origin,
                "Access-Control-Allow-Headers": "Content-Type, Authorization, Content-Length, X-Requested-With",
                "content-type": "text/html; charset=utf-8",
                "content-length": data.length
            }
        );

        // Close out the response.
        response.end(data);
    }
);
}

    // -----------------------------------------------------------------------------------------------------------------------------
    // Fonctions et appels liés au traitement des requêtes sur socket.io
    // -----------------------------------------------------------------------------------------------------------------------------

    // Choix du protocole de transport  
io.configure(function () {
    //io.set('transports', ['websocket', 'flashsocket', 'htmlfile', 'xhr-polling', 'json-polling']);
    io.set('origins', '*:*');
//    io.set('transports', ['websocket', 'xhr-polling']);
    io.set('transports', ['websocket']);

});

hub.init(io, notification);

