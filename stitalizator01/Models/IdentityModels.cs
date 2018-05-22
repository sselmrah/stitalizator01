using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace stitalizator01.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        private string _telegramUserName;
        private long _telegramChatId;
        private int _telegramBetId;

        public string TelegramUserName { get => _telegramUserName; set => _telegramUserName = value; }
        public long TelegramChatId { get => _telegramChatId; set => _telegramChatId = value; }
        public int TelegramBetId { get => _telegramBetId; set => _telegramBetId = value; }


        
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        public DbSet<Program> Programs { get; set; }
        public DbSet<Bet> Bets { get; set; }
        public DbSet<Period> Periods { get; set; }
        public DbSet<ConversationStarter> CSs { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public System.Data.Entity.DbSet<stitalizator01.Models.Channel> Channels { get; set; }

        public System.Data.Entity.DbSet<stitalizator01.Models.CatalogueEntry> CatalogueEntries { get; set; }
        public System.Data.Entity.DbSet<stitalizator01.Models.Category> Categories { get; set; }

        //public System.Data.Entity.DbSet<stitalizator01.Models.ApplicationUser> ApplicationUsers { get; set; }

        //public System.Data.Entity.DbSet<stitalizator01.Models.ApplicationUser> ApplicationUsers { get; set; }
    }
}