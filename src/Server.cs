using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Use a different port to avoid conflicts
int port = 6379; // Change to any available port number
TcpListener server = new TcpListener(IPAddress.Any, port);

server.Start(); // wait for client
Console.WriteLine($"Server started on port {port}");
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
                var data = PartitionRequest(receivedMessage);
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

string[] PartitionRequest(string request) {
    // Split the request by spaces and newlines
    return request.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
}

string HandleParsing(string[] request) {
    string reply = "Nope";
     for (int i = 0; i < request.Length; i++){
        Console.WriteLine($"Request[{i}]: {request[i]}");
     }
    switch (request[0].ToLower()) {
        case "ping":
            reply = "+PONG\r\n";
            break;
        case "echo":
            reply = $"${request[1].Length}\r\n{request[1]}\r\n";
            break;
        case "set":
            if (request.Length > 3 && request.Length > 5 && request[3].ToLower() == "px") {
                Console.WriteLine($"Key: {request[1]}, Value: {request[2]}, Expiry: {request[4]}");
                dict[request[1]] = new DataType { value = request[2], expiryTime = DateTime.Now.AddMilliseconds(int.Parse(request[4])) };
                StartExpiryTask(request[1], int.Parse(request[4]));
                reply = "+OK\r\n";
            } else if (request.Length > 2) {
                dict[request[1]] = new DataType { value = request[2], expiryTime = DateTime.Now.AddSeconds(100000) };
                reply = "+OK\r\n";
            }
            break;
        case "get":
            var key = request[1];
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
