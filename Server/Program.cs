using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class Program
    {
        // TopicList will have a key value as its identifier 
        // and a list of sockets acting as each client which subsribe to recieve messages
        private static Dictionary<string, List<Socket>> TopicList = new()
        {
            ["Science"] = new List<Socket>(),
            ["Math"] = new List<Socket>(),
            ["English"] = new List<Socket>(),
            ["Geography"] = new List<Socket>(),
        };

        static void Main(string[] args)
        {
            int port = 15000;
            string ipAddress = "127.0.0.1";

            // We use tcp here to avoid losing data
            // Create a socket to start listening 
            Socket ServerListener = new Socket(AddressFamily
                .InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            ServerListener.Bind(endPoint);
            ServerListener.Listen(100);

            // Begin working with clients
            Console.WriteLine("Server started...");
            Console.WriteLine("List of participants: ");
            while (true)
            {
                // Create a socket for each client
                Socket clientSocket = ServerListener.Accept();
                Console.WriteLine("Client " + clientSocket.RemoteEndPoint + " is connected");

                // Send a list of topics to client
                string topic_List = ("List of Topics: @ - " + string.Join(" @ - ", TopicList.Keys)).Replace("@", Environment.NewLine);
                byte[] tmp = Encoding.ASCII.GetBytes(topic_List);
                clientSocket.Send(tmp);

                // Handle client's query here
                Thread userThread = new Thread(new ThreadStart(() => HandleClient(clientSocket)));
                userThread.Start();
            }
        }

        private static void HandleClient(Socket client)
        {
            while (true)
            {
                // Limit the size of the message to 1024 bytes before converting it to a string
                byte[] msg = new byte[1024];

                // Prevent server from bugging out in case a client is forcefully closed
                // Remove client if they disconnect first
                int size = 0;
                try
                {
                    size = client.Receive(msg);
                }
                catch (SocketException)
                {
                    closeClient(client);
                    return;
                }
                string[] receiveMessage = Encoding.ASCII.GetString(msg, 0, size).Split(' ');

                // Turn the message into an array of string with the length of 2
                string tmp = string.Join(" ", receiveMessage, 1, receiveMessage.Length - 1);
                string[] topic_message = { receiveMessage[0], tmp };
                //foreach (var x in topic_message) Console.WriteLine(x + "1");

                // Begin operation
                // Check if the user want to subscribe
                if (topic_message[0].ToLower() == "exit")
                {
                    closeClient(client);
                    return;
                }
                else if (topic_message[0].ToLower() == "sub")
                {
                    string returnMessage = string.Empty;
                    if (TopicList.ContainsKey(topic_message[1]))
                    {
                        if (TopicList[topic_message[1]].Contains(client))
                            returnMessage = "You are already subscribed to this topic.";
                        else
                        {
                            TopicList[topic_message[1]].Add(client);
                            returnMessage = "You are now subscribed to " + topic_message[1] + " topic.";
                        }
                        //Console.WriteLine("YES");
                    }
                    else
                    {
                        returnMessage = "The entered topic doesn't exist.";
                        //Console.WriteLine("NO");
                    }
                    byte[] returnMessageByte = Encoding.ASCII.GetBytes(returnMessage);
                    client.Send(returnMessageByte);
                }
                // Check the input string is a message to a topic
                else if (TopicList.ContainsKey(topic_message[0]))
                {
                    // If the user is not subscribed
                    if (!TopicList[topic_message[0]].Contains(client))
                        client.Send(Encoding.ASCII.GetBytes("User is not subscribed to the topic yet!"));
                    else
                    {
                        if (TopicList[topic_message[0]].Count != 0)
                        {
                            // Sending the message back to the client
                            Console.WriteLine("Sending to all subscribers in topic: " + topic_message[0]);
                            foreach (var x in TopicList[topic_message[0]])
                            {
                                byte[] toUser = Encoding.ASCII.GetBytes("From server: " + topic_message[1]);
                                x.Send(toUser);
                            }
                        }
                        else
                            Console.WriteLine("This topic doesn't have any subscriber to send to.");
                    }
                }
                else
                {
                    client.Send(Encoding.ASCII.GetBytes("The input topic doesn't exist / the formatted input is wrong."));
                }
            }
        }
        private static void closeClient(Socket client)
        {
            Console.WriteLine("Client " + client.RemoteEndPoint + " has diconnected");
            client.Shutdown(SocketShutdown.Both);
            client.Close();
            for (int i = 0; i < TopicList.Count; i++)
            {
                if (TopicList[TopicList.ElementAt(i).Key].Contains(client))
                    TopicList[TopicList.ElementAt(i).Key].Remove(client);
            }
        }
    }
}

