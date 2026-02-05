using BookSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookSystem.Data
{
    public class DataContext:DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Books;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False");
        }
       
        public DbSet<Folder> Folders { get; set; }  
        public DbSet<Book> books { get; set; }  

    }
}
