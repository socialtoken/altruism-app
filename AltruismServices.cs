using System;
using System.Collections.Generic;
using System.Linq;

namespace Blockchain.Altruism
{
    class AltruismServices
    {
        public bool SetManaged(string tx, bool isAltruist, bool isHacked, string altruistTx = null)
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                TransactionModel model = ctx.Transactions.Find(tx);
                if (model != null)
                {
                    model.altruismMode = isAltruist;
                    model.hackedMode = isHacked;
                    model.managed = true;
                    model.altruistTx = altruistTx;
                    ctx.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public TransactionModel GetLastTransaction()
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                TransactionModel model = ctx.Transactions.OrderByDescending(x => x.timeStamp).FirstOrDefault();
                return model;
            }
        }

        public TransactionModel GetTransaction(string tx)
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                return ctx.Transactions.Find(tx);
            }
        }

        public bool AddTransaction(TransactionModel tx)
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                TransactionModel modelExists = ctx.Transactions.Find(tx.hash);
                if (modelExists != null)
                    return false;
                ctx.Transactions.Add(tx);
                ctx.SaveChanges();
            }
            return true;
        }

        public IEnumerable<TransactionModel> GetUnmanagedTransactions()
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                IEnumerable<TransactionModel> result = ctx.Transactions.Where(x => !x.managed).OrderBy(x => x.timeStamp).ToList();
                if (result == null)
                    return new List<TransactionModel>();
                return result;
            }
        }

        public bool AddContributor(string address)
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                ContributorModel modelExists = ctx.Contributors.Find(address);
                if (modelExists != null)
                    return false;
                modelExists = new ContributorModel();
                modelExists.Address = address;
                ctx.Contributors.Add(modelExists);
                ctx.SaveChanges();
            }
            return true;
        }

        public List<string> GetContributors()
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                List<string> result = ctx.Contributors.Select(x => x.Address).ToList();
                return result;
            }
        }

        public int GetLastBlock()
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                PropertyModel model = ctx.Properties.Where(x => x.Name == "lastBlock").FirstOrDefault();
                if (model == null)
                    return 0;
                int block;
                int.TryParse(model.Value, out block);
                return block;
            }
        }

        public bool UpdateProperty(string name, string value)
        {
            using (AltruismContext ctx = new AltruismContext())
            {
                PropertyModel model = ctx.Properties.Where(x => x.Name == name).FirstOrDefault();
                if (model == null)
                {
                    model = new PropertyModel();
                    model.Name = name;
                    model.Value = value;
                    ctx.Properties.Add(model);
                }
                else
                {
                    model.Value = value;
                }
                ctx.SaveChanges();
            }
            return true;
        }
    }
}
