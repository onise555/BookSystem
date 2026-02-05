using BookSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace BookSystem.Data
{
    public class DataContext:DbContext
    {

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connString = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (string.IsNullOrEmpty(connString))
            {
                // 1. აქ ჩასვი ის DATABASE_PUBLIC_URL, რომელიც დააკოპირე
                // 2. ბოლოში მიაწერე ?sslmode=require
                optionsBuilder.UseNpgsql("postgresql://postgres:sZMHaECUNouZutVpeolnrgQwlJIKzRHB@yamabiko.proxy.rlwy.net:33961/railway");
            }
            else
            {
                // Railway-ზე გაშვებისას გამოიყენებს შიდა DATABASE_URL-ს
                optionsBuilder.UseNpgsql(connString);
            }
        }

        public DbSet<Folder> Folders { get; set; }  
        public DbSet<Book> books { get; set; }  

    }
}
