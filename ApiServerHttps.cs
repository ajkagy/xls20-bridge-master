using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace XLS_20_Bridge_MasterProcess
{
    public class ApiServerHttps : NetCoreServer.HttpsServer
    {
        public ApiServerHttps(SslContext context, IPAddress address, int port) : base(context, address, port) { }

        protected override SslSession CreateSession()
        {
            Settings config = new Settings();
            database db = new database();
            return new ApiServerSessionHttps(this, db, config);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTP session caught an error: {error}");
        }
    }

}
