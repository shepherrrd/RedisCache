using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;



// Uncomment this block to pass the first stage
 class Server {
     static async Task Main(string[] args)
    {
        int port = 6379; // Default port

        // Parse command-line arguments to get the port number
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--port" && i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedPort))
            {
                port = parsedPort;
                break;                
            }
        }

        // Start the server with the specified port
      
    
    // You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");
TcpListener server = new TcpListener(IPAddress.Any, args.Length > 0 ? int.Parse(args[0]) : port);

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
                    string responseMessage = HandleParsing(data,dict);
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
    }
 

static string HandleParsing(string[] request, ConcurrentDictionary<string, DataType> dict) {
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
            Console.WriteLine($"Key: {request[4]}, Value: {request[6]}");
                if (request.Length > 10 && request[10] != null && request[8] != null) {
                if (request[10].Length > 0 && request[8].ToLower() == "px") {
                    Console.WriteLine($"Key: {request[4]}, Value: {request[6]}, Expiry: {request[10]}");
                    dict[request[4]] = new DataType { value = request[6], expiryTime = DateTime.Now.AddMilliseconds(int.Parse(request[10])) };
                    StartExpiryTask(request[4], int.Parse(request[10]), dict);
                    reply = "+OK\r\n";
                    break;
                }
            }
                Console.WriteLine($"Keysss: {request[4]}, Value: {request[6]},");

             dict[request[4]] = new DataType { value = request[6], expiryTime = DateTime.Now.AddMilliseconds(100000) };
                StartExpiryTask(request[4], 100000, dict);

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

static async Task SendResponse(Socket clientSocket, string response) {
    byte[] responseMessage = Encoding.UTF8.GetBytes(response);
    await clientSocket.SendAsync(responseMessage, SocketFlags.None);
}

static async void StartExpiryTask(string key, int delayMilliseconds, ConcurrentDictionary<string, DataType> dict) {
    await Task.Delay(delayMilliseconds);
    dict.TryRemove(key, out _);
}
 
public class DataType {
    public string value { get; set; } = default!;
    public DateTime expiryTime { get; set; } = default!;
}
 }