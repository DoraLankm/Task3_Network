using ConsoleApp21.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp21
{
    public class MessageSource : IMessageSource
    {
        private readonly UdpClient UdpClient;
        public MessageSource(int port)
        {
            UdpClient = new UdpClient(port);
        }
        public MessageUDP ReceiveMessage(ref IPEndPoint iPEndPoint)
        {
            byte[] data = UdpClient.Receive(ref iPEndPoint);
            string json = Encoding.UTF8.GetString(data);
            return MessageUDP.FromJson(json);
        }

        public void SendMessage(MessageUDP message, IPEndPoint iPEndPoint)
        {
            string json = message.ToJson();
            byte[] data = Encoding.UTF8.GetBytes(json);
            UdpClient.Send(data, data.Length, iPEndPoint);
        }
    }
}
