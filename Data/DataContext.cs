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
                // ლოკალური ტესტირებისთვის გამოიყენეთ სტანდარტული ფორმატი + SSL პარამეტრები
                optionsBuilder.UseNpgsql("Host=yamabiko.proxy.rlwy.net;Port=33961;Database=railway;Username=postgres;Password=sZMHaECUNouZutVpeolnrgQwlJIKzRHB;SSL Mode=Require;Trust Server Certificate=true");
            }
            else
            {
                // Railway-ზე გაშვებისას გამოიყენებს გარემოს ცვლადს
                optionsBuilder.UseNpgsql(connString);
            }
        }

        public DbSet<Folder> Folders { get; set; }  
        public DbSet<Book> books { get; set; }  

    }
}
