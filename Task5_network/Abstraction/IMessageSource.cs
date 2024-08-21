using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp21.Abstraction
{
    public interface IMessageSource
    {
        void SendMessage(MessageUDP message, IPEndPoint iPEndPoint);

        MessageUDP ReceiveMessage(ref IPEndPoint iPEndPoint);
    }
}
