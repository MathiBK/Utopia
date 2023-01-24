"use strict";

let connection = new signalR.HubConnectionBuilder().withUrl("/huntHub").build();

console.log(connection)


connection.on("ReceiveHunted", function (message, individualId) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = " says " + msg;
    console.log(message);
    console.log(individualId);
    
    $.ajax({
        url: "/Home/HuntConfirm",

        data: "speciesIndividId=" + individualId,
        type: "GET",
        success: function(data)
        {
            $("#HuntData").html(data);

        },
        error: function(passParams)
        {
            console.log(passParams)
        }
    });
});

connection.on("AnimalSlain", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = " says " + msg;
    console.log(message);
    
    $("#HuntData").html(message);


});

connection.on("RefreshResources", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    message = JSON.parse(message);
    for (const messageKey in message) 
    {
        $("#" + messageKey + "Name").html(messageKey);
        $("#" + messageKey + "Data").html(message[messageKey]);
        
    }
});

connection.on("RefreshItems", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    message = JSON.parse(message);
    console.log(message)
    for (const messageKey in message)
    {
        $("#" + messageKey + "Name").html(messageKey);
        $("#" + messageKey + "Data").html(message[messageKey]);

    }
});


connection.on("ShowVisibleTiles", function() {
    getVisibleTiles();

});
connection.on("GetTribe", function () {

    console.log("Getting tribes...")
    $.ajax({
        url: "/Home/GetTribeInfo",

        type: "GET",
        success: function(data)
        {
            console.log("running whichtiles")
            $("#tribeData").html(data);

        },
        error: function(passParams)
        {
            console.log(passParams)
        }
    });
});


connection.on("UpdateCurrentTile", function (message) {
    console.log(message)
    currentPlayerTileId = message;
    console.log(currentPlayerTileId)
});

connection.on("UpdatePlayerInfo", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    console.log("updateplayerinfo")
    message = JSON.parse(message);
    console.log(message);
    for (const messageKey in message)
    {
        $("#" + messageKey + "Info").html(message[messageKey]);

    }
});



connection.on("PlayerDeath", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    message = JSON.parse(message);
    alert("You died.");
});


connection.start().then(function () {
    console.log("started signalR!");
}).catch(function (err) {
    return console.error(err.toString());
});
