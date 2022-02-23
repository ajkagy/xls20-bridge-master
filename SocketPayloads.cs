using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLS_20_Bridge_MasterProcess
{
    public class Payload
    {
        public string type { get; set; }
        public string command { get; set; }
        public string validator { get; set; }
    }

    public class PayloadSign : Payload
    {
        public string originOwner { get; set; }
        public string contractAddress { get; set; }
        public int tokenId { get; set; }
        public string xrplAddress { get; set; }
        public string signMessage { get; set; }
    }

    public class CreateOffer : Payload
    {
        public string originOwner { get; set; }
        public string contractAddress { get; set; }
        public int tokenId { get; set; }
        public string xrplAddress { get; set; }
        public string signMessage { get; set; }
        public string txnHash { get; set; }
        public string tokenUri { get; set; }
    }

    public class ResponsePayloadBridgeNFT : Payload
    {
        public List<BridgeNFT> bridgelist { get; set; }
    }

    public class Combine : Payload
    {
        public Combine()
        {
            txn_blob = new List<string>();
        }
        public string originOwner { get; set; }
        public string contractAddress { get; set; }
        public int tokenId { get; set; }
        public List<string> txn_blob { get; set; }
    }

    public class ResponseCombined : Payload
    {
        public string originOwner { get; set; }
        public string contractAddress { get; set; }
        public int tokenId { get; set; }
        public string txn_blob { get; set; }
    }

    public class ResponseOfferSign : Payload
    {
        public string originOwner { get; set; }
        public string contractAddress { get; set; }
        public int tokenId { get; set; }
        public string xrplAddress { get; set; }
        public string signMessage { get; set; }
        public string xrplTokenId { get; set; }
    }

    public class ResponseObjectNFT
    {
        public string xrplTokenId { get; set; }
        public string tokenUri { get; set; }
    }

    public class ResponseObjectStatus
    {
        public string status { get; set; }
    }
}
