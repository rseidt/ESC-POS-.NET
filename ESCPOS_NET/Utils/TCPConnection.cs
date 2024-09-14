using ESCPOS_NET.Utils;
using SuperSimpleTcp;
using System;
using System.IO;

namespace ESCPOS_NET
{
    public class TCPConnection
    {
        public Stream ReadStream { get; private set; } = new EchoStream();
        public Stream WriteStream { get; private set; }
        public event EventHandler<EventArgs> Connected;
        public event EventHandler<EventArgs> Disconnected;
        public bool IsConnected => _client?.IsConnected ?? false;
        private SimpleTcpClient _client;
        //public event EventHandler<DataReceivedEventArgs> DataReceived;
        public TCPConnection(string destination)
        {
            _client = new SimpleTcpClient(destination);
            _client.Events.Connected += ConnectedEventHandler;
            _client.Events.Disconnected += DisconnectedEventHandler;
            _client.Events.DataReceived += DataReceivedEventHandler;
            _client.Keepalive = new SimpleTcpKeepaliveSettings() { EnableTcpKeepAlives = true, TcpKeepAliveInterval = 1, TcpKeepAliveTime = 1, TcpKeepAliveRetryCount = 3 };
            ReadStream.ReadTimeout = 1500;
            WriteStream = new InterceptableWriteMemoryStream(bytes => _client.Send(bytes));
        }
        private void ConnectedEventHandler(object sender, EventArgs e)
        {
            Connected?.Invoke(sender, e);
        }
        private void DisconnectedEventHandler(object sender, EventArgs e)
        {
            Disconnected?.Invoke(sender, e);
        }
        private void DataReceivedEventHandler(object sender, DataReceivedEventArgs e)
        {
            ReadStream.Write(e.Data.ToArray(), 0, e.Data.Count);
        }
        public void ConnectWithRetries(int timeoutMs)
        {
            _client.ConnectWithRetries(timeoutMs);
        }

        ~TCPConnection()
        {
            try
            {
                _client.Events.DataReceived -= DataReceivedEventHandler;
                _client.Events.Connected -= ConnectedEventHandler;
                _client.Events.Disconnected -= DisconnectedEventHandler;
                _client?.Dispose();
            }
            catch { }
        }

    }
}
