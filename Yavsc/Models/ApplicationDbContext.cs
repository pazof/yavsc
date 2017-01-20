
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using Yavsc.Models.Relationship;

namespace Yavsc.Models
{

    using Auth;
    using Billing;
    using Booking;
    using OAuth;
    using Workflow;
    using Identity;
    using Market;
    using Chat;
    using Messaging;
    using Access;
    using Yavsc.Models.Booking.Profiles;
    using System.Web;
    using System.Threading;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Contact>().HasKey(x => new { x.OwnerId, x.UserId });
            builder.Entity<GoogleCloudMobileDeclaration>().Property(x=>x.DeclarationDate).HasDefaultValueSql("LOCALTIMESTAMP");
            builder.Entity<PostTag>().HasKey(x=>new { x.PostId, x.TagId});
            builder.Entity<ApplicationUser>().HasMany<Connection>( c=>c.Connections );
            builder.Entity<UserActivity>().HasKey(u=> new { u.DoesCode, u.UserId});
            builder.Entity<Instrumentation>().HasKey(u=> new { u.InstrumentId, u.UserId});
            builder.Entity<CircleAuthorizationToBlogPost>().HasKey(a=> new { a.CircleId, a.BlogPostId});
            foreach (var et in builder.Model.GetEntityTypes()) {
                if (et.ClrType.GetInterface("IBaseTrackedEntity")!=null)
                et.FindProperty("DateCreated").IsReadOnlyAfterSave = true;
            }
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Startup.ConnectionString);
        }

        public DbSet<Client> Applications { get; set; }
        
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        /// <summary>
        /// Activities referenced on this site
        /// </summary>
        /// <returns></returns>
        public DbSet<Activity> Activities { get; set; }
        public DbSet<UserActivity> UserActivities { get; set; }
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
        public DbSet<BookQuery> Commands { get; set; }
        /// <summary>
        /// Special commands, talking about
        /// a given place and date.
        /// </summary>
        /// <returns></returns>
        public DbSet<BookQuery> BookQueries { get; set; }
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

        public DbSet<Service> Services { get; set; }
        public DbSet<Product> Products { get; set; }
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

        Client FindApplication(string clientId)
        {
            return Applications.FirstOrDefault(
                app=>app.Id == clientId);
        }

        public DbSet<ExceptionSIREN> ExceptionsSIREN { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<PostTag> TagsDomain { get; set; }

        public DbSet<EstimateTemplate> EstimateTemplates { get; set; }

        public DbSet<Contact> Contacts { get; set; }

        public DbSet<ClientProviderInfo> ClientProviderInfo { get; set; }

        public DbSet<Connection> Connections { get; set; }

        public DbSet<BlackListed> BlackListed { get; set; }

        public DbSet<MusicalPreference> MusicalPreferences { get; set; }

        public DbSet<MusicalTendency> MusicalTendency { get; set; }

        public DbSet<LocationType> LocationType { get; set; }

        public DbSet<Instrument> Instrument { get; set; }        
        public DbSet<DjSettings> DjSettings { get; set; }
        public DbSet<Instrumentation> Instrumentation { get; set; }
        public DbSet<FormationSettings> FormationSettings { get; set; }
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        public DbSet<CoWorking> WorkflowProviders { get; set; }

        private void AddTimestamps()
    {
        var entities = ChangeTracker.Entries().Where(x => x.Entity.GetType().GetInterface("IBaseTrackedEntity")!=null && (x.State == EntityState.Added || x.State == EntityState.Modified));

        var currentUsername = !string.IsNullOrEmpty(System.Web.HttpContext.Current?.User?.Identity?.Name)
            ? HttpContext.Current.User.Identity.Name
            : "Anonymous";

        foreach (var entity in entities)
        {
            if (entity.State == EntityState.Added)
            {
                ((IBaseTrackedEntity)entity.Entity).DateCreated = DateTime.Now;
                ((IBaseTrackedEntity)entity.Entity).UserCreated = currentUsername;
            }

            ((IBaseTrackedEntity)entity.Entity).DateModified = DateTime.Now;
            ((IBaseTrackedEntity)entity.Entity).UserModified = currentUsername;
        }
    }
        

        override public int SaveChanges() {
            AddTimestamps();
            return base.SaveChanges();
        }
        
         public override async Task<int> SaveChangesAsync(CancellationToken ctoken = default(CancellationToken)) {
            AddTimestamps();
            return await base.SaveChangesAsync();
        }
        
         public DbSet<Circle> Circle { get; set; }

         public DbSet<CircleAuthorizationToBlogPost> BlogACL { get; set; }

    }
}
