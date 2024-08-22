using ConsoleApp21;
using ConsoleApp21.Abstraction;
using NUnit.Framework.Interfaces;
using System.Net;

namespace TestProject1
{
    public class Tests
    {
        IMessageSource _source;
        IPEndPoint endPoint;
        [SetUp]
        public void Setup()
        {
            endPoint = new IPEndPoint(IPAddress.Any,0);
        }

        [Test]
        public void TestReceiveMessage()
        {
            _source = new MockMessageSource();
            var result = _source.ReceiveMessage(ref endPoint);
            Assert.IsNotNull(result);
            Assert.IsNull(result.Text);
            Assert.IsNotNull(result.FromName);
            Assert.That(Command.Register, Is.EqualTo(result.Command));
        }

        [Test]
        //���� ��� �������� �������� ��������� �������� 
        public void TestClientSend()
        {
            MockMessageSource mok = new MockMessageSource(); //���
            Client client = new Client(mok, endPoint, "newClient");
            client.ClientSender("Hello","World");

            Assert.IsTrue(mok.messageList.Count > 0);
            MessageUDP get_message = mok.messageList.Last(); // ����� ��������� ��������� �� ������
            Assert.IsNotNull(get_message); //��������� �� ������
            Assert.IsNotNull(get_message.ToName); //��� ����������� �������
            Assert.AreEqual("newClient", get_message.FromName); //��� ����������� ���������
            Assert.AreEqual("Hello", get_message.Text); //��� ����������� ���������
            Assert.AreEqual("World", get_message.ToName); //��� ����������� ���������
            Assert.AreEqual(Command.Message, get_message.Command); //������� ���������
        }



        [Test]
        //���� ��� ��������� ���������
        public void ClientListenOnce()
        {
            MockMessageSource mok = new MockMessageSource(); //���
            Client client = new Client(mok, endPoint, "newClient");

            var message = new MessageUDP
            {
                Command = Command.Register,
                FromName = "Client",
                ToName = "Server",
                Text = "",
                Time = DateTime.Now
            };

            int count = mok.messages.Count; //����� ��������� �� ��������� � �������

            mok.messages.Enqueue(message); //��������� ��������� � ����� �������

            client.ClientListenerOnce(); //�������

            Assert.AreEqual(count, mok.messages.Count); //�������� �� ������� ���������
        }
    }
}