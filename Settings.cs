using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLS_20_Bridge_MasterProcess
{
    public class Settings
    {
        private string settingsFilePath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config/settings.json");
        public int _tickTime { get; set; }
        public int _blockNumber { get; set; }
        public int _serverPort { get; set; }
        public int _httpsServerPort { get; set; }
        public string _bridgeContract { get; set; }
        public string _ethRPC { get; set; }
        public string _validatorKey { get; set; }
        public string _apiKey { get; set; }
        public string _ethPrivateKey { get; set; }
        public string _chain { get; set; }
        public string _xrplRPC { get; set; }
        public int _numberOfValidators { get; set; }
        public bool _sslEnabled { get; set; }
        public string _sslCertPath { get; set; }
        public string _sslCertPassword { get; set; }
        public string _xrplIssuer { get; set; }

        public Settings()
        {
            string jsonConfig = File.ReadAllText(settingsFilePath);
            dynamic d = JObject.Parse(jsonConfig);
            _tickTime = d.Tick_Time;
            _blockNumber = d.Block_Number;
            _serverPort = d.WSS_Server_Port;
            _bridgeContract = d.Bridge_Contract;
            _ethRPC = d.Ethereum_RPC_Url;
            _validatorKey = d.Validator_Key;
            _apiKey = d.API_Key;
            _ethPrivateKey = d.Eth_private_key;
            _chain = d.Ethereum_Chain;
            _xrplRPC = d.XRPL_RPC;
            _numberOfValidators = d.Number_Validators;
            _httpsServerPort = d.Https_Server_Port;
            _sslEnabled = d.SSL_Enabled;
            _sslCertPath = d.SSL_Cert_Path;
            _sslCertPassword = d.SSL_Cert_Password;
            _xrplIssuer = d.XRPL_Issuer;
        }

        public Settings SaveBlockNumber(int blockNumber)
        {
            var settingsObj = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(settingsFilePath));
            settingsObj["Block_Number"] = blockNumber;
            var json = JsonConvert.SerializeObject(settingsObj, Formatting.Indented);
            File.WriteAllText(settingsFilePath, json);
            return new Settings();
        }
    }
}
