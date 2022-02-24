using Ripple.Core.Types;
using RippleDotNet;
using RippleDotNet.Model.Account;
using RippleDotNet.Model.Transaction;
using RippleDotNet.Model;
using RippleDotNet.Requests.Account;
using RippleDotNet.Requests.Transaction;
using RippleDotNet.Responses.Transaction.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TransactionType = RippleDotNet.Model.TransactionType;
using RippleDotNet.Model.Transaction.TransactionTypes;
using Newtonsoft.Json;

namespace XLS_20_Bridge_MasterProcess
{
    public class Xrpl
    {
        private static database db { get; set; }
        private static Settings config { get; set; }
        private static dynamic validatorServer { get; set; }
        public Xrpl(database _db, Settings _config, dynamic _validatorServer)
        {
            config = _config;
            db = _db;
            validatorServer = _validatorServer;
        }

        public async Task SendNFTOfferTransactions()
        {
            List<BridgeNFT> list = db.GetRecordsByStatus("Validator Agreement Offer");
            IRippleClient client = new RippleClient(config._xrplRPC);
            foreach (BridgeNFT nft in list)
            {
                try
                {
                    client.Connect();
                    SubmitBlobRequest request = new SubmitBlobRequest();
                    request.TransactionBlob = nft.validatorMeta[0].mintOfferSignedCombined;

                    Submit result = new Submit(); ;
                    try
                    {
                        result = await client.SubmitTransactionBlob(request);
                    }
                    catch (Exception ex)
                    {
                        db.UpdateTransactionHashOffer("Error", nft.contractAddress, nft.originOwner, nft.tokenId, 0, ex.Message);
                        continue;
                    }

                    //Sleep for ledger Close
                    Thread.Sleep(10000);

                    if (result.EngineResult == "tesSUCCESS" || result.EngineResult == "terQUEUED")
                    {
                        bool isValid = await isValidTxnBool(client, result.Transaction.Hash);
                        if (isValid)
                        {
                            db.UpdateTransactionHashOffer("OfferCompleted", nft.contractAddress, nft.originOwner, nft.tokenId, result);
                        }
                        else
                        {
                            db.UpdateTransactionHashOffer("Error", nft.contractAddress, nft.originOwner, nft.tokenId, result);
                        }
                    }
                    else
                    {
                        db.UpdateTransactionHashOffer("Error", nft.contractAddress, nft.originOwner, nft.tokenId, result);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    try
                    {
                        client.Disconnect();
                    }
                    catch (Exception) { }
                }
            }
        }

        public async Task SendNFTTransactions()
        {
            List<BridgeNFT> list = db.GetRecordsByStatus("Validator Agreement");
            foreach (BridgeNFT nft in list)
            {
                IRippleClient client = new RippleClient(config._xrplRPC);
                try
                {
                    client.Connect();
                    SubmitBlobRequest request = new SubmitBlobRequest();
                    request.TransactionBlob = nft.validatorMeta[0].mintSignCombined;

                    Submit result = new Submit();
                    try
                    {
                        result = await client.SubmitTransactionBlob(request);
                    }
                    catch (Exception ex)
                    {
                        db.UpdateTransactionHash("NFT Mint Failure", nft.contractAddress, nft.originOwner, nft.tokenId, 0, ex.Message);
                        continue;
                    }

                    //Sleep for ledger Close
                    Thread.Sleep(10000);

                    if (result.EngineResult == "tesSUCCESS" || result.EngineResult == "terQUEUED")
                    {
                        Tuple<bool, string> t = await isValidTxn(client, result.Transaction.Hash);
                        if (t.Item1)
                        {
                            db.UpdateTransactionHash("OfferCreate", nft.contractAddress, nft.originOwner, nft.tokenId, 1, result);
                            CreateOffer payload = new CreateOffer();
                            payload.type = "Request";
                            payload.command = "CreateOffer";
                            payload.validator = "0";
                            payload.contractAddress = nft.contractAddress;
                            payload.originOwner = nft.originOwner;
                            payload.tokenId = nft.tokenId;
                            payload.xrplAddress = nft.xrplAddress;
                            payload.txnHash = result.Transaction.Hash;
                            payload.tokenUri = nft.tokenUri;
                            var createOfferRequest = System.Text.Json.JsonSerializer.Serialize(payload);
                            validatorServer.MulticastText(createOfferRequest);
                        }
                        else
                        {
                            db.UpdateTransactionHash("Error", nft.contractAddress, nft.originOwner, nft.tokenId, 0, result);
                        }
                    }
                    else
                    {
                        db.UpdateTransactionHash("Error", nft.contractAddress, nft.originOwner, nft.tokenId, 0, result);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    try
                    {
                        client.Disconnect();
                    }
                    catch (Exception) { }
                }
            }
        }

        public async Task AddXRPLTransactions()
        {
            try
            {
                object marker = null;
                int count = 0;
                IRippleClient client = new RippleClient(config._xrplRPC);
                try
                {
                    client.Connect();
                    do
                    {

                        TransactionReturnObj returnObj = await ReturnTransactions(client, marker, config._xrplIssuer);
                        marker = returnObj.marker;
                        if (returnObj.transactions != null)
                        {
                            count = count + returnObj.transactions.Transactions.Count;
                        }

                        foreach (TransactionSummary tx in returnObj.transactions.Transactions)
                        {
                            if (tx.Validated && (tx.Transaction.TransactionType == TransactionType.NFTokenMint || tx.Transaction.TransactionType == TransactionType.NFTokenCreateOffer) && tx.Transaction.Memos != null)
                            {
                                string address = tx.Transaction.Account;
                                if (tx.Transaction.Memos.Count > 0)
                                {
                                    foreach (Memo txnMemo in tx.Transaction.Memos)
                                    {
                                        try
                                        {
                                            if (txnMemo.Memo2.MemoDataAsText != "")
                                            {
                                                MemoFragment memo = JsonConvert.DeserializeObject<MemoFragment>(txnMemo.Memo2.MemoDataAsText.Replace("&quot;", "\""));
                                                switch (tx.Transaction.TransactionType)
                                                {
                                                    case TransactionType.NFTokenMint:
                                                        db.UpdateMinted(memo, tx.Transaction.Hash);
                                                        break;
                                                    case TransactionType.NFTokenCreateOffer:
                                                        db.UpdateOffered(memo, tx.Transaction.Hash);
                                                        break;
                                                    default:
                                                        break;
                                                }
                                            }
                                        }
                                        catch (Exception) { }
                                    }
                                }
                            }
                        }

                        //Throttle
                        Thread.Sleep(5 * 1000);
                    } while (marker != null);

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    try
                    {
                        client.Disconnect();
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private async Task<Tuple<bool, string>> isValidTxn(IRippleClient client, string txnHash)
        {
            try
            {
                ITransactionResponseCommon response = await client.Transaction(txnHash);
                if (response.Validated != null)
                {
                    if (response.Validated.Value)
                    {
                        return Tuple.Create(true, response.Memos[0].Memo2.MemoDataAsText);
                    }
                    else
                        return Tuple.Create(false, "");
                }
                else
                    return Tuple.Create(false, "");
            }
            catch (Exception)
            {
                return Tuple.Create(false, "");
            }
        }

        private async Task<bool> isValidTxnBool(IRippleClient client, string txnHash)
        {
            try
            {
                ITransactionResponseCommon response = await client.Transaction(txnHash);
                if (response.Validated != null)
                {
                    if (response.Validated.Value)
                    {
                        return true;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<TransactionReturnObj> ReturnTransactions(IRippleClient client, object marker, string xrplAddress)
        {
            TransactionReturnObj returnObj = new TransactionReturnObj();
            AccountTransactionsRequest req = new AccountTransactionsRequest(xrplAddress);
            if (marker != null)
            {
                req.Marker = marker;
            }
            AccountTransactions transactions = await client.AccountTransactions(req);

            returnObj.transactions = transactions;
            returnObj.marker = transactions.Marker;

            return returnObj;
        }
    }

    public struct TransactionReturnObj
    {
        public AccountTransactions transactions { get; set; }
        public object marker { get; set; }
    }
    public struct MemoFragment
    {
        public string contractAddress { get; set; }
        public string originOwner { get; set; }
        public string tokenId { get; set; }
    }
}
