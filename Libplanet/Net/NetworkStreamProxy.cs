using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Libplanet.Net
{
    internal class NetworkStreamProxy : IDisposable
    {
        private const int _bufferSize = 8192;
        private readonly NetworkStream _source;
        private TcpClient _targetClient;

        public NetworkStreamProxy(NetworkStream source)
        {
            _source = source;
        }

        public async Task StartAsync(string hostname, int port)
        {
            _targetClient = new TcpClient(hostname, port);
            NetworkStream target = _targetClient.GetStream();
            await Task.WhenAll(
                Proxy(_source, target),
                Proxy(target, _source));
        }

        public void Dispose()
        {
            _targetClient?.Dispose();
        }

        private async Task Proxy(NetworkStream source, NetworkStream target)
        {
            while (true)
            {
                var buf = new byte[_bufferSize];
                int read = await source.ReadAsync(buf, 0, buf.Length);
                await target.WriteAsync(buf, 0, read);
            }
        }
    }
}
