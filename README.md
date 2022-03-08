# xls20-bridge-master
C#/.NET Core master process for the ERC721 to XLS20 Bridge

### Requirements

+ [NodeJs](https://nodejs.org/en/)
+ [Git](https://git-scm.com/downloads)
+ [.NET 5.0 Core Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/5.0)
+ [Infura Account](https://infura.io/)
+ [Visual Studio 2019 or greater](https://visualstudio.microsoft.com/downloads/) (Optional: Only for debugging)

## Getting Started

1. Open a command prompt or Powershell prompt and issue the following commands

```
git clone https://github.com/ajkagy/RippleDotNet
```

```
git clone https://github.com/ajkagy/xls20-bridge-master
```

2. Open `xls20-bridge-master//config/settings.json' and edit the settings variables
    - `WSS_Server_Port` The Port that will run the WS or WSS Server that validators will connect to
    - `Https_Server_Port` The Port that will run the http or https api that the bridge express.js proxy will connect to
    - `Bridge_Contract` The Rinkeby Bridge contract address.
    - `XRPL_Issuer` The multi-sig issuer wallet address on the xrpl
    - `Ethereum_RPC_Url` Infura account URL endpoint for Rinkeby
    - `Validator_Key` a random generated guid that validators will use to connect via WS or WSS
    - `API_Key` a random generated guid for http or https api access into the master process
    - `Eth_private_key` arbitrary eth private key for making view function calls
    - `SSL_Enabled` true or false. Used to turn on https and wss
    - `SSL_Cert_Path` Path to the SSL cert for https/wss (only used when `SSL_Enabled` is true)
    - `SSL_Cert_Password` cert password for https/wss (only used when `SSL_Enabled` is true)
3. Build The Project
```
dotnet build --configuration Release
```
4. Navigate to the Release Folder
```
cd xls20-bridge-master/bin/Release/net5.0
```
5. Run
```
dotnet XLS-20-Bridge-MasterProcess.dll
```
