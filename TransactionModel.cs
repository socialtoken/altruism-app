using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Blockchain.Altruism
{
    [Table("transaction")]
    class TransactionModel
    {
        [Key]
        public string hash { get; set; }
        public int blockNumber { get; set; }
        public long timeStamp { get; set; }
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
        public bool altruismMode { get; set; }
        public bool hackedMode { get; set; }
        public bool managed { get; set; }
        public string altruistTx { get; set; }
        public BigInteger BigValue { get { return BigInteger.Parse(value); } }

        public TransactionModel() { }
        public TransactionModel(EtherScanTx tx)
        {
            blockNumber = tx.blockNumber;
            timeStamp = tx.timeStamp;
            hash = tx.hash;
            nonce = tx.nonce;
            blockHash = tx.blockHash;
            transactionIndex = tx.transactionIndex;
            from = tx.from;
            to = tx.to;
            value = tx.value;
            gas = tx.gas;
            gasPrice = tx.gasPrice;
            isError = tx.isError;
            input = tx.input;
            contractAddress = tx.contractAddress;
            cumulativeGasUsed = tx.cumulativeGasUsed;
            gasUsed = tx.gasUsed;
            confirmations = tx.confirmations;
            altruismMode = false;
            hackedMode = false;
            managed = false;
        }
    }

    [Table("Property")]
    class PropertyModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    [Table("contributor")]
    class ContributorModel
    {
        [Key]
        [Column("address")]
        public string Address { get; set; }
    }
}
