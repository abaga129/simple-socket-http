using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace WebClient
{
    class Program
    {
        private static Socket Server;
        private static IPHostEntry hostAddr;

        public static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine("usage: WebClient <compname> <path> [appnum]");
                System.Environment.Exit(1);
            }

            try
            {
                hostAddr = Dns.GetHostEntry(args[0]);
                String GETrequest = "GET / HTTP/1.1\r\n" +
                          "Host: " + hostAddr.AddressList[0].ToString() + "\r\n" +
                          "Content-Length: 0\r\n" +
                          "\r\n";
                Console.WriteLine(GETrequest);

                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Server.Connect(hostAddr.AddressList[0].ToString(), int.Parse(args[2]));
                Server.Send(Encoding.ASCII.GetBytes(GETrequest));

                bool flag = true;
                string headerString = "";
                int contentLength = 0;
                byte[] bodyBuff = new byte[0];
                while (flag)
                {
                    // read the header byte by byte, until \r\n\r\n
                    byte[] buffer = new byte[1];
                    Server.Receive(buffer, 0, 1, 0);
                    headerString += Encoding.ASCII.GetString(buffer);
                    if (headerString.Contains("\r\n\r\n"))
                    {
                        Regex reg = new Regex("\\\r\nContent-Length: (.*?)\\\r\n");
                        Match m = reg.Match(headerString);
                        contentLength = int.Parse(m.Groups[1].ToString());
                        flag = false;
                        bodyBuff = new byte[contentLength];
                        Server.Receive(bodyBuff, 0, contentLength, 0);
                    }
                }
                Console.WriteLine("Server Response :");
                string body = Encoding.ASCII.GetString(bodyBuff);
                Console.WriteLine(body);
                Server.Close();

            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

        }
    }
}
