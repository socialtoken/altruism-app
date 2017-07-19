using Microsoft.EntityFrameworkCore;

namespace Blockchain.Altruism
{
    class AltruismContext : DbContext
    {
        public DbSet<TransactionModel> Transactions { get; set; }
        public DbSet<PropertyModel> Properties { get; set; }
        public DbSet<ContributorModel> Contributors { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"");
        }
    }
}
