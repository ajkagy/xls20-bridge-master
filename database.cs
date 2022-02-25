using RippleDotNet.Model.Transaction;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XLS_20_Bridge_MasterProcess
{
    public class database
    {
        static string connectionstring = System.IO.Path.Combine("Data Source=" + System.AppDomain.CurrentDomain.BaseDirectory, "db\\storage.db");
        public database() { }

        public void Startup()
        {
            //Create Tables if they don't already exist
            string startupScript = File.ReadAllText(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "config/startup_script.txt"));
            var con = new System.Data.SQLite.SQLiteConnection(connectionstring);
            con.Open();
            using var cmd = new SQLiteCommand(startupScript, con);
            cmd.ExecuteNonQuery();
            con.Close();

        }

        public void AddNFTs(List<BridgeNFT> addList, Settings config)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (BridgeNFT a in addList)
                        {
                            var cmdInsert = new SQLiteCommand("Insert into BridgeNFT (contractAddress,originOwner,tokenId,xrplAddress,blockNumber,status,last_updated,tokenUri) select @contractAddress,@originOwner,@tokenId,@xrplAddress,@blockNumber,@status,@last_updated,@tokenUri WHERE (SELECT COUNT(*) FROM BridgeNFT WHERE contractAddress = @contractAddress and tokenId = @tokenId) = 0", conn);
                            cmdInsert.Parameters.Add(new SQLiteParameter("@contractAddress", a.contractAddress));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@originOwner", a.originOwner));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@tokenId", a.tokenId));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@xrplAddress", a.xrplAddress));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@blockNumber", a.blockNumber));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@status", "Pending"));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@last_updated", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@tokenUri", a.tokenUri));
                            cmdInsert.ExecuteNonQuery();
                            long lastID = conn.LastInsertRowId;

                            if (lastID != 0)
                            {
                                for (int i = 1; i <= config._numberOfValidators; i++)
                                {
                                    var cmdInsertValidatorMeta = new SQLiteCommand("Insert into ValidatorMetadata (bridgeNFTId,validatorNumber) select @bridgeNFTId,@validatorNumber", conn);
                                    cmdInsertValidatorMeta.Parameters.Add(new SQLiteParameter("@bridgeNFTId", lastID));
                                    cmdInsertValidatorMeta.Parameters.Add(new SQLiteParameter("@validatorNumber", i));
                                    cmdInsertValidatorMeta.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
        }
        public void AddNFTs(List<BridgeNFT> addList, Settings config, string status)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        foreach (BridgeNFT a in addList)
                        {
                            var cmdInsert = new SQLiteCommand("Insert into BridgeNFT (contractAddress,originOwner,tokenId,xrplAddress,blockNumber,status,last_updated,tokenUri) select @contractAddress,@originOwner,@tokenId,@xrplAddress,@blockNumber,@status,@last_updated,@tokenUri WHERE (SELECT COUNT(*) FROM BridgeNFT WHERE contractAddress = @contractAddress and tokenId = @tokenId) = 0", conn);
                            cmdInsert.Parameters.Add(new SQLiteParameter("@contractAddress", a.contractAddress));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@originOwner", a.originOwner));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@tokenId", a.tokenId));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@xrplAddress", a.xrplAddress));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@blockNumber", a.blockNumber));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@status", status));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@last_updated", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                            cmdInsert.Parameters.Add(new SQLiteParameter("@tokenUri", a.tokenUri));
                            cmdInsert.ExecuteNonQuery();
                            long lastID = conn.LastInsertRowId;

                            if (lastID != 0)
                            {
                                for (int i = 1; i <= config._numberOfValidators; i++)
                                {
                                    var cmdInsertValidatorMeta = new SQLiteCommand("Insert into ValidatorMetadata (bridgeNFTId,validatorNumber) select @bridgeNFTId,@validatorNumber", conn);
                                    cmdInsertValidatorMeta.Parameters.Add(new SQLiteParameter("@bridgeNFTId", lastID));
                                    cmdInsertValidatorMeta.Parameters.Add(new SQLiteParameter("@validatorNumber", i));
                                    cmdInsertValidatorMeta.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
        }

        public void UpdateSignMessage(string originOwner, string contractAddress, int tokenId, string signMessage, string validatorNumber)
        {
            try
            {
                int bridgeNFTID = 0;
                var cmdSelect = new SQLiteCommand();
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {

                        cmdSelect = new SQLiteCommand("SELECT id from BridgeNFT WHERE contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmdSelect.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        using (SQLiteDataReader dr = cmdSelect.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                bridgeNFTID = Convert.ToInt32(dr["id"]);
                            }
                        }

                        if (bridgeNFTID != 0)
                        {
                            var cmdUpdate = new SQLiteCommand("Update ValidatorMetadata SET mintSign = @signMessage WHERE bridgenftid = @bridgeNftId and validatorNumber = @validator", conn);

                            cmdUpdate.Parameters.Add(new SQLiteParameter("@bridgeNftId", bridgeNFTID));
                            cmdUpdate.Parameters.Add(new SQLiteParameter("@signMessage", signMessage));
                            cmdUpdate.Parameters.Add(new SQLiteParameter("@validator", validatorNumber));
                            cmdUpdate.ExecuteNonQuery();

                            cmdUpdate = new SQLiteCommand("Update BridgeNFT SET status = 'Signed' where status = 'Pending' and (select count(*) from ValidatorMetadata where mintSign = '' and bridgenftid = BridgeNFT.id) = 0;", conn);
                            cmdUpdate.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
        }

        public void UpdateOfferSignMessage(string originOwner, string contractAddress, int tokenId, string signMessage, string validatorNumber, string xrplTokenId)
        {
            try
            {
                int bridgeNFTID = 0;
                var cmdSelect = new SQLiteCommand();
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {

                        cmdSelect = new SQLiteCommand("SELECT id from BridgeNFT WHERE contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmdSelect.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        using (SQLiteDataReader dr = cmdSelect.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                bridgeNFTID = Convert.ToInt32(dr["id"]);
                            }
                        }

                        if (bridgeNFTID != 0)
                        {
                            var cmdUpdate = new SQLiteCommand("Update BridgeNFT SET xrplTokenId = @xrplTokenId WHERE id = @bridgeNftId", conn);

                            cmdUpdate.Parameters.Add(new SQLiteParameter("@bridgeNftId", bridgeNFTID));
                            cmdUpdate.Parameters.Add(new SQLiteParameter("@xrplTokenId", xrplTokenId));
                            cmdUpdate.ExecuteNonQuery();

                            cmdUpdate = new SQLiteCommand("Update ValidatorMetadata SET mintOfferSigned = @signMessage WHERE bridgenftid = @bridgeNftId and validatorNumber = @validator", conn);

                            cmdUpdate.Parameters.Add(new SQLiteParameter("@bridgeNftId", bridgeNFTID));
                            cmdUpdate.Parameters.Add(new SQLiteParameter("@signMessage", signMessage));
                            cmdUpdate.Parameters.Add(new SQLiteParameter("@validator", validatorNumber));
                            cmdUpdate.ExecuteNonQuery();

                            cmdUpdate = new SQLiteCommand("Update BridgeNFT SET status = 'OfferSigned' where status = 'OfferCreate' and (select count(*) from ValidatorMetadata where mintOfferSigned = '' and bridgenftid = BridgeNFT.id) = 0;", conn);
                            cmdUpdate.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
        }

        public void UpdateSignMessageCombined(string originOwner, string contractAddress, int tokenId, string signMessage, string validatorNumber, Settings config)
        {
            try
            {
                int bridgeNFTID = 0;
                var cmdSelect = new SQLiteCommand();
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        cmdSelect = new SQLiteCommand("SELECT id from BridgeNFT WHERE contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmdSelect.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        using (SQLiteDataReader dr = cmdSelect.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                bridgeNFTID = Convert.ToInt32(dr["id"]);
                            }
                        }

                        if (bridgeNFTID != 0)
                        {

                        }
                        var cmdUpdate = new SQLiteCommand("Update ValidatorMetadata SET mintSignCombined = @signMessage WHERE bridgenftid = @bridgeNftId and validatorNumber = @validator", conn);

                        cmdUpdate.Parameters.Add(new SQLiteParameter("@bridgeNftId", bridgeNFTID));
                        cmdUpdate.Parameters.Add(new SQLiteParameter("@signMessage", signMessage));
                        cmdUpdate.Parameters.Add(new SQLiteParameter("@validator", validatorNumber));
                        cmdUpdate.ExecuteNonQuery();

                        cmdUpdate = new SQLiteCommand("Update BridgeNFT SET status = 'Validator Agreement' where status = 'Signed' and (select count(*) from ValidatorMetadata where mintSign = '' and bridgenftid = BridgeNFT.id) = 0;", conn);
                        cmdUpdate.Parameters.Add(new SQLiteParameter("@numValidators", config._numberOfValidators));
                        cmdUpdate.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
        }

        public void UpdateSignOfferMessageCombined(string originOwner, string contractAddress, int tokenId, string signMessage, string validatorNumber, Settings config)
        {
            try
            {
                int bridgeNFTID = 0;
                var cmdSelect = new SQLiteCommand();
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        cmdSelect = new SQLiteCommand("SELECT id from BridgeNFT WHERE contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmdSelect.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmdSelect.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        using (SQLiteDataReader dr = cmdSelect.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                bridgeNFTID = Convert.ToInt32(dr["id"]);
                            }
                        }

                        if (bridgeNFTID != 0)
                        {

                        }
                        var cmdUpdate = new SQLiteCommand("Update ValidatorMetadata SET mintOfferSignedCombined = @signMessage WHERE bridgenftid = @bridgeNftId and validatorNumber = @validator", conn);

                        cmdUpdate.Parameters.Add(new SQLiteParameter("@bridgeNftId", bridgeNFTID));
                        cmdUpdate.Parameters.Add(new SQLiteParameter("@signMessage", signMessage));
                        cmdUpdate.Parameters.Add(new SQLiteParameter("@validator", validatorNumber));
                        cmdUpdate.ExecuteNonQuery();

                        cmdUpdate = new SQLiteCommand("Update BridgeNFT SET status = 'Validator Agreement Offer' where status = 'OfferSigned' and (select count(*) from ValidatorMetadata where mintOfferSigned = '' and bridgenftid = BridgeNFT.id) = 0;", conn);
                        cmdUpdate.Parameters.Add(new SQLiteParameter("@numValidators", config._numberOfValidators));
                        cmdUpdate.ExecuteNonQuery();
                        transaction.Commit();
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
        }


        public List<BridgeNFT> GetPendingNFTs(string validator)
        {
            List<BridgeNFT> list = new List<BridgeNFT>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "Select BridgeNFT.* from BridgeNFT Inner join ValidatorMetadata on BridgeNFT.id = ValidatorMetadata.bridgeNFTId  Where status = 'Pending' and (Select count(*) from BridgeNFT where status != 'Pending' and status != 'Error' and status != 'Previously Offered' and status != 'Previously Minted') = 0 and ValidatorMetadata.mintSign = '' and validatorNumber = @validatorNum order by id asc LIMIT 1";
                    cmd.Parameters.Add(new SQLiteParameter("@validatorNum", validator));
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new BridgeNFT
                            {
                                id = Convert.ToInt32(dr["id"]),
                                contractAddress = dr["contractAddress"].ToString(),
                                originOwner = dr["originOwner"].ToString(),
                                tokenId = Convert.ToInt32(dr["tokenId"]),
                                xrplAddress = dr["xrplAddress"].ToString(),
                                blockNumber = Convert.ToInt32(dr["blockNumber"].ToString()),
                                xrplTokenId = dr["xrplTokenId"].ToString(),
                                nft_minted = Convert.ToInt32(dr["nft_minted"].ToString()),
                                nft_mint_txn_hash = dr["nft_mint_txn_hash"].ToString(),
                                nft_offer_txn_hash = dr["nft_offer_txn_hash"].ToString(),
                                status = dr["status"].ToString(),
                                last_updated = Convert.ToInt32(dr["last_updated"].ToString()),
                                tokenUri = dr["tokenUri"].ToString(),
                            });
                        }
                    }
                }
                conn.Close();
            }
            return list;
        }

        public List<BridgeNFT> GetPendingNFTs()
        {
            List<BridgeNFT> list = new List<BridgeNFT>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "Select BridgeNFT.* from BridgeNFT Inner join ValidatorMetadata on BridgeNFT.id = ValidatorMetadata.bridgeNFTId  Where status = 'Pending' and (Select count(*) from BridgeNFT where status != 'Pending' and status != 'Error' and status != 'OfferCompleted' and status != 'Previously Offered' and status != 'Previously Minted') = 0 and ValidatorMetadata.mintSign = '' order by id asc LIMIT 1";
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new BridgeNFT
                            {
                                id = Convert.ToInt32(dr["id"]),
                                contractAddress = dr["contractAddress"].ToString(),
                                originOwner = dr["originOwner"].ToString(),
                                tokenId = Convert.ToInt32(dr["tokenId"]),
                                xrplAddress = dr["xrplAddress"].ToString(),
                                blockNumber = Convert.ToInt32(dr["blockNumber"].ToString()),
                                xrplTokenId = dr["xrplTokenId"].ToString(),
                                nft_minted = Convert.ToInt32(dr["nft_minted"].ToString()),
                                nft_mint_txn_hash = dr["nft_mint_txn_hash"].ToString(),
                                nft_offer_txn_hash = dr["nft_offer_txn_hash"].ToString(),
                                status = dr["status"].ToString(),
                                last_updated = Convert.ToInt32(dr["last_updated"].ToString()),
                                tokenUri = dr["tokenUri"].ToString(),
                            });
                        }
                    }
                }
                conn.Close();
            }
            return list;
        }
        public List<BridgeNFT> GetPendingOfferSignedNFTs(string validator)
        {
            List<BridgeNFT> list = new List<BridgeNFT>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "Select BridgeNFT.* from BridgeNFT Inner join ValidatorMetadata on BridgeNFT.id = ValidatorMetadata.bridgeNFTId  Where status = 'OfferCreate' and ValidatorMetadata.mintOfferSigned = '' and validatorNumber = @validatorNum order by id asc";
                    cmd.Parameters.Add(new SQLiteParameter("@validatorNum", validator));
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List<ValidatorMetadata> v = new List<ValidatorMetadata>();
                            using (SQLiteCommand cmd2 = new SQLiteCommand(conn))
                            {
                                cmd2.CommandText = "select * from ValidatorMetadata WHERE bridgeNFTId = @bridgeId";
                                cmd2.Parameters.Add(new SQLiteParameter("@bridgeId", Convert.ToInt32(dr["id"])));
                                using (SQLiteDataReader dr2 = cmd2.ExecuteReader())
                                {
                                    while (dr2.Read())
                                    {
                                        v.Add(new ValidatorMetadata
                                        {
                                            agreeMint = Convert.ToInt32(dr2["agreeMint"]),
                                            agreeOffer = Convert.ToInt32(dr2["agreeOffer"]),
                                            bridgeNFTId = Convert.ToInt32(dr2["bridgeNFTId"]),
                                            id = Convert.ToInt32(dr2["id"]),
                                            mintSign = dr2["mintSign"].ToString(),
                                            mintOfferSigned = dr2["mintOfferSigned"].ToString(),
                                            mintOfferSignedCombined = dr2["mintOfferSignedCombined"].ToString(),
                                            mintSignCombined = dr2["mintSignCombined"].ToString(),
                                            validatorNumber = dr2["validatorNumber"].ToString()
                                        });
                                    }
                                }
                            }

                            list.Add(new BridgeNFT
                            {
                                id = Convert.ToInt32(dr["id"]),
                                contractAddress = dr["contractAddress"].ToString(),
                                originOwner = dr["originOwner"].ToString(),
                                tokenId = Convert.ToInt32(dr["tokenId"]),
                                xrplAddress = dr["xrplAddress"].ToString(),
                                blockNumber = Convert.ToInt32(dr["blockNumber"].ToString()),
                                xrplTokenId = dr["xrplTokenId"].ToString(),
                                nft_minted = Convert.ToInt32(dr["nft_minted"].ToString()),
                                nft_mint_txn_hash = dr["nft_mint_txn_hash"].ToString(),
                                nft_offer_txn_hash = dr["nft_offer_txn_hash"].ToString(),
                                status = dr["status"].ToString(),
                                last_updated = Convert.ToInt32(dr["last_updated"].ToString()),
                                validatorMeta = v
                            });
                        }
                    }
                }
                conn.Close();
            }
            return list;
        }

        public List<BridgeNFT> GetNFTsReadyToBeExecuted()
        {
            List<BridgeNFT> list = new List<BridgeNFT>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    var cmdUpdate = new SQLiteCommand("Update BridgeNFT SET status = 'Signed' where status = 'Pending' and (select count(*) from ValidatorMetadata where mintSign = '' and bridgenftid = BridgeNFT.id) = 0;", conn);
                    cmdUpdate.ExecuteNonQuery();

                    cmd.CommandText = "select BridgeNFT.* from BridgeNFT WHERE status = 'Signed' and (select count(*) from ValidatorMetadata where mintSign = '' and bridgenftid = BridgeNFT.id) = 0";
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List<ValidatorMetadata> v = new List<ValidatorMetadata>();
                            using (SQLiteCommand cmd2 = new SQLiteCommand(conn))
                            {
                                cmd2.CommandText = "select * from ValidatorMetadata WHERE bridgeNFTId = @bridgeId";
                                cmd2.Parameters.Add(new SQLiteParameter("@bridgeId", Convert.ToInt32(dr["id"])));
                                using (SQLiteDataReader dr2 = cmd2.ExecuteReader())
                                {
                                    while (dr2.Read())
                                    {
                                        v.Add(new ValidatorMetadata
                                        {
                                            agreeMint = Convert.ToInt32(dr2["agreeMint"]),
                                            agreeOffer = Convert.ToInt32(dr2["agreeOffer"]),
                                            bridgeNFTId = Convert.ToInt32(dr2["bridgeNFTId"]),
                                            id = Convert.ToInt32(dr2["id"]),
                                            mintSign = dr2["mintSign"].ToString(),
                                            mintOfferSigned = dr2["mintOfferSigned"].ToString(),
                                            mintOfferSignedCombined = dr2["mintOfferSignedCombined"].ToString(),
                                            mintSignCombined = dr2["mintSignCombined"].ToString(),
                                            validatorNumber = dr2["validatorNumber"].ToString()
                                        });
                                    }
                                }
                            }

                            list.Add(new BridgeNFT
                            {
                                id = Convert.ToInt32(dr["id"]),
                                contractAddress = dr["contractAddress"].ToString(),
                                originOwner = dr["originOwner"].ToString(),
                                tokenId = Convert.ToInt32(dr["tokenId"]),
                                xrplAddress = dr["xrplAddress"].ToString(),
                                blockNumber = Convert.ToInt32(dr["blockNumber"].ToString()),
                                xrplTokenId = dr["xrplTokenId"].ToString(),
                                nft_minted = Convert.ToInt32(dr["nft_minted"].ToString()),
                                nft_mint_txn_hash = dr["nft_mint_txn_hash"].ToString(),
                                nft_offer_txn_hash = dr["nft_offer_txn_hash"].ToString(),
                                status = dr["status"].ToString(),
                                last_updated = Convert.ToInt32(dr["last_updated"].ToString()),
                                tokenUri = dr["tokenUri"].ToString(),
                                validatorMeta = v
                            });
                        }
                    }
                }
                conn.Close();
            }
            return list;
        }

        public List<BridgeNFT> GetNFTsReadyToBeExecutedOffer()
        {
            List<BridgeNFT> list = new List<BridgeNFT>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    var cmdUpdate = new SQLiteCommand("Update BridgeNFT SET status = 'OfferSigned' where status = 'OfferCreate' and (select count(*) from ValidatorMetadata where mintOfferSigned = '' and bridgenftid = BridgeNFT.id) = 0;", conn);
                    cmdUpdate.ExecuteNonQuery();

                    cmd.CommandText = "select BridgeNFT.* from BridgeNFT WHERE status = 'OfferSigned' and (select count(*) from ValidatorMetadata where mintOfferSigned = '' and bridgenftid = BridgeNFT.id) = 0";
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List<ValidatorMetadata> v = new List<ValidatorMetadata>();
                            using (SQLiteCommand cmd2 = new SQLiteCommand(conn))
                            {
                                cmd2.CommandText = "select * from ValidatorMetadata WHERE bridgeNFTId = @bridgeId";
                                cmd2.Parameters.Add(new SQLiteParameter("@bridgeId", Convert.ToInt32(dr["id"])));
                                using (SQLiteDataReader dr2 = cmd2.ExecuteReader())
                                {
                                    while (dr2.Read())
                                    {
                                        v.Add(new ValidatorMetadata
                                        {
                                            agreeMint = Convert.ToInt32(dr2["agreeMint"]),
                                            agreeOffer = Convert.ToInt32(dr2["agreeOffer"]),
                                            bridgeNFTId = Convert.ToInt32(dr2["bridgeNFTId"]),
                                            id = Convert.ToInt32(dr2["id"]),
                                            mintSign = dr2["mintSign"].ToString(),
                                            mintOfferSigned = dr2["mintOfferSigned"].ToString(),
                                            mintOfferSignedCombined = dr2["mintOfferSignedCombined"].ToString(),
                                            mintSignCombined = dr2["mintSignCombined"].ToString(),
                                            validatorNumber = dr2["validatorNumber"].ToString()
                                        });
                                    }
                                }
                            }

                            list.Add(new BridgeNFT
                            {
                                id = Convert.ToInt32(dr["id"]),
                                contractAddress = dr["contractAddress"].ToString(),
                                originOwner = dr["originOwner"].ToString(),
                                tokenId = Convert.ToInt32(dr["tokenId"]),
                                xrplAddress = dr["xrplAddress"].ToString(),
                                blockNumber = Convert.ToInt32(dr["blockNumber"].ToString()),
                                xrplTokenId = dr["xrplTokenId"].ToString(),
                                nft_minted = Convert.ToInt32(dr["nft_minted"].ToString()),
                                nft_mint_txn_hash = dr["nft_mint_txn_hash"].ToString(),
                                nft_offer_txn_hash = dr["nft_offer_txn_hash"].ToString(),
                                status = dr["status"].ToString(),
                                last_updated = Convert.ToInt32(dr["last_updated"].ToString()),
                                tokenUri = dr["tokenUri"].ToString(),
                                validatorMeta = v
                            });
                        }
                    }
                }
                conn.Close();
            }
            return list;
        }

        public List<BridgeNFT> GetRecordsByStatus(string status)
        {
            List<BridgeNFT> list = new List<BridgeNFT>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "Select BridgeNFT.* from BridgeNFT Where status = @status";
                    cmd.Parameters.Add(new SQLiteParameter("@status", status));
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            List<ValidatorMetadata> v = new List<ValidatorMetadata>();
                            using (SQLiteCommand cmd2 = new SQLiteCommand(conn))
                            {
                                cmd2.CommandText = "select * from ValidatorMetadata WHERE bridgeNFTId = @bridgeId";
                                cmd2.Parameters.Add(new SQLiteParameter("@bridgeId", Convert.ToInt32(dr["id"])));
                                using (SQLiteDataReader dr2 = cmd2.ExecuteReader())
                                {
                                    while (dr2.Read())
                                    {
                                        v.Add(new ValidatorMetadata
                                        {
                                            agreeMint = Convert.ToInt32(dr2["agreeMint"]),
                                            agreeOffer = Convert.ToInt32(dr2["agreeOffer"]),
                                            bridgeNFTId = Convert.ToInt32(dr2["bridgeNFTId"]),
                                            id = Convert.ToInt32(dr2["id"]),
                                            mintSign = dr2["mintSign"].ToString(),
                                            mintOfferSigned = dr2["mintOfferSigned"].ToString(),
                                            mintOfferSignedCombined = dr2["mintOfferSignedCombined"].ToString(),
                                            mintSignCombined = dr2["mintSignCombined"].ToString(),
                                            validatorNumber = dr2["validatorNumber"].ToString()
                                        });
                                    }
                                }
                            }
                            list.Add(new BridgeNFT
                            {
                                id = Convert.ToInt32(dr["id"]),
                                contractAddress = dr["contractAddress"].ToString(),
                                originOwner = dr["originOwner"].ToString(),
                                tokenId = Convert.ToInt32(dr["tokenId"]),
                                xrplAddress = dr["xrplAddress"].ToString(),
                                blockNumber = Convert.ToInt32(dr["blockNumber"].ToString()),
                                xrplTokenId = dr["xrplTokenId"].ToString(),
                                nft_minted = Convert.ToInt32(dr["nft_minted"].ToString()),
                                nft_mint_txn_hash = dr["nft_mint_txn_hash"].ToString(),
                                nft_offer_txn_hash = dr["nft_offer_txn_hash"].ToString(),
                                status = dr["status"].ToString(),
                                last_updated = Convert.ToInt32(dr["last_updated"].ToString()),
                                tokenUri = dr["tokenUri"].ToString(),
                                validatorMeta = v
                            });
                        }
                    }
                }
                conn.Close();
            }
            return list;
        }

        public void UpdateTransactionHash(string status, string contractAddress, string originOwner, int tokenId, int verified, Submit response)
        {
            DateTime now = DateTime.Now;
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    var cmd = new SQLiteCommand();
                    conn.Open();
                    if (response != null)
                    {
                        cmd = new SQLiteCommand("Update BridgeNFT SET status = @status, last_updated = @datetime, nft_minted = @verified, nft_mint_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmd.Parameters.Add(new SQLiteParameter("@status", status));
                        cmd.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmd.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmd.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        cmd.Parameters.Add(new SQLiteParameter("@verified", verified));
                        cmd.Parameters.Add(new SQLiteParameter("@txnHash", response.Transaction.Hash));
                        cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    }
                    else
                    {
                        cmd = new SQLiteCommand("Update BridgeNFT SET status = @status, last_updated = @datetime, nft_minted = @verified, nft_mint_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmd.Parameters.Add(new SQLiteParameter("@status", status));
                        cmd.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmd.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmd.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        cmd.Parameters.Add(new SQLiteParameter("@verified", verified));
                        cmd.Parameters.Add(new SQLiteParameter("@txnHash", response.EngineResultMessage));
                        cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    }

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void UpdateTransactionHash(string status, string contractAddress, string originOwner, int tokenId, int verified, string errMsg)
        {
            DateTime now = DateTime.Now;
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    var cmd = new SQLiteCommand();
                    conn.Open();

                    cmd = new SQLiteCommand("Update BridgeNFT SET status = @status, last_updated = @datetime, nft_minted = @verified, nft_mint_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                    cmd.Parameters.Add(new SQLiteParameter("@status", status));
                    cmd.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                    cmd.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                    cmd.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                    cmd.Parameters.Add(new SQLiteParameter("@verified", verified));
                    cmd.Parameters.Add(new SQLiteParameter("@txnHash", errMsg));
                    cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void UpdateTransactionHashOffer(string status, string contractAddress, string originOwner, int tokenId, Submit response)
        {
            DateTime now = DateTime.Now;
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    var cmd = new SQLiteCommand();
                    conn.Open();
                    if (response != null)
                    {
                        cmd = new SQLiteCommand("Update BridgeNFT SET status = @status, last_updated = @datetime, nft_offer_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmd.Parameters.Add(new SQLiteParameter("@status", status));
                        cmd.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmd.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmd.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        cmd.Parameters.Add(new SQLiteParameter("@txnHash", response.Transaction.Hash));
                        cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    }
                    else
                    {
                        cmd = new SQLiteCommand("Update BridgeNFT SET status = @status, last_updated = @datetime, nft_offer_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                        cmd.Parameters.Add(new SQLiteParameter("@status", status));
                        cmd.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                        cmd.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                        cmd.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                        cmd.Parameters.Add(new SQLiteParameter("@txnHash", response.EngineResultMessage));
                        cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    }

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void UpdateTransactionHashOffer(string status, string contractAddress, string originOwner, int tokenId, int verified, string errMsg)
        {
            DateTime now = DateTime.Now;
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    var cmd = new SQLiteCommand();
                    conn.Open();

                    cmd = new SQLiteCommand("Update BridgeNFT SET status = @status, last_updated = @datetime, nft_offer_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                    cmd.Parameters.Add(new SQLiteParameter("@status", status));
                    cmd.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                    cmd.Parameters.Add(new SQLiteParameter("@originOwner", originOwner));
                    cmd.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                    cmd.Parameters.Add(new SQLiteParameter("@txnHash", errMsg));
                    cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));

                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public List<BridgeNFT> GetNFTsReadyToBeClaimed(string xrpAddress)
        {
            List<BridgeNFT> list = new List<BridgeNFT>();
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "Select BridgeNFT.* from BridgeNFT Where status = 'OfferCompleted' and xrplAddress = @xrpAddress";
                    cmd.Parameters.Add(new SQLiteParameter("@xrpAddress", xrpAddress));
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            list.Add(new BridgeNFT
                            {
                                id = Convert.ToInt32(dr["id"]),
                                contractAddress = dr["contractAddress"].ToString(),
                                originOwner = dr["originOwner"].ToString(),
                                tokenId = Convert.ToInt32(dr["tokenId"]),
                                xrplAddress = dr["xrplAddress"].ToString(),
                                blockNumber = Convert.ToInt32(dr["blockNumber"].ToString()),
                                xrplTokenId = dr["xrplTokenId"].ToString(),
                                nft_minted = Convert.ToInt32(dr["nft_minted"].ToString()),
                                nft_mint_txn_hash = dr["nft_mint_txn_hash"].ToString(),
                                nft_offer_txn_hash = dr["nft_offer_txn_hash"].ToString(),
                                status = dr["status"].ToString(),
                                last_updated = Convert.ToInt32(dr["last_updated"].ToString()),
                                tokenUri = dr["tokenUri"].ToString()
                            });
                        }
                    }
                }
                conn.Close();
            }
            return list;
        }

        public string GetStatus(int tokenId, string contractAddress)
        {
            using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
            {
                conn.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(conn))
                {
                    cmd.CommandText = "Select status from BridgeNFT Where tokenId = @tokenId and lower(contractAddress) = lower(@contractAddress) order by id desc LIMIT 1 ";
                    cmd.Parameters.Add(new SQLiteParameter("@tokenId", tokenId));
                    cmd.Parameters.Add(new SQLiteParameter("@contractAddress", contractAddress));
                    using (SQLiteDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            return dr["status"].ToString();
                        }
                    }
                }
                conn.Close();
            }
            return "";
        }
        public void UpdateMinted(MemoFragment memo, string txnHash)
        {
            DateTime now = DateTime.Now;
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    var cmd = new SQLiteCommand();
                    conn.Open();
                    cmd = new SQLiteCommand("Update BridgeNFT SET status = 'Previously Minted', last_updated = @datetime, nft_minted = @minted, nft_mint_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                    cmd.Parameters.Add(new SQLiteParameter("@contractAddress", memo.contractAddress));
                    cmd.Parameters.Add(new SQLiteParameter("@originOwner", memo.originOwner));
                    cmd.Parameters.Add(new SQLiteParameter("@tokenId", memo.tokenId));
                    cmd.Parameters.Add(new SQLiteParameter("@minted", 1));
                    cmd.Parameters.Add(new SQLiteParameter("@txnHash", txnHash));
                    cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void UpdateOffered(MemoFragment memo, string txnHash)
        {
            DateTime now = DateTime.Now;
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    var cmd = new SQLiteCommand();
                    conn.Open();
                    cmd = new SQLiteCommand("Update BridgeNFT SET status = 'Previously Offered', last_updated = @datetime, nft_offer_txn_hash = @txnHash where contractAddress = @contractAddress and originOwner = @originOwner and tokenId = @tokenId", conn);
                    cmd.Parameters.Add(new SQLiteParameter("@contractAddress", memo.contractAddress));
                    cmd.Parameters.Add(new SQLiteParameter("@originOwner", memo.originOwner));
                    cmd.Parameters.Add(new SQLiteParameter("@tokenId", memo.tokenId));
                    cmd.Parameters.Add(new SQLiteParameter("@txnHash", txnHash));
                    cmd.Parameters.Add(new SQLiteParameter("@datetime", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public void ValidatorUpdatePing(string validatorNumber)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection(connectionstring))
                {
                    conn.Open();

                        var cmdInsert = new SQLiteCommand("Insert into ValidatorStatus (validatorNumber,last_pinged) select @validatorNumber,@last_pinged WHERE (SELECT COUNT(*) FROM ValidatorStatus WHERE validatorNumber = @validatorNumber) = 0", conn);
                        cmdInsert.Parameters.Add(new SQLiteParameter("@validatorNumber", validatorNumber));
                        cmdInsert.Parameters.Add(new SQLiteParameter("@last_pinged", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                        cmdInsert.ExecuteNonQuery();

                        var cmdUpdate = new SQLiteCommand("Update ValidatorStatus SET last_pinged = @last_pinged WHERE validatorNumber = @validatorNumber", conn);

                        cmdUpdate.Parameters.Add(new SQLiteParameter("@validatorNumber", validatorNumber));
                        cmdUpdate.Parameters.Add(new SQLiteParameter("@last_pinged", DateTimeOffset.UtcNow.ToUnixTimeSeconds()));
                        cmdUpdate.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {

            }
        }

        public List<string> GetValidatorsToPing(int timeOffSetMins)
        {
            try
            {
                List<string> validators = new List<string>();
                long offSet = (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - (timeOffSetMins * 60));
                using (SQLiteConnection conn = new SQLiteConnection(connectionstring))
                {
                    conn.Open();
                    using (SQLiteCommand cmd = new SQLiteCommand(conn))
                    {
                        cmd.CommandText = "Select last_pinged from ValidatorStatus Where last_pinged < @offSet";
                        cmd.Parameters.Add(new SQLiteParameter("@offSet", offSet));
                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                            {
                                validators.Add(dr["validatorNumber"].ToString());
                            }
                        }
                    }
                    conn.Close();
                    return validators;
                }
            }
            catch (Exception){ return new List<string>(); }
        }
    }

    public class ValidatorMetadata
    {
        public int id { get; set; }
        public int bridgeNFTId { get; set; }
        public string validatorNumber { get; set; }
        public string mintSign { get; set; }
        public string mintSignCombined { get; set; }
        public string mintOfferSigned { get; set; }
        public string mintOfferSignedCombined { get; set; }
        public int agreeMint { get; set; }
        public int agreeOffer { get; set; }
    }

    public class BridgeNFT
    {
        public int id { get; set; }
        public string contractAddress { get; set; }
        public string originOwner { get; set; }
        public int tokenId { get; set; }
        public string xrplAddress { get; set; }
        public int blockNumber { get; set; }
        public string xrplTokenId { get; set; }
        public int nft_minted { get; set; }
        public string nft_mint_txn_hash { get; set; }
        public string nft_offer_txn_hash { get; set; }
        public string status { get; set; }
        public int last_updated { get; set; }
        public string tokenUri { get; set; }
        public List<ValidatorMetadata> validatorMeta { get; set; }

    }
}
