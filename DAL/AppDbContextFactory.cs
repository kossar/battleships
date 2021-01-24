

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DAL
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;
                                Database=BattleShips;
                                Trusted_Connection=True;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }

    }
}