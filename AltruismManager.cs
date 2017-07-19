using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace Blockchain.Altruism
{
    class AltruismManager
    {
        private string ownerAddress;
        private string ownerPassword;
        private string ownerContract;
        private Web3 web3;
        private bool isConnected;
        List<string> contributors;

        public AltruismManager(string address, string password, string contract)
        {
            if (string.IsNullOrWhiteSpace(address))
                throw new ArgumentNullException("address");
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(contract))
                throw new ArgumentNullException("contract");
            ownerAddress = address;
            ownerPassword = password;
            ownerContract = contract;
            isConnected = false;
            contributors = new List<string>();
        }

        public void Connect(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException("url");
            try
            {
                web3 = new Web3(url);
            }
            catch { }
            web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
            isConnected = true;
        }

        public void Subscribe()
        {
            AltruismServices svc = new AltruismServices();
            int lastBlock = svc.GetLastBlock();
            while (true)
            {
                // Ask Etherscan
                lastBlock = ParseTransactions(svc, lastBlock);
                // Let's see...
                ParseUnmanagedTransactions(svc);
                //Save the last block read
                svc.UpdateProperty("lastBlock", lastBlock.ToString());
                // Now, let's sleep for 5 min
                Thread.Sleep(300000);
            }
        }

        private int ParseTransactions(AltruismServices svc, int fromBlock)
        {
            EtherScanApi api = new EtherScanApi();
            bool stopLastBlock = false;
            int lastBlock = fromBlock;
            EtherScanAddress transactions = api.GetTransactions(ownerContract, lastBlock);
            if (transactions != null && transactions.result != null && transactions.result.Any())
            {
                foreach (EtherScanTx tx in transactions.result)
                {
                    if (tx.confirmations == 0)
                    {
                        // OK we got some pending tx
                        // we will parse again from this block
                        // So no need to update the last block read
                        if (!stopLastBlock)
                        {
                            stopLastBlock = true;
                            lastBlock = tx.blockNumber;
                        }
                    }
                    else
                    {
                        // Get the tx from its hash
                        TransactionModel model = svc.GetTransaction(tx.hash);
                        if (model == null)
                        {
                            // New tx ? Add it to the database
                            model = new TransactionModel(tx);
                            svc.AddTransaction(model);
                        }
                        if (!stopLastBlock)
                        {
                            // If no pending tx, save the last block read
                            lastBlock = tx.blockNumber;
                        }
                    }
                }
            }
            return lastBlock;
        }

        private void ParseUnmanagedTransactions(AltruismServices svc)
        {
            BigInteger big10Finney = BigInteger.Parse("10000000000000000"); // 0.01 ETH
            BigInteger big30Finney = BigInteger.Parse("30000000000000000"); // 0.03 ETH
            BigInteger big40Finney = BigInteger.Parse("40000000000000000"); // 0.04 ETH
            BigInteger bigDivision = new BigInteger(3.0);
            // Get the unmanaged tx
            IEnumerable<TransactionModel> toManage = svc.GetUnmanagedTransactions();
            if (toManage != null && toManage.Any())
            {
                // Get the past contributors
                List<string> contributors = svc.GetContributors();
                foreach (TransactionModel model in toManage)
                {
                    if (model.isError != 0)
                    {
                        // This tx is on error => Done.
                        svc.SetManaged(model.hash, false, false);
                        continue;
                    }
                    if (model.to != ownerContract)
                    {
                        // This tx does not concern us -> Done.
                        svc.SetManaged(model.hash, false, false);
                        continue;
                    }
                    if (BigInteger.Compare(model.BigValue, big10Finney) < 0)
                    {
                        // This tx is less than 0.01 ETH. Must be on error. Do I jump here ? => Done.
                        svc.SetManaged(model.hash, false, false);
                        continue;
                    }
                    if (!contributors.Contains(model.from))
                    {
                        // Hoo a new contributor. Save it.
                        contributors.Add(model.from);
                        svc.AddContributor(model.from);
                    }
                    if (BigInteger.Compare(model.BigValue, big40Finney) == 0)
                    {
                        // Hacked mode enabled => Done.
                        svc.SetManaged(model.hash, false, true);
                        continue;
                    }
                    if (BigInteger.Compare(model.BigValue, big30Finney) < 0)
                    {
                        // No Altruism mode => Done.
                        svc.SetManaged(model.hash, false, false);
                        continue;
                    }
                    // Altruist one \o/
                    // Take a third of the value
                    BigInteger toSend = BigInteger.Divide(model.BigValue, bigDivision);
                    Random rnd = new Random();
                    // Pick a random number
                    int next = rnd.Next(contributors.Count);
                    // Get the winner
                    string winnerAddress = contributors.ElementAt(next);
                    // Unlock my account
                    Task<bool> unlocked = web3.Personal.UnlockAccount.SendRequestAsync(ownerAddress, ownerPassword, new HexBigInteger(60));
                    if (unlocked.Result)
                    {
                        TransactionInput input = new TransactionInput();
                        input.From = ownerAddress;
                        input.To = winnerAddress;
                        input.Value = new HexBigInteger(toSend);
                        // Generate the tx
                        Task<string> txHash = web3.Eth.Transactions.SendTransaction.SendRequestAsync(input);
                        // Get the receipt tx
                        Task<TransactionReceipt> receipt = web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(txHash.Result);
                        // Save the tx => Done.
                        svc.SetManaged(model.hash, true, false, txHash.Result);
                    }
                }
            }
        }
    }
}
