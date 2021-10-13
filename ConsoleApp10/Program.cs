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

        static List<ChatClient> clients = new List<ChatClient>();

        static void Listen(object p)
        {
            TcpListener listener = (TcpListener)p;
            while (true)
            {
                var client = new ChatClient (listener.AcceptTcpClient());
                clients.Add(client);
                Thread thread = new Thread(ProxyMessages);
                thread.Start(client);
            }
        }

        static void ProxyMessages(object p)
        {
            ChatClient client = (ChatClient)p;
            string message = null;
            int errorCount = 0;
            while (true)
            {
                try
                {
                    message = client.Reader.ReadString();
                    errorCount = 0;
                    if (message.StartsWith("nick$"))
                    {
                        client.Nick = message.Split('$')[1];
                        message = $"{client.Nick} ворвался на сервер";
                        BroadcastMessage(message, null);
                        continue;
                    }
                    // /p nick: test
                    else if (message.StartsWith("/p"))
                    {
                        int index = message.IndexOf(':');
                        string targetNick = new string(message.Skip(3).Take(index - 3).ToArray());
                        string text = new string(message.Skip(index + 1).ToArray());
                        SendPrivate(client, targetNick, text);
                        continue;
                    }
                    else if (message == "exit")
                    {
                        clients.Remove(client);
                        client.Sender.Write("exit");
                        break;
                    }
                    BroadcastMessage(message, client);
                }
                catch (Exception e) 
                {
                    errorCount++;
                    Console.WriteLine(e.Message);
                    if (errorCount > 10)
                        break;
                }
            }
            BroadcastMessage($"{client.Nick} покинул заведение", null);
            client.Close();
        }

        private static void SendPrivate(ChatClient client, string targetNick, string text)
        {
            var targetClient = clients.Find(s => s.Nick == targetNick);
            if (targetClient == null)
                return;
            targetClient.Sender.Write($"{client.Nick} шепчет: {text}");


        }

        static void BroadcastMessage(string message, ChatClient from)
        {
            if (from != null)
                message = $"{from.Nick}: {message}";
            else
                message = $"сервер: {message}";
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i] == from)
                    continue;
                try
                {
                    clients[i].Sender.Write(message);
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
