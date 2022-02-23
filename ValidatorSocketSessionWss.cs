using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using NetCoreServer;

namespace XLS_20_Bridge_MasterProcess
{
    public class ValidatorSocketSessionWss : WssSession
    {
        private string ApiKey;
        private database db;
        private Settings config;
        public ValidatorSocketSessionWss(WssServer server, database _db, Settings _config) : base(server)
        {
            ApiKey = _config._validatorKey;
            db = _db;
            config = _config;
        }

        public override void OnWsConnected(HttpRequest request)
        {
            string tempApiKeyStorage = "";

            long totalHeaders = request.Headers;
            for (long j = 0; j < totalHeaders; j++)
            {
                if (request.Header((int)j).Item1 == "token")
                {
                    tempApiKeyStorage = request.Header((int)j).Item2;
                }
            }

            if (tempApiKeyStorage != ApiKey)
            {
                this.Disconnect();
            }
            else
            {
                Console.WriteLine("Validator Connected!");
            }
        }

        public override void OnWsDisconnected()
        {
            Console.WriteLine($"WebSocket session with Id {Id} disconnected!");
        }

        public override void OnWsReceived(byte[] buffer, long offset, long size)
        {
            try
            {
                string message = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
                var request = JsonSerializer.Deserialize<Payload>(message);
                if (request.type == "Request")
                {
                    if (request.command == "GetNewNFTs")
                    {
                        ResponsePayloadBridgeNFT responsePayload = new ResponsePayloadBridgeNFT();
                        List<BridgeNFT> list = db.GetPendingNFTs(request.validator);
                        responsePayload.bridgelist = list;
                        responsePayload.type = "Response";
                        responsePayload.command = "NewBridgeNFTs";
                        responsePayload.validator = request.validator;
                        var response = JsonSerializer.Serialize(responsePayload);
                        SendText(response);
                    }
                }
                if (request.type == "Response")
                {
                    if (request.command == "SignMessage")
                    {
                        var payloadSign = JsonSerializer.Deserialize<PayloadSign>(message);
                        db.UpdateSignMessage(payloadSign.originOwner, payloadSign.contractAddress, Convert.ToInt32(payloadSign.tokenId), payloadSign.signMessage, payloadSign.validator);

                        PayloadSign responsePayload = new PayloadSign();
                        responsePayload.type = "Response";
                        responsePayload.command = "SignMessageConfirmed";
                        responsePayload.validator = request.validator;
                        responsePayload.originOwner = payloadSign.originOwner;
                        responsePayload.contractAddress = payloadSign.contractAddress;
                        responsePayload.tokenId = payloadSign.tokenId;
                        var response = JsonSerializer.Serialize(responsePayload);
                        SendText(response);
                    }
                    if (request.command == "CombineMultiSig")
                    {
                        var responseCombined = JsonSerializer.Deserialize<ResponseCombined>(message);
                        db.UpdateSignMessageCombined(responseCombined.originOwner, responseCombined.contractAddress, Convert.ToInt32(responseCombined.tokenId), responseCombined.txn_blob, responseCombined.validator, config);
                    }
                    if (request.command == "CombineMultiSigOffer")
                    {
                        var responseCombined = JsonSerializer.Deserialize<ResponseCombined>(message);
                        db.UpdateSignOfferMessageCombined(responseCombined.originOwner, responseCombined.contractAddress, Convert.ToInt32(responseCombined.tokenId), responseCombined.txn_blob, responseCombined.validator, config);
                    }
                    if (request.command == "OfferSignMessage")
                    {
                        var payloadSign = JsonSerializer.Deserialize<ResponseOfferSign>(message);
                        db.UpdateOfferSignMessage(payloadSign.originOwner, payloadSign.contractAddress, Convert.ToInt32(payloadSign.tokenId), payloadSign.signMessage, payloadSign.validator, payloadSign.xrplTokenId);

                        PayloadSign responsePayload = new PayloadSign();
                        responsePayload.type = "Response";
                        responsePayload.command = "SignOfferMessageConfirmed";
                        responsePayload.validator = request.validator;
                        responsePayload.originOwner = payloadSign.originOwner;
                        responsePayload.contractAddress = payloadSign.contractAddress;
                        responsePayload.tokenId = payloadSign.tokenId;
                        var response = JsonSerializer.Serialize(responsePayload);
                        SendText(response);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"WebSocket session caught an error with code {error}");
        }
    }
}
