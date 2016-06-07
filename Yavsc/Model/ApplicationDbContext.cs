
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Yavsc.Models.Booking;

namespace Yavsc.Models
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationStore
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Contact>().HasKey(x => new { x.OwnerId, x.UserId });
            builder.Entity<BookQuery>().Property(x=>x.CreationDate).HasDefaultValueSql("LOCALTIMESTAMP");
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Startup.ConnectionString);
        }

        public DbSet<Application> Applications { get; set; }
        /// <summary>
        /// Activities referenced on this site
        /// </summary>
        /// <returns></returns>
        public DbSet<Activity> Activities { get; set; }

        /// <summary>
        /// Users posts
        /// </summary>
        /// <returns></returns>
        public DbSet<Blog> Blogspot { get; set; }

        /// <summary>
        /// Skills propulsed by this site
        /// </summary>
        /// <returns></returns>
        public DbSet<Skill> SiteSkills { get; set; }

        /// <summary>
        /// Circle members
        /// </summary>
        /// <returns></returns>
        public DbSet<CircleMember> CircleMembers { get; set; }

        /// <summary>
        /// Commands, from an user, to a performer
        /// (A performer is an user who's actived a main activity
        /// on his profile).
        /// </summary>
        /// <returns></returns>
        public DbSet<Command> Commands { get; set; }
        /// <summary>
        /// Special commands, talking about
        /// a given place and date.
        /// </summary>
        /// <returns></returns>
        public DbSet<Booking.BookQuery> BookQueries { get; set; }
        public DbSet<PerformerProfile> Performers { get; set; }
        public DbSet<Estimate> Estimates { get; set; }
        public DbSet<AccountBalance> BankStatus { get; set; }
        public DbSet<BalanceImpact> BankBook { get; set; }
        public DbSet<Location> Map { get; set; }

        /// <summary>
        /// Google Calendar offline
        /// open auth tokens
        /// </summary>
        /// <returns>tokens</returns>
        public DbSet<OAuth2Tokens> Tokens { get; set; }
        
        /// <summary>
        /// References all declared external GCM devices
        /// </summary>
        /// <returns></returns>
        public DbSet<GoogleCloudMobileDeclaration> GCMDevices { get; set; }

        public Task ClearTokensAsync()
        {
            Tokens.RemoveRange(this.Tokens);
            SaveChanges();
            return Task.FromResult(0);
        }

        public Task DeleteTokensAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("email MUST have a value");
            }

            var item = this.Tokens.FirstOrDefault(x => x.UserId == email);
            if (item != null)
            {
                Tokens.Remove(item);
                SaveChanges();
            }
            return Task.FromResult(0);
        }

        public Task<OAuth2Tokens> GetTokensAsync(string googleUserId)
        {
            if (string.IsNullOrEmpty(googleUserId))
            {
                throw new ArgumentException("email MUST have a value");
            }

            using (var context = new ApplicationDbContext())
            {
                var item = this.Tokens.FirstOrDefault(x => x.UserId == googleUserId);
                return Task.FromResult(item);
            }
        }

        public Task StoreTokenAsync(string googleUserId, OAuthTokenResponse value)
        {
            if (string.IsNullOrEmpty(googleUserId))
            {
                throw new ArgumentException("googleUserId MUST have a value");
            }

            var item = this.Tokens.SingleOrDefaultAsync(x => x.UserId == googleUserId).Result;
            if (item == null)
            {
                Tokens.Add(new OAuth2Tokens
                {
                    TokenType = "Bearer", // FIXME why value.TokenType would be null?
                    AccessToken = value.AccessToken,
                    RefreshToken = value.RefreshToken,
                    Expiration = DateTime.Now.AddSeconds(int.Parse(value.ExpiresIn)),
                    UserId = googleUserId
                });
            }
            else
            {
                item.AccessToken = value.AccessToken;
                item.Expiration = DateTime.Now.AddMinutes(int.Parse(value.ExpiresIn));
                if (value.RefreshToken != null)
                    item.RefreshToken = value.RefreshToken;
                Tokens.Update(item);
            }
            SaveChanges();
            return Task.FromResult(0);
        }

        IApplication IApplicationStore.FindApplication(string clientId)
        {
            return Applications.FirstOrDefault(
                app=>app.ApplicationID == clientId);
        }
    }
}
