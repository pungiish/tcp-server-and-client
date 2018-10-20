var WebSocket = require("ws")

const socket= new WebSocket("ws://127.0.0.1:1234");
data1 = "Hello Server!";

socket.on('open', function open() {
    console.log("open");
    
    socket.send("something");
})
socket.addEventListener('message', function mssg(event) {
    console.log("Message");
    console.log(event);
})

socket.addEventListener('message', function (event) {
    console.log('Message from server ', event.data);
});

