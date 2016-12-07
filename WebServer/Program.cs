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
        private static string html = "<head></head><body><html><h1>Welcome to the CS480\\Demo Server</h1><p>Why not visit: <ul><li><a href =\"http://www2.semo.edu/csdept/ \">Computer Science Home Page</a><li><a href =\"http://cstl-csm.semo.edu/liu/cs480_fall2012/index.htm\"\\>CS480 Home Page<a></ul></html></body>\n";
        private static SocketFlags flags;
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
                    string message = Encoding.ASCII.GetString(data, 0, dataLength);
                    AddMessage(message);

                    if (message.Contains("\r\n\r\n")) ;
                    AddMessage("Header Fully Recieved");
                    break;

                    //string message = "Test\n";
                    //data = Encoding.ASCII.GetBytes(message);
                    //client.Send(data, SocketFlags.None);
                }
                SendHead(client, 200, html.Length);
                AddMessage("Sending: " + html);
                byte[] htmlData = Encoding.ASCII.GetBytes(html + "\r\n\r\n");
                client.Send(htmlData, SocketFlags.None);

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

        private static void SendHead(Socket conn, int stat, int len)
        {
            String statStr = "";
            switch (stat)
            {
                case 200:
                    statStr = "OK";
                    break;
                case 400:
                    statStr = "Bad Request";
                    break;
                case 404:
                    statStr = "Not Found";
                    break;
                default:
                    statStr = "Unknown";
                    break;
            }

            string message = "HTTP/1.1 " + stat + " " + statStr + "\r\n";
            byte[] data = Encoding.ASCII.GetBytes(message);
            AddMessage(message);
            conn.Send(data, SocketFlags.None);

            message = "Server: CS480 Demo Server\r\n";
            data = Encoding.ASCII.GetBytes(message);
            AddMessage(message);
            conn.Send(data, SocketFlags.None);

            message = "Content-Length: " + len.ToString() + "\r\n";
            data = Encoding.ASCII.GetBytes(message);
            AddMessage(message);
            conn.Send(data, SocketFlags.None);

            message = "Content-Type: text/html\r\n";
            data = Encoding.ASCII.GetBytes(message);
            AddMessage(message);
            conn.Send(data, SocketFlags.None);

            message = "\r\n";
            data = Encoding.ASCII.GetBytes(message);
            AddMessage(message);
            conn.Send(data, SocketFlags.None);
        }
    }
}
