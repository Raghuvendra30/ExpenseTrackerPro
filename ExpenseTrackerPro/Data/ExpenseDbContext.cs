using Microsoft.EntityFrameworkCore;
using ExpenseTrackerPro.Models;

namespace ExpenseTrackerPro.Data
{
    public class ExpenseDbContext : DbContext
    {
        public DbSet<Expense> Expenses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=expenses.db");
        }
    }
}