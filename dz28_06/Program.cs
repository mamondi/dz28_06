using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var serverTask = StartServer();

        await Task.Delay(1000);

        await StartClient();

        await serverTask;
    }

    static async Task StartServer()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 8888);
        server.Start();
        Console.WriteLine("Сервер запущено. Очiкування пiдключень...");

        TcpClient client = await server.AcceptTcpClientAsync();
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[256];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string clientRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

        string response = "";
        if (clientRequest == "TIME")
        {
            response = DateTime.Now.ToString("HH:mm:ss");
        }
        else if (clientRequest == "DATE")
        {
            response = DateTime.Now.ToString("yyyy-MM-dd");
        }

        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);

        stream.Close();
        client.Close();
        server.Stop();
    }

    static async Task StartClient()
    {
        TcpClient client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", 8888);
        NetworkStream stream = client.GetStream();

        Console.WriteLine("Введiть 'TIME' для запиту часу або 'DATE' для запиту дати:");
        string userInput = Console.ReadLine().ToUpper();

        byte[] data = Encoding.UTF8.GetBytes(userInput);
        await stream.WriteAsync(data, 0, data.Length);

        byte[] buffer = new byte[256];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string serverResponse = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        string serverIP = client.Client.RemoteEndPoint.ToString();
        Console.WriteLine($"О {DateTime.Now:HH:mm} вiд {serverIP} отримано вiдповiдь: {serverResponse}");

        stream.Close();
        client.Close();
    }
}
