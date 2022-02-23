using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace XLS_20_Bridge_MasterProcess
{
    class ValidatorSocketServer : WsServer
    {
        public ValidatorSocketServer(IPAddress address, int port) : base(address, port) { }

        protected override TcpSession CreateSession()
        {
            Settings config = new Settings();
            database db = new database();

            return new ValidatorSocketSession(this, db, config);

        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"WebSocket server caught an error with code {error}");
        }
    }
}
