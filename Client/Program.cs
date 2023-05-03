using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        private static readonly Socket ClientSocket = new Socket(AddressFamily
                .InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private const int port = 15000;

        static void Main(string[] args)
        {
            // Check if the server has started or not
            IsConnected();

            /*string ipAddress = "127.0.0.1";
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            ClientSocket.Connect(endPoint);
            Console.WriteLine("Client " + ClientSocket.RemoteEndPoint + ":is connected.");*/

            // Start querying messages
            while (true)
            {
                // Receive the message from ther server
                byte[] serverMessage = new byte[1024];
                int size = ClientSocket.Receive(serverMessage);
                Console.WriteLine(Encoding.ASCII.GetString(serverMessage, 0, size));
                Console.WriteLine("-----------------------------------------------------");

                // Let client enter query
                string clientMessage = string.Empty;
                Console.WriteLine("Enter your query based on either of the formats below: @ * <sub> <topic> @ * <topic> <message> @ * <exit>"
                    .Replace("@", Environment.NewLine));
                clientMessage = Console.ReadLine();

                //Console.Write(clientMessage);

                if (clientMessage != null)
                {
                    ClientSocket.Send(Encoding.ASCII.GetBytes(clientMessage), 0, clientMessage.Length, SocketFlags.None);
                    if (clientMessage == "exit")
                    {
                        ClientSocket.Shutdown(SocketShutdown.Both);
                        ClientSocket.Close();
                        return;
                    }
                }
                else
                    Console.WriteLine("You didn't write anything.");
            }
        }

        private static void IsConnected()
        {
            while (!ClientSocket.Connected)
            {
                try
                {
                    Console.WriteLine("Waiting for connection.");
                    ClientSocket.Connect(IPAddress.Loopback, port);
                }
                catch (SocketException)
                {
                    Console.Clear();
                }
            }
        }
    }
}