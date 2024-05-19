
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);

server.Start(); // wait for client// wait for client
 int clientId = 1;
 var dict = new Dictionary<string, DataType>();
while (true) {
    DeleteFromCache();
  Socket clientSocket = await server.AcceptSocketAsync(); 
  HandleSocketConnection(clientSocket, clientId++);
}

 async void HandleSocketConnection(Socket clientSocket, int clientId) {
  try {
    while (true) {
      byte[] databuffer = new byte[clientSocket.ReceiveBufferSize];
      int bytesRead = await clientSocket.ReceiveAsync(databuffer);
      if (bytesRead > 0) {
       
      string recivedMessage = Encoding.UTF8.GetString(databuffer, 0, bytesRead);
      Console.WriteLine(
          $"Recived Message: {recivedMessage}, clientId: {clientId}");
      var data = recivedMessage.Split("\r\n");
      if(data.Length > 0){
        
      string responseMessage = HandleParsing(data);
       SendResponse(clientSocket, responseMessage);
      }
    }
    }
    
  } catch (Exception ex) {
    throw new Exception($"Error Happened in this client {ex.Message}");
  } finally {
    clientSocket.Close();
  }
}

 string HandleParsing(string[] request) {
  string reply = "Nope";
  switch (request[2].ToLower()) {
  case "ping":
    reply = "+PONG\r\n";
    break;
  case "echo":
    reply = $"${request[4].Length}\r\n{request[4]}\r\n";
    break;
   case "set":
   if(request[6].ToLower() == "px"){
    dict.Add(request[4], new DataType(){value = request[8], time = TimeSpan.FromMilliseconds(int.Parse(request[10]))});
    reply = "+OK\r\n";
    break;
   }
   dict.Add(request[4], new DataType(){value = request[6], time = TimeSpan.FromSeconds(100000)});
    reply = "+OK\r\n";
    break; 
    case "get":
    var res = request[4];
    if(dict.ContainsKey(res)){
      reply = $"${dict[res].value.Length}\r\n{dict[res]}\r\n";
  }else{
    reply = "$-1\r\n";
  }
    break;
   
}
    return reply;
 }
 
 async void SendResponse(Socket clientSocket, string response) {
  byte[] responseMessage = Encoding.UTF8.GetBytes(response);
  await clientSocket.SendAsync(responseMessage);
  }
   void DeleteFromCache(){
    foreach(var key in dict.Keys){
        var now = DateTime.Now;
      if(dict[key].time < now.TimeOfDay){
        dict.Remove(key);
      }    
  }
  }
public class DataType{
  public string value {get; set;} = default!;
  public TimeSpan time {get; set;} = default!;
}