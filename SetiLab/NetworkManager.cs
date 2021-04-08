using System;
using System.Net.Sockets;

namespace SetiLab
{
    public class NetworkManager
    {
        protected NetworkStream _stream;

        public NetworkManager(NetworkStream stream) => _stream = stream;

        public virtual string GetMessage()
        {
            byte[] buffer = new byte[256];
            int num_bytes = _stream.Read(buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, num_bytes);
        }

        public virtual void SendMessage(string message)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);
        }
    }

    public class NetworkManagerServer : NetworkManager
    {
        private TcpClient _client;

        public NetworkManagerServer(TcpClient client) : base(client.GetStream()) => _client = client;

        public override string GetMessage()
        {
            byte[] buffer = new byte[256];
            int num_bytes = _stream.Read(buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, num_bytes);
        }

        public override void SendMessage(string message)
        {
            if (_client.Client.Poll(100, SelectMode.SelectWrite))
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                _stream.Write(buffer, 0, buffer.Length);
            }
        }
    }
}
