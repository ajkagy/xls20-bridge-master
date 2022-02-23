using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;

namespace XLS_20_Bridge_MasterProcess
{
    public class ValidatorSocketServerWss : WssServer
    {
        public ValidatorSocketServerWss(SslContext context, IPAddress address, int port) : base(context, address, port) { }

        protected override SslSession CreateSession()
        {
            Settings config = new Settings();
            database db = new database();

            return new ValidatorSocketSessionWss(this, db, config);

        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"WebSocket server caught an error with code {error}");
        }
    }
}
