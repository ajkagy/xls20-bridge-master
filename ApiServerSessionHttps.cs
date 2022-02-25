using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text;
using NetCoreServer;
using System.Collections.Generic;
using System.Text.Json;
using System.Web;

namespace XLS_20_Bridge_MasterProcess
{
    class ApiServerSessionHttps : HttpsSession
    {
        private string ApiKey;
        private database db;
        private Settings config;
        public ApiServerSessionHttps(NetCoreServer.HttpsServer server, database _db, Settings _config) : base(server)
        {
            ApiKey = _config._apiKey;
            db = _db;
            config = _config;
        }

        protected override void OnReceivedRequest(HttpRequest request)
        {
            try
            {
                string tempApiKeyStorage = "";
                // Show HTTP request content
                Console.WriteLine(request);
                long totalHeaders = request.Headers;
                for (long j = 0; j < totalHeaders; j++)
                {
                    if (request.Header((int)j).Item1 == "x-api-key")
                    {
                        tempApiKeyStorage = request.Header((int)j).Item2;
                    }
                }

                if (tempApiKeyStorage != ApiKey)
                {
                    SendResponseAsync(Response.MakeErrorResponse(401, "Not Authorized"));
                }
                else
                {
                    if (request.Method == "GET")
                    {
                        string url = request.Url;
                        if (url.StartsWith("/api/xls20bridge/getBridgeInfoByXRPAddress/"))
                        {
                            try
                            {
                                string[] splitVal = url.Split("/api/xls20bridge/getBridgeInfoByXRPAddress/");
                                List<BridgeNFT> list = db.GetNFTsReadyToBeClaimed(splitVal[1]);
                                List<ResponseObjectNFT> returnObj = new List<ResponseObjectNFT>();
                                foreach (BridgeNFT b in list)
                                {
                                    ResponseObjectNFT r = new ResponseObjectNFT();
                                    r.xrplTokenId = b.xrplTokenId;
                                    r.tokenUri = b.tokenUri;
                                    returnObj.Add(r);
                                }

                                SendResponseAsync(Response.MakeGetResponse(JsonSerializer.Serialize(returnObj), "application/json; charset=UTF-8"));
                            }
                            catch (Exception)
                            {
                                SendResponseAsync(Response.MakeErrorResponse(500, "Server Error"));
                            }
                        }
                        else if (url == "/api/xls20bridge/bridgeStatus")
                        {
                            List<string> validators = db.GetValidatorsToPing(6);
                            string status = "";
                            if(validators.Count == 0)
                            {
                                status = "Online";
                                SendResponseAsync(Response.MakeGetResponse(JsonSerializer.Serialize(status), "application/json; charset=UTF-8"));
                            }
                            else
                            {
                                status = "Validation Offline";
                                SendResponseAsync(Response.MakeGetResponse(JsonSerializer.Serialize(status), "application/json; charset=UTF-8"));
                            }
                        }
                        else if (url.StartsWith("/api/xls20bridge/getStatus"))
                        {
                            try
                            {
                                string[] splitVal = url.Split("?");
                                string tokenId = HttpUtility.ParseQueryString(splitVal[1]).Get("tokenId");
                                string tokenAddress = HttpUtility.ParseQueryString(splitVal[1]).Get("tokenAddress");

                                ResponseObjectStatus r = new ResponseObjectStatus();
                                string status = db.GetStatus(Convert.ToInt32(tokenId), tokenAddress);
                                r.status = status;

                                SendResponseAsync(Response.MakeGetResponse(JsonSerializer.Serialize(r), "application/json; charset=UTF-8"));
                            }
                            catch (Exception)
                            {
                                SendResponseAsync(Response.MakeErrorResponse(500, "Server Error"));
                            }
                        }
                        else
                        {
                            SendResponseAsync(Response.MakeErrorResponse("Unsupported HTTP method: " + request.Method));
                        }
                    }
                    else
                        SendResponseAsync(Response.MakeErrorResponse("Unsupported HTTP method: " + request.Method));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        protected override void OnReceivedRequestError(HttpRequest request, string error)
        {
            Console.WriteLine($"Request error: {error}");
        }

        protected override void OnError(SocketError error)
        {
            Console.WriteLine($"HTTP session caught an error: {error}");
        }
    }
}
