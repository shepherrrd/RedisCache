using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);

server.Start(); // wait for client
int clientId = 1;
var dict = new ConcurrentDictionary<string, DataType>();

while (true) {
    Socket clientSocket = await server.AcceptSocketAsync();
    HandleSocketConnection(clientSocket, clientId++);
}

async void HandleSocketConnection(Socket clientSocket, int clientId) {
    try {
        while (true) {
            byte[] databuffer = new byte[clientSocket.ReceiveBufferSize];
            int bytesRead = await clientSocket.ReceiveAsync(databuffer, SocketFlags.None);
            if (bytesRead > 0) {
                string receivedMessage = Encoding.UTF8.GetString(databuffer, 0, bytesRead);
                Console.WriteLine($"Received Message: {receivedMessage}, clientId: {clientId}");
                var data = receivedMessage.Split("\r\n");
                if (data.Length > 0) {
                    string responseMessage = HandleParsing(data);
                    await SendResponse(clientSocket, responseMessage);
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
    for (int i = 0; i < request.Length; i++) {
        Console.WriteLine($"Request: {i} - {request[i]}");
    }
    string reply = "Nope";
    switch (request[2].ToLower()) {
        case "ping":
            reply = "+PONG\r\n";
            break;
        case "echo":
            reply = $"${request[4].Length}\r\n{request[4]}\r\n";
            break;
        case "set":         
                if(request[10] != null&& request[8] is not null){
                    if( request[10].Length > 0 && request[8].ToLower() == "px"){

                Console.WriteLine($"Key: {request[4]}, Value: {request[6]}, Expiry: {request[10]}");
                dict[request[4]] = new DataType { value = request[6], expiryTime = DateTime.Now.AddMilliseconds(int.Parse(request[10])) };
                StartExpiryTask(request[4], int.Parse(request[10]));
                reply = "+OK\r\n"; 
                break;
                 }
                }
             dict[request[4]] = new DataType { value = request[6], expiryTime = DateTime.Now.AddMilliseconds(100000) };
                StartExpiryTask(request[4], 100000);

                reply = "+OK\r\n";          
            break;
        case "get":
            var key = request[4];
            if (dict.ContainsKey(key) && dict[key].expiryTime > DateTime.Now) {
                reply = $"${dict[key].value.Length}\r\n{dict[key].value}\r\n";
            } else {
                reply = "$-1\r\n";
            }
            break;
    }
    return reply;
}

async Task SendResponse(Socket clientSocket, string response) {
    byte[] responseMessage = Encoding.UTF8.GetBytes(response);
    await clientSocket.SendAsync(responseMessage, SocketFlags.None);
}

async void StartExpiryTask(string key, int delayMilliseconds) {
    await Task.Delay(delayMilliseconds);
    dict.TryRemove(key, out _);
}

public class DataType {
    public string value { get; set; } = default!;
    public DateTime expiryTime { get; set; } = default!;
}
