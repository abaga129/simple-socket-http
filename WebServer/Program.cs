using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WebServer
{
    class Program
    {
        private static Socket Server;

        static void Main(string[] args)
        {
            AddMessage("Starting Server...");
            StartServer();

            while (true)
            {
                Socket client = ConnectToClient();

                while (true)
                {
                    byte[] data = new byte[1024];
                    int dataLength = client.Receive(data);

                    if (dataLength == 0) break;

                    AddMessage(Encoding.ASCII.GetString(data, 0, dataLength));

                    string message = "Test\n";
                    data = Encoding.ASCII.GetBytes(message);
                    client.Send(data, SocketFlags.None);
                }

                DisconnectFromClient(client);

                if (RequestShutdown()) break;
            }

            Server.Close();
        }

        private static void StartServer()
        {
            Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Server.Bind(new IPEndPoint(IPAddress.Any, 8080));
            Server.Listen(10);
        }

        private static Socket ConnectToClient()
        {
            Console.WriteLine("Waiting for a client...");
            Socket client = Server.Accept();
            AddMessage(String.Format("Connected with {0} at port {1}", GetClientAddress(client), GetClientPort(client)));
            SendWelcomeMessageToClient(client);
            return client;
        }

        private static IPAddress GetClientAddress(Socket client)
        {
            return ((IPEndPoint)client.RemoteEndPoint).Address;
        }

        private static int GetClientPort(Socket client)
        {
            return ((IPEndPoint)client.RemoteEndPoint).Port;
        }

        private static void DisconnectFromClient(Socket client)
        {
            AddMessage(String.Format("Disconnecting from {0}", GetClientAddress(client)));
            client.Close();
        }

        private static void SendWelcomeMessageToClient(Socket client)
        {
            byte[] data;
            string welcomeMessage = "You have successfully connected to the server!";
            data = Encoding.ASCII.GetBytes(welcomeMessage);
            client.Send(data, data.Length, SocketFlags.None);
        }

        private static void AddMessage(string message)
        {
            Console.WriteLine(message);
        }

        private static bool RequestShutdown()
        {
            Console.WriteLine("Would you like to shut down the server? (Yes or No)");
            bool quitting = Console.ReadLine().ToLower().Contains("y");
            if (quitting) Console.WriteLine("The server is shutting down...");
            return quitting;
        }
    }
}
