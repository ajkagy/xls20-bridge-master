using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using System.IO;
using System.Numerics;
using Nethereum.Contracts;

namespace XLS_20_Bridge_MasterProcess
{
    public class Ethereum
    {
        const string ABIPath = "config/Bridge-Abi.txt";
        const string ABIERC721Base = "config/erc-721-base.txt";
        private static Settings config;
        private static database db;
        public Ethereum(Settings _config, database _database)
        {
            config = _config;
            db = _database;
        }

        public async Task PullBridgeContractDataAsync(int blockNumber, string status = "")
        {
            try
            {
                List<BridgeNFT> addList = new List<BridgeNFT>();
                string abi = File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, ABIPath));
                string abi_base = File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, ABIERC721Base));
                Account account;
                if (config._chain == "rinkeby")
                {
                    account = new Nethereum.Web3.Accounts.Account(config._ethPrivateKey, Nethereum.Signer.Chain.Rinkeby);
                }
                else
                {
                    account = new Nethereum.Web3.Accounts.Account(config._ethPrivateKey, Nethereum.Signer.Chain.MainNet);
                }
                var web3 = new Nethereum.Web3.Web3(account, config._ethRPC);

                var contract = web3.Eth.GetContract(abi, config._bridgeContract);

                var returnBridgeDataFunction = contract.GetFunction("returnBridgeNFTs");
                var returnAllBridgeNFTsOutputDTO = await returnBridgeDataFunction.CallAsync<ReturnAllBridgeNFTsOutputDTO>(config._blockNumber);

                foreach (NFTBridgeData nft in returnAllBridgeNFTsOutputDTO.ReturnValue1)
                {
                    BridgeNFT b = new BridgeNFT();
                    b.blockNumber = (int)nft.BlockNumber;
                    b.contractAddress = nft.ContractAddress;
                    b.originOwner = nft.OriginOwner;
                    b.tokenId = (int)nft.TokenId;
                    b.xrplAddress = nft.XrplAddress;

                    var contractERC721 = web3.Eth.GetContract(abi_base, nft.ContractAddress);
                    var returnTokenUriFunction = contractERC721.GetFunction("tokenURI");
                    var uri = await returnTokenUriFunction.CallAsync<string>(nft.TokenId);
                    b.tokenUri = uri;
                    addList.Add(b);
                }

                if (status == "")
                {
                    db.AddNFTs(addList, config);
                }
                else
                {
                    db.AddNFTs(addList, config, status);
                }

                if (returnAllBridgeNFTsOutputDTO.ReturnValue1.Count > 0)
                {
                    int highestBlockNumber = getHighestBlockNumber(returnAllBridgeNFTsOutputDTO.ReturnValue1);
                    config = config.SaveBlockNumber(highestBlockNumber);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        public int getHighestBlockNumber(List<NFTBridgeData> bridgeList)
        {
            int highestBlock = 0;
            foreach (NFTBridgeData b in bridgeList)
            {
                if (highestBlock < b.BlockNumber)
                {
                    highestBlock = (int)b.BlockNumber;
                }
            }
            return highestBlock;
        }

    }


    [FunctionOutput]
    public class ReturnAllBridgeNFTsOutputDTO : IFunctionOutputDTO
    {
        [Parameter("tuple[]", "", 1)]
        public virtual List<NFTBridgeData> ReturnValue1 { get; set; }
    }

    public class NFTBridgeData
    {
        [Parameter("address", "contractAddress", 1)]
        public virtual string ContractAddress { get; set; }
        [Parameter("address", "originOwner", 2)]
        public virtual string OriginOwner { get; set; }
        [Parameter("uint256", "tokenId", 3)]
        public virtual BigInteger TokenId { get; set; }
        [Parameter("string", "xrplAddress", 4)]
        public virtual string XrplAddress { get; set; }
        [Parameter("uint256", "blockNumber", 5)]
        public virtual BigInteger BlockNumber { get; set; }
    }

    [Function("returnBridgeNFTs", typeof(ReturnAllBridgeNFTsOutputDTO))]
    public class ReturnAllBridgeNFTsFunction : FunctionMessage
    {
        [Parameter("uint256", "_blockNumber", 1)]
        public virtual BigInteger BlockNumber { get; set; }
    }
}
