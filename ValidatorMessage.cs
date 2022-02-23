using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace XLS_20_Bridge_MasterProcess
{
    public class ValidatorMessage
    {
        private static database db { get; set; }
        private static Settings config { get; set; }
        private static dynamic validatorServer { get; set; }
        public ValidatorMessage(database _db, Settings _config, dynamic _validatorServer)
        {
            config = _config;
            db = _db;
            validatorServer = _validatorServer;
        }

        public void PushNewNFTsToValidators()
        {
            List<BridgeNFT> list = db.GetPendingNFTs();
            foreach (BridgeNFT nft in list)
            {
                PayloadSign payload = new PayloadSign();
                payload.type = "Request";
                payload.command = "NewBridgeNFTs";
                payload.validator = "0";
                payload.contractAddress = nft.contractAddress;
                payload.originOwner = nft.originOwner;
                payload.tokenId = nft.tokenId;
                payload.xrplAddress = nft.xrplAddress;
                var request = JsonSerializer.Serialize(payload);
                validatorServer.MulticastText(request);
            }
        }

        public void CheckForSignedOfferMessages()
        {
            List<BridgeNFT> list = db.GetNFTsReadyToBeExecutedOffer();
            foreach (BridgeNFT nft in list)
            {
                Combine payload = new Combine();
                payload.type = "Request";
                payload.command = "CombineMultiSigOffer";
                payload.validator = "0";
                payload.contractAddress = nft.contractAddress;
                payload.originOwner = nft.originOwner;
                payload.tokenId = nft.tokenId;
                for (int i = 0; i < config._numberOfValidators; i++)
                {
                    payload.txn_blob.Add(nft.validatorMeta[i].mintOfferSigned);
                }
                var request = JsonSerializer.Serialize(payload);
                validatorServer.MulticastText(request);
            }
        }

        public void CheckForSignedMessages()
        {
            List<BridgeNFT> list = db.GetNFTsReadyToBeExecuted();
            foreach (BridgeNFT nft in list)
            {
                Combine payload = new Combine();
                payload.type = "Request";
                payload.command = "CombineMultiSig";
                payload.validator = "0";
                payload.contractAddress = nft.contractAddress;
                payload.originOwner = nft.originOwner;
                payload.tokenId = nft.tokenId;
                for (int i = 0; i < config._numberOfValidators; i++)
                {
                    payload.txn_blob.Add(nft.validatorMeta[i].mintSign);
                }
                var request = JsonSerializer.Serialize(payload);
                validatorServer.MulticastText(request);
            }
        }
    }
}
