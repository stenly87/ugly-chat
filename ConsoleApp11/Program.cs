using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace ConsoleApp11
{
    class Program
    {
        static void Main(string[] args)
        {
            TcpClient client = new TcpClient("192.168.1.11", 6666);
            Thread thread = new Thread(Listen);
            thread.Start(client);
            var stream = client.GetStream();
            var bw = new BinaryWriter(stream);
            while (true)
            {
                string message = Console.ReadLine();
                bw.Write(message);
                if (message == "exit")
                    break;
            }
        }

        static void Listen(object p)
        {
            TcpClient client = (TcpClient)p;
            var stream = client.GetStream();
            using (var br = new BinaryReader(stream))
            {
                while (true)
                {
                    string message = br.ReadString();
                    if (message == "exit")
                        break;
                    Console.WriteLine(message);
                }
            }
        }
    }
}
