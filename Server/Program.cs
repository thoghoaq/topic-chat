using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace MultiServer
{
    class Program
    {
        private static readonly Socket serverSocket = new (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private static readonly List<Socket> clientSockets = new ();
        private const int BUFFER_SIZE = 2048;
        private const int PORT = 100;
        private static readonly byte[] buffer = new byte[BUFFER_SIZE];
        private static readonly List<string> listTopics = new ()
        {
            "A","B","C","D","E","F"
        };
        private static readonly Dictionary<Socket, List<string>> clientTopics = new ();

        static void Main()
        {
            Console.Title = "Server";
            SetupServer();
            Console.ReadLine(); // When we press enter close everything
            CloseAllSockets();
        }

        private static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, PORT));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(AcceptCallback, null);
            Console.WriteLine("Server setup complete");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients).
        /// </summary>
        private static void CloseAllSockets()
        {
            foreach (Socket socket in clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            serverSocket.Close();
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            clientSockets.Add(socket);
            clientTopics.Add(socket, new List<string>()); //Initial list topic of a client
            socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            Console.WriteLine($"Client {clientSockets.IndexOf(socket)} connected, waiting for request...");
            serverSocket.BeginAccept(AcceptCallback, null);
            SendListTopicToClient(socket);
        }

        private static void SendListTopicToClient(Socket socket)
        {
            byte[] data = Encoding.ASCII.GetBytes("Select a topic (using /sub <topic>): " + string.Join(", ", listTopics) + "\nUsing /list to list subcribed topic");
            socket.Send(data);
            Console.WriteLine($"Sended list topic to client {clientSockets.IndexOf(socket)}");
        }

        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Console.WriteLine($"Client {clientSockets.IndexOf(current)} forcefully disconnected");
                // Don't shutdown because the socket may be disposed and its disconnected anyway.
                current.Close();
                clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(buffer, recBuf, received);
            string text = Encoding.ASCII.GetString(recBuf);
            Console.WriteLine($"Received Text from client {clientSockets.IndexOf(current)}: " + text);

            if (text.ToLower() == "get time") // Client requested time
            {
                Console.WriteLine("Text is a get time request");
                byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
                current.Send(data);
                Console.WriteLine("Time sent to client");
            }
            else if (text.ToLower() == "exit") // Client wants to exit gracefully
            {
                // Always Shutdown before closing
                current.Shutdown(SocketShutdown.Both);
                current.Close();
                clientSockets.Remove(current);
                Console.WriteLine("Client disconnected");
                return;
            }
            else if (text.StartsWith("/"))
            {
                if (text.StartsWith("/sub ")) // Client subcribe topic
                {
                    var topic = listTopics.FirstOrDefault(e => e.Equals(text.Substring(5).Trim()));
                    if (topic == null) // Not found topic
                    {
                        current.Send(Encoding.ASCII.GetBytes("Not found topic"));
                    }
                    else
                    {
                        AddIfNotExists(clientTopics.SingleOrDefault(e => e.Key.Equals(current)).Value, topic);
                        current.Send(Encoding.ASCII.GetBytes("Subcribed topic " + topic + "\nUsing /send \"<topic>\" \"<message>\""));
                    }
                }
                else if (text.StartsWith("/send "))
                {
                    string pattern = @"""([^""]+)""\s+""([^""]+)"""; // Matches two quoted strings

                    Match match = Regex.Match(text, pattern);

                    if (match.Success)
                    {
                        string arg1 = match.Groups[1].Value;
                        string arg2 = match.Groups[2].Value;

                        Console.WriteLine("Topic recevied:" + arg1); // Output: "A"
                        Console.WriteLine("Message recevied:" + arg2); // Output: "message"

                        // Send message to all client subcribe topic A
                        var listClient = FindListSockets(arg1);
                        foreach (Socket client in listClient)
                        {
                            client.Send(Encoding.ASCII.GetBytes($"Message from topic {arg1}: {arg2}"));
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Client {clientSockets.IndexOf(current)}: Input string doesn't match pattern.");
                        current.Send(Encoding.ASCII.GetBytes("Input string doesn't match pattern."));
                    }
                } else if (text.Equals("/list"))
                {
                    var subcribedTopic = clientTopics.SingleOrDefault(e => e.Key.Equals(current)).Value;
                    current.Send(Encoding.ASCII.GetBytes("Subcribed topics: " + string.Join(", ", subcribedTopic)));
                }
                else
                {
                    current.Send(Encoding.ASCII.GetBytes("Invalid command"));
                }
            }
            else
            {
                byte[] data = Encoding.ASCII.GetBytes("Message from server: " + text);
                current.Send(data);
            }

            current.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
        }
        private static void AddIfNotExists<T>(List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
            }
        }

        private static List<Socket> FindListSockets(string topic)
        {
            List<Socket> list = clientTopics.Where(e => e.Value.Contains(topic)).Select(d => d.Key).ToList();
            return list;
        }
    }
}
