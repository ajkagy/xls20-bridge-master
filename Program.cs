using NetCoreServer;
using RippleDotNet;
using RippleDotNet.Model.Transaction;
using RippleDotNet.Requests.Transaction;
using RippleDotNet.Responses.Transaction.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace XLS_20_Bridge_MasterProcess
{
    class Program
    {
        private static System.Timers.Timer t;
        private static Settings config;
        private static database db;
        private static Ethereum eth;
        private static Xrpl xrpl;
        private static ValidatorMessage validatorMessage;
        private static Recovery recovery;
        private static dynamic validatorServer;
        private static dynamic apiServer;
        static async Task Main(string[] args)
        {
            bool runServer = true;
            config = new Settings();

            // WebSocket server port
            Console.WriteLine($"Validator WebSocket server port: {config._serverPort}");
            Console.WriteLine($"API server port: {config._httpsServerPort}");
            Console.WriteLine();

            if (config._sslEnabled)
            {
                // Create and prepare a new SSL server context
                var context = new SslContext(SslProtocols.Tls12, new X509Certificate2(config._sslCertPath, config._sslCertPassword == "" ? null : config._sslCertPassword));

                // Create a new WebSocket server
                validatorServer = new ValidatorSocketServerWss(context, IPAddress.Any, config._serverPort);

                //Create http/https server for front end
                apiServer = new ApiServerHttps(context, IPAddress.Any, config._httpsServerPort);
            }
            else
            {
                // Create a new WebSocket server
                validatorServer = new ValidatorSocketServer(IPAddress.Any, config._serverPort);

                //Create http/https server for front end
                apiServer = new ApiServer(IPAddress.Any, config._httpsServerPort);
            }

            // Start the server
            Console.Write("Servers starting...");
            validatorServer.Start();
            apiServer.Start();
            Console.WriteLine("Done!");
            db = new database();
            eth = new Ethereum(config, db);

            xrpl = new Xrpl(db, config, validatorServer);
            validatorMessage = new ValidatorMessage(db, config, validatorServer);

            recovery = new Recovery(db, config, eth, xrpl);
            bool startValid = await recovery.CheckForRecoveryMode();

            if (!startValid)
            {
                Console.WriteLine($"Startup Invalid. Shutting Down. Pretty any key to exit.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Starting Master Process");
            t = new System.Timers.Timer();
            t.AutoReset = false;
            t.Elapsed += new System.Timers.ElapsedEventHandler(t_ElapsedAsync);
            t.Interval = (config._tickTime * 1000);
            t.Start();

            while (runServer)
            {
            }
        }

        static async void t_ElapsedAsync(object sender, System.Timers.ElapsedEventArgs e)
        {
            await eth.PullBridgeContractDataAsync(config._blockNumber);

            validatorMessage.PushNewNFTsToValidators();

            validatorMessage.CheckForSignedMessages();

            await xrpl.SendNFTTransactions();

            validatorMessage.CheckForSignedOfferMessages();

            await xrpl.SendNFTOfferTransactions();

            t.Interval = (config._tickTime * 1000);
            t.Start();
        }
    }
}
