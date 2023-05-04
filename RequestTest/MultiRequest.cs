using System.Net.Sockets;
using System.Text;

namespace RequestTest
{
    public class MultiRequest
    {
        static List<Socket> sockets = new List<Socket>();
        static int attemps = 1;
        [Fact]
        public static void ASetupServer()
        {
            MultiServer.Program.SetupServer();
            //Console.ReadLine(); // When we press enter close everything
            //MultiServer.Program.CloseAllSockets();
        }
        [Fact]
        public static void BConnectServer()
        {
            MultiClient.Program.ConnectToServer();
        }
        [Fact]
        public static async void CSendMultiRequest()
        {
            Socket ClientSocket1 = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MultiClient.Program.ClientSocket = ClientSocket1;
            MultiClient.Program.ConnectToServer();
            sockets.Add(ClientSocket1);

            Socket ClientSocket2 = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            MultiClient.Program.ClientSocket = ClientSocket2;
            MultiClient.Program.ConnectToServer();
            sockets.Add(ClientSocket2);

            //Socket ClientSocket3 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket3;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket3);

            //Socket ClientSocket4 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket4;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket4);

            //Socket ClientSocket5 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket5;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket5);

            //Socket ClientSocket6 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket6;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket6);

            //Socket ClientSocket7 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket7;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket7);

            //Socket ClientSocket8 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket8;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket8);

            //Socket ClientSocket9 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket9;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket9);

            //Socket ClientSocket10 = new Socket
            //(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //MultiClient.Program.ClientSocket = ClientSocket10;
            //MultiClient.Program.ConnectToServer();
            //sockets.Add(ClientSocket10);

            var task1 = Task.Run(() => SendMultiMessage(ClientSocket1));
            var task2 = Task.Run(() => SendMultiMessage(ClientSocket2));
            //var task3 = Task.Run(() => SendMultiMessage(ClientSocket3));
            //var task4 = Task.Run(() => SendMultiMessage(ClientSocket4));
            //var task5 = Task.Run(() => SendMultiMessage(ClientSocket5));
            //var task6 = Task.Run(() => SendMultiMessage(ClientSocket6));
            //var task7 = Task.Run(() => SendMultiMessage(ClientSocket7));
            //var task8 = Task.Run(() => SendMultiMessage(ClientSocket8));
            //var task9 = Task.Run(() => SendMultiMessage(ClientSocket9));
            //var task10 = Task.Run(() => SendMultiMessage(ClientSocket10));
            await task1;
            await task2;
            //await task3;
            //await task4;
            //await task5;
            //await task6;
            //await task7;
            //await task8;
            //await task9;
            //await task10;
        }

        static void SendMultiMessage(Socket ClientSocket)
        {
            while (attemps <= 100)
            {
                //Thread.Sleep(1);
                string request = $"Request message {attemps}\n";
                attemps++;
                SendString(ClientSocket, request);
            }
        }

        static void SendString(Socket client, string text)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(text);
            client.Send(buffer, 0, buffer.Length, SocketFlags.None);
        }
    }
}