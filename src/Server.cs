using System.Net;
using System.Net.Sockets;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);

server.Start(); // wait for client// wait for client
 int clientId = 1;
while (true) {
  Socket clientSocket = await server.AcceptSocketAsync(); 
  HandleSocketConnection(clientSocket, clientId++);
}
static async void HandleSocketConnection(Socket clientSocket, int clientId) {
  try {
    while (true) {
      byte[] databuffer = new byte[1024];
      int bytesRead = await clientSocket.ReceiveAsync(databuffer);
      string recivedMessage = Encoding.UTF8.GetString(databuffer, 0, bytesRead);
      Console.WriteLine(
          $"Recived Message: {recivedMessage}, clientId: {clientId}");
      string responseString = "+PONG\r\n";
      byte[] responseMessage = Encoding.UTF8.GetBytes(responseString);
      await clientSocket.SendAsync(responseMessage);
    }
  } catch (Exception ex) {
    throw new Exception($"Error Happened in this client {ex.Message}");
  } finally {
    clientSocket.Close();
  }
}