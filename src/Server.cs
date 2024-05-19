using System.Net;
using System.Net.Sockets;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 6379);
var arg = Console.ReadLine()!.Split(' ');
if ( arg.Length == 1 && arg[0] == "PING" )
{
    if(server.Server.IsBound && arg[1] == null)
    {
        Console.WriteLine("+PONG\r\n");
    }
    else
    {
        Console.WriteLine(arg[1] + "\r\n");
    }
}
server.Start();
server.AcceptSocket(); // wait for client
