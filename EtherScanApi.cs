using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Blockchain.Altruism
{
    class EtherScanApi
    {
        public const string accountQuery = "?module=account&action=txlist&address={0}&startblock={1}&endblock=99999999&sort=desc&apikey=YourApiKeyToken";
        public const string urlApi = "https://api.etherscan.io/api";

        public EtherScanAddress GetTransactions(string address, int start = 0)
        {
            EtherScanAddress response;
            using (HttpClient wc = new HttpClient())
            {
                wc.BaseAddress = new Uri(urlApi);
                var json = wc.GetAsync(string.Format(accountQuery, address, start));
                string r = json.Result.Content.ReadAsStringAsync().Result;
                response = JsonConvert.DeserializeObject<EtherScanAddress>(r);
                return response;
            }
        }
    }

    public class EtherScanAddress
    {
        public int status { get; set; }
        public string message { get; set; }
        public List<EtherScanTx> result { get; set; }
    }

    public class EtherScanTx
    {
        public int blockNumber { get; set; }
        public long timeStamp { get; set; }
        public string hash { get; set; }
        public int nonce { get; set; }
        public string blockHash { get; set; }
        public int transactionIndex { get; set; }
        public string from { get; set; }
        public string to { get; set; }
        public string value { get; set; }
        public long gas { get; set; }
        public long gasPrice { get; set; }
        public int isError { get; set; }
        public string input { get; set; }
        public string contractAddress { get; set; }
        public long cumulativeGasUsed { get; set; }
        public long gasUsed { get; set; }
        public int confirmations { get; set; }
    }

}
