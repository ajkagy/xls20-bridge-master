using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace XLS_20_Bridge_MasterProcess
{
    public class ApiServer : NetCoreServer.HttpServer
    {
        public ApiServer(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession()
        {
            Settings config = new Settings();
            database db = new database();
            return new ApiServerSession(this, db, config);
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTP session caught an error: {error}");
        }
    }

}
