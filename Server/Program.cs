//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace Server
//{
//    class Program
//    {
//        static Dictionary<string, List<Socket>> topicSubscriptions = new Dictionary<string, List<Socket>>();

//        static void Main(string[] args)
//        {
//            StartServer();
//        }

//        static void StartServer()
//        {
//            // Create a new socket object
//            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

//            // Set the IP address and port number for the server
//            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
//            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, 5000);

//            // Bind the socket to the IP address and port number
//            server.Bind(ipEndPoint);

//            // Start listening for client connections
//            server.Listen(10);

//            Console.WriteLine("Server started. Listening for incoming connections...");

//            while (true)
//            {
//                // Accept incoming client connections
//                Socket client = server.Accept();

//                // Start a new thread to handle communication with the client
//                Task.Run(() =>
//                {
//                    byte[] buffer = new byte[1024];

//                    // Get the remote endpoint of the client
//                    EndPoint remoteEndPoint = client.RemoteEndPoint;

//                    // Receive the topics that the client wants to subscribe to
//                    int bytesReceived = client.Receive(buffer);
//                    string topics = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
//                    string[] topicList = topics.Split(',');

//                    // Display a message indicating that the client has connected
//                    Console.WriteLine($"\nClient on {((IPEndPoint)remoteEndPoint).Port} connected.");

//                    // Notify the server which topics the client has chosen
//                    Console.WriteLine("\nClient on port " + ((IPEndPoint)remoteEndPoint).Port + " subscribed to topics: " + topics);

//                    // Add the client socket to the appropriate topic subscription list
//                    foreach (string topic in topicList)
//                    {
//                        if (!topicSubscriptions.ContainsKey(topic))
//                        {
//                            topicSubscriptions.Add(topic, new List<Socket>());
//                        }
//                        topicSubscriptions[topic].Add(client);
//                    }

//                    // Display all clients in each topic
//                    var groupedByTopic = topicSubscriptions.GroupBy(x => x.Key);
//                    foreach (var group in groupedByTopic)
//                    {
//                        string topic = group.Key;
//                        List<Socket> clients = group.SelectMany(x => x.Value).ToList();
//                        int totalClient = clients.Count;
//                        Console.WriteLine($"\nTopic {topic} - {totalClient} client(s)");

//                        foreach (Socket client in clients)
//                        {
//                            EndPoint clientEndPoint = client.RemoteEndPoint;
//                            int clientPort = ((IPEndPoint)clientEndPoint).Port;

//                            Console.WriteLine($"  Client: {client.RemoteEndPoint.ToString()} (Port {clientPort})");
//                        }
//                    }

//                    // Start a loop to receive messages from the client
//                    while (true)
//                    {
//                        try
//                        {
//                            // Receive data from the client
//                            bytesReceived = client.Receive(buffer);

//                            // Convert the received data to a string
//                            string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

//                            // Get the port number of the remote endpoint
//                            int port = ((IPEndPoint)remoteEndPoint).Port;

//                            // Display the received message and the port number of the client
//                            Console.WriteLine("\nReceived from client on port " + port + ": " + message);

//                            // Get the topic and message content from the received message
//                            string[] messageParts = message.Split(':');
//                            string topic = messageParts[0];
//                            //string messageContent = messageParts[1];

//                            // Broadcast the received message to all clients subscribed to the appropriate topic
//                            // Checks whether the topicSubscriptions dictionary contains a key that matches the specified topic
//                            if (topicSubscriptions.ContainsKey(topic))
//                            {
//                                foreach (Socket connectedClient in topicSubscriptions[topic])
//                                {
//                                    EndPoint connectedClientEndPoint = connectedClient.RemoteEndPoint;
//                                    int connectedClientPort = ((IPEndPoint)connectedClientEndPoint).Port;

//                                    if (connectedClient != client)
//                                    {
//                                        byte[] messageBytes = Encoding.ASCII.GetBytes(message);
//                                        connectedClient.Send(messageBytes, messageBytes.Length, SocketFlags.None);
//                                    }
//                                }
//                            }
//                        }
//                        catch
//                        {
//                            // Client disconnected
//                            Console.WriteLine("\nClient on port " + ((IPEndPoint)remoteEndPoint).Port + " disconnected.");

//                            // Remove the client socket from all topic subscription lists
//                            foreach (string topic in topicList)
//                            {
//                                if (topicSubscriptions.ContainsKey(topic))
//                                {
//                                    topicSubscriptions[topic].Remove(client);
//                                }
//                            }

//                            return;
//                        }
//                    }
//                });
//            }
//        }
//    }
//}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Example
{
    public class TcpServer
    {
        private readonly Socket server;
        private readonly Dictionary<string, List<Socket>> topicSubscriptions;

        public TcpServer()
        {
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            topicSubscriptions = new Dictionary<string, List<Socket>>();
        }
        static void Main(string[] args)
        {
            TcpServer server = new TcpServer();
            server.Start(5000);

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        public void Start(int port)
        {
            server.Bind(new IPEndPoint(IPAddress.Any, port));
            server.Listen(10);
            Console.WriteLine("Server started on port " + port);

            while (true)
            {
                Socket client = server.Accept();
                Task.Run(() => HandleClient(client));
            }
        }

        private void HandleClient(Socket client)
        {
            byte[] buffer = new byte[1024];
            EndPoint remoteEndPoint = client.RemoteEndPoint;

            // Receive the topics that the client wants to subscribe to
            int bytesReceived = client.Receive(buffer);
            string topics = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
            string[] topicList = topics.Split(',');

            // Display a message indicating that the client has connected
            Console.WriteLine($"\nClient on {((IPEndPoint)remoteEndPoint).Port} connected.");

            // Notify the server which topics the client has chosen
            Console.WriteLine("\nClient on port " + ((IPEndPoint)remoteEndPoint).Port + " subscribed to topics: " + topics);

            // Add the client socket to the appropriate topic subscription list
            foreach (string topic in topicList)
            {
                if (!topicSubscriptions.ContainsKey(topic))
                {
                    topicSubscriptions.Add(topic, new List<Socket>());
                }
                topicSubscriptions[topic].Add(client);
            }

            DisplayAllClientsByTopic();

            while (true)
            {
                try
                {
                    // Receive data from the client
                    bytesReceived = client.Receive(buffer);

                    // Convert the received data to a string
                    string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

                    // Get the port number of the remote endpoint
                    int port = ((IPEndPoint)remoteEndPoint).Port;

                    // Display the received message and the port number of the client
                    Console.WriteLine("\nReceived from client on port " + port + ": " + message);

                    // Get the topic and message content from the received message
                    string[] messageParts = message.Split(':');
                    string topic = messageParts[0];

                    // Broadcast the received message to all clients subscribed to the appropriate topic
                    if (topicSubscriptions.ContainsKey(topic))
                    {
                        foreach (Socket connectedClient in topicSubscriptions[topic])
                        {
                            if (connectedClient != client)
                            {
                                byte[] messageBytes = Encoding.ASCII.GetBytes(message);
                                connectedClient.Send(messageBytes, messageBytes.Length, SocketFlags.None);
                            }
                        }
                    }
                }
                catch (SocketException)
                {
                    // Client has disconnected
                    Console.WriteLine($"Client on port {((IPEndPoint)remoteEndPoint).Port} disconnected.");
                    RemoveClient(client);
                    DisplayAllClientsByTopic();
                    break;
                }
            }
        }

        private void RemoveClient(Socket client)
        {
            foreach (var topicSubscription in topicSubscriptions.Values)
            {
                topicSubscription.Remove(client);
            }
        }

        private void DisplayAllClientsByTopic()
        {
            var groupedByTopic = topicSubscriptions.GroupBy(x => x.Key);

            foreach (var group in groupedByTopic)
            {
                string topic = group.Key;
                List<Socket> clients = group.SelectMany(x => x.Value).ToList();
                int totalClient = clients.Count;

                Console.WriteLine($"\nTopic {topic} - {totalClient} client(s)");

                foreach (Socket client in clients)
                {
                    EndPoint clientEndPoint = client.RemoteEndPoint;
                    int clientPort = ((IPEndPoint)clientEndPoint).Port;
                    Console.WriteLine($"  Client: {client.RemoteEndPoint} (Port {clientPort})");
                }
            }
        }
    }
}
