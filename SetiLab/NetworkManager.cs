using System;
using System.Net.Sockets;

namespace SetiLab
{
    public class NetworkManager
    {
        private NetworkStream _stream;

        public NetworkManager(NetworkStream stream) => _stream = stream;

        public string GetMessage()
        {
            byte[] buffer = new byte[256];
            int num_bytes = _stream.Read(buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer, 0, num_bytes);
        }

        public void SendMessage(string message)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
            _stream.Write(buffer, 0, buffer.Length);
        }
    }
}
