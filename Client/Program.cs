using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            StartClient().Wait();
        }

        static async Task StartClient()
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 5000);

            await client.ConnectAsync(ipEndPoint);
            Console.WriteLine("Connected to server.");

            Console.WriteLine("\nAvailable topics: A, B, C.");
            Console.Write("Enter topics to subscribe (comma-separated): ");
            string topics = Console.ReadLine().ToUpper();

            byte[] buffer = Encoding.ASCII.GetBytes(topics);
            await client.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);

            Console.WriteLine("Subscribed to topics: " + topics);

            // Allow the client to receive messages from the server while still being able to send messages.
            Task.Run(() => ReceiveLoop(client));

            while (true)
            {
                Console.Write("\nEnter message (topic:message): ");
                string input = Console.ReadLine();
                string[] parts = input.Split(':');
                if (parts.Length != 2)
                {
                    Console.WriteLine("Invalid input format. Please enter in the format: topic:message");
                    continue;
                }
                string topic = parts[0].ToUpper();
                if (!topics.Contains(topic))
                {
                    Console.WriteLine($"You are not subscribed to topic {topic}.");
                    continue;
                }
                string message = parts[1];
                string data = $"{topic}:{message}";
                buffer = Encoding.ASCII.GetBytes(data);
                await client.SendAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
            }
        }

        static async Task ReceiveLoop(Socket client)
        {
            byte[] buffer = new byte[1024];
            while (true)
            {
                int bytesRead = await client.ReceiveAsync(new ArraySegment<byte>(buffer), SocketFlags.None);
                if (bytesRead == 0)
                {
                    Console.WriteLine("\nServer closed connection.");
                    break;
                }
                string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                string[] parts = response.Split(':');
                string topic = parts[0];
                string message = parts[1];
                Console.WriteLine($"\nReceived message from topic {topic}: {message}");
            }
        }
    }
}
