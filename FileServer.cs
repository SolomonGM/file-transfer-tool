using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

class FileServer
{
   //Initialize a port for the server and a directory for the saved files
    private const int Port = 8888;
    private const string SaveDirectory = "ReceievedFiles";

    static async Task Main(string[] args)
    {
        try
        {
            //Ensure that the directory for the files exists, if not create it.
            if (!Directory.Exists(SaveDirectory))
            {
                Directory.CreateDirectory(SaveDirectory);   
            }

            //Initialize the server
            TcpListener server = new TcpListener(IPAddress.Any, Port);
            server.Start();
            Task.Delay(5000).Wait();
            Console.WriteLine("Server Started...")

            while (true)
            {
                TcpClient client = new TcpClient();

                //Handle all clients in another Task method
                _ = Task.Run(() => HandleClient(client));
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to connect clients to server: {ex.Message}");
        }
    }

    static async Task HandleClient(TcpClient client)
    {
        try
        {
            Console.WriteLine($"Client connected: {((IPEndPoint)client.Client.RemoteEndPoint).Address}:{((IPEndPoint)client.Client.RemoteEndPoint).Port}");

            using (NetworkStream networkStream = client.GetStream())
            using (StreamReader reader = new StreamReader(networkStream))
            {
                //Read the file pathfrom the client
                string fileName = await reader.ReadLineAsync();
                string filePath = Path.Combine(SaveDirectory, fileName);

                //opens a fileStream to save a copy of the recieved file
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    //Read the file content from the client and save it
                    await networkStream.CopyToAsync(fileStream);
                }

                Console.WriteLine($"File recieved and saved: {filePath}");
            }
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling clients: {ex.Message}");
        }
        finally
        {
            client.Close();
        }
    }
}