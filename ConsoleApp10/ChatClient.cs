using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    public class ChatClient
    {
        public ChatClient(TcpClient tcpClient)
        {
            TcpClient = tcpClient;
            Sender = new BinaryWriter(tcpClient.GetStream());
            Reader = new BinaryReader(tcpClient.GetStream());
        }

        public string Nick { get; set; }
        public TcpClient TcpClient { get; private set; }
        public BinaryWriter Sender { get; private set; }
        public BinaryReader Reader { get; private set; }

        public void Close()
        {
            Sender.Close();
            Reader.Close();
        }
    }
}
