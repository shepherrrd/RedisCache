using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);

server.Start(); // wait for client// wait for client
 while (true) {
  Socket socket = await server.AcceptSocketAsync(); // wait for client
  await HandleSocketAsync(socket);
}
async Task HandleSocketAsync(Socket socket) {
  while (socket.Connected) {
    byte[] requestData = new byte[socket.ReceiveBufferSize];
    await socket.ReceiveAsync(requestData);
    var request =  Encoding.UTF8.GetString(requestData);
    var response = Encoding.UTF8.GetBytes("PONG");
    switch (request) {
    case "PING":
      response = Encoding.UTF8.GetBytes("PONG");
      break;
    default:
      response = Encoding.UTF8.GetBytes("-ERR unknown command\r\n");
      break;
    }
    await socket.SendAsync(response);
  }
}