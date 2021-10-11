using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpListener listener = 
                new TcpListener(IPAddress.Parse("192.168.1.11"), 6666);
            listener.Start();

            Thread thread = new Thread(Listen);
            thread.Start(listener);

            Console.ReadLine();
        }

        static List<TcpClient> clients = new List<TcpClient>();

        static void Listen(object p)
        {
            TcpListener listener = (TcpListener)p;
            while (true)
            {
                var client = listener.AcceptTcpClient();
                clients.Add(client);
                Thread thread = new Thread(ProxyMessages);
                thread.Start(client);
            }
        }

        static void ProxyMessages(object p)
        {
            TcpClient client = (TcpClient)p;
            string message = null;
            var stream = client.GetStream();
            var br = new BinaryReader(stream);
            var bw = new BinaryWriter(stream);
            while (true)
            {
                try
                {
                    message = br.ReadString();
                    if (message == "exit")
                    {
                        clients.Remove(client);
                        bw.Write("exit");
                        break;
                    }
                    BroadcastMessage(message, client);
                }
                catch (Exception e) 
                {
                    Console.WriteLine(e.Message);
                }
            }
            br.Close();
            bw.Close();
        }

        static void BroadcastMessage(string message, TcpClient from)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i] == from)
                    continue;
                try
                {
                    var bw = new BinaryWriter(clients[i].GetStream());
                    bw.Write(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    clients.Remove(clients[i]);
                    break;
                }
            }
        }
    }
}
