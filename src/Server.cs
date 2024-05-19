using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);

server.Start(); // wait for client
var socket = server.AcceptSocket(); // wait for client
 var pong = Encoding.UTF8.GetBytes("+PONG\r\n");
while (socket.Connected){
    byte[] buffer = new byte[socket.ReceiveBufferSize];
    await socket.ReceiveAsync(buffer);
    var command = Encoding.UTF8.GetString(buffer);
   
       await  socket.SendAsync(pong);
    
}