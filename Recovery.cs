using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLS_20_Bridge_MasterProcess
{
    public class Recovery
    {
        private static database db { get; set; }
        private static Settings config { get; set; }
        private static Ethereum eth { get; set; }
        private static Xrpl xrpl { get; set; }
        public Recovery(database _db, Settings _config, Ethereum _eth, Xrpl _xrpl)
        {
            config = _config;
            db = _db;
            eth = _eth;
            xrpl = _xrpl;
        }

        public async Task<bool> CheckForRecoveryMode()
        {
            try
            {
                if (!System.IO.File.Exists(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "db\\storage.db")))
                {
                    //Recovery Mode, Create database and pull in historical records
                    Console.WriteLine("Database not found...running in reocvery mode. Please wait...");
                    if (!Directory.Exists(Environment.CurrentDirectory + "\\db"))
                    {
                        System.IO.Directory.CreateDirectory(Environment.CurrentDirectory + "\\db");
                    }
                    System.Data.SQLite.SQLiteConnection.CreateFile(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "db\\storage.db"));
                    db.Startup();
                    //Pull Eth Data
                    Console.WriteLine("Checking Ethereum Bridge Transactions...");
                    await eth.PullBridgeContractDataAsync(0);
                    Console.WriteLine("Checking XRPL Issuer Transactions...");
                    //Pull XRPL Data
                    await xrpl.AddXRPLTransactions();
                    Console.WriteLine("Recovery mode complete.");
                }
            }
            catch (Exception ex)
            {
                Console.Write("Error: " + ex.Message);
                return false;
            }

            return true;
        }
    }
}
