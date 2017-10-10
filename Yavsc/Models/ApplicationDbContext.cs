
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using System.Threading;
using Yavsc.Models.Haircut;
using Yavsc.Models.IT.Maintaining;
using Yavsc.Models.IT.Fixing;

namespace Yavsc.Models
{
    using Relationship;
    using Forms;
    using Yavsc;
    using Auth;
    using Billing;
    using Musical;
    using OAuth;
    using Workflow;
    using Identity;
    using Market;
    using Chat;
    using Messaging;
    using Access;
    using Musical.Profiles;
    using Workflow.Profiles;
    using Drawing;
    using Attributes;
    using Bank;
    using Payment;
    using Yavsc.Models.Calendar;
    using Blog;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Relationship.Contact>().HasKey(x => new { x.OwnerId, x.UserId });
            builder.Entity<GoogleCloudMobileDeclaration>().Property(x=>x.DeclarationDate).HasDefaultValueSql("LOCALTIMESTAMP");
            builder.Entity<BlogTag>().HasKey(x=>new { x.PostId, x.TagId});
            builder.Entity<ApplicationUser>().HasMany<Connection>( c=>c.Connections );
            builder.Entity<ApplicationUser>().Property(u=>u.Avatar).HasDefaultValue(Constants.DefaultAvatar);
            builder.Entity<ApplicationUser>().Property(u=>u.DiskQuota).HasDefaultValue(Constants.DefaultFSQ);
            builder.Entity<UserActivity>().HasKey(u=> new { u.DoesCode, u.UserId});
            builder.Entity<Instrumentation>().HasKey(u=> new { u.InstrumentId, u.UserId});
            builder.Entity<CircleAuthorizationToBlogPost>().HasKey(a=> new { a.CircleId, a.BlogPostId});
            builder.Entity<CircleMember>().HasKey(c=> new { MemberId = c.MemberId, CircleId = c.CircleId });
            builder.Entity<DimissClicked>().HasKey(c=>new { uid = c.UserId, notid = c.NotificationId});
            builder.Entity<HairTaintInstance>().HasKey(ti=>new { ti.TaintId, ti.PrestationId } );
            builder.Entity<HyperLink>().HasKey(l=>new { l.HRef, l.Method });
            builder.Entity<Period>().HasKey(l=>new { l.Start, l.End });
            builder.Entity<Models.Cratie.Option>().HasKey( o => new { o.Code, o.CodeScrutin });
            builder.Entity<Notification>().Property(n=> n.icon).HasDefaultValue("exclam");
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
        public DbSet<Blog.BlogPost> Blogspot { get; set; }

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
        /// Special commands, talking about
        /// a given place and date.
        /// </summary>
        public DbSet<RdvQuery> RdvQueries { get; set; }

        public DbSet<HairCutQuery> HairCutQueries { get; set; }
        public DbSet<HairPrestation> HairPrestation { get; set; }

        public DbSet<HairMultiCutQuery> HairMultiCutQueries { get; set; }
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
        

        public Task<OAuth2Tokens> GetTokensAsync(string googleUserId)
        {
            if (string.IsNullOrEmpty(googleUserId))
            {
                throw new ArgumentException("email MUST have a value");
            }

            using (var context = new ApplicationDbContext())
            {
                var item = this.Tokens.FirstOrDefault(x => x.UserId == googleUserId);
                // TODO Refresh token
                
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
            SaveChanges(googleUserId);
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

        public DbSet<BlogTag> TagsDomain { get; set; }

        public DbSet<EstimateTemplate> EstimateTemplates { get; set; }

        public DbSet<Relationship.Contact> Contacts { get; set; }

        public DbSet<ClientProviderInfo> ClientProviderInfo { get; set; }

        public DbSet<Connection> Connections { get; set; }

        public DbSet<BlackListed> BlackListed { get; set; }

        public DbSet<MusicalPreference> MusicalPreferences { get; set; }

        public DbSet<MusicalTendency> MusicalTendency { get; set; }

        public DbSet<LocationType> LocationType { get; set; }

        public DbSet<Instrument> Instrument { get; set; }

        [ActivitySettings]
        public DbSet<DjSettings> DjSettings { get; set; }

        [ActivitySettings]
        public DbSet<Instrumentation> Instrumentation { get; set; }

        [ActivitySettings]
        public DbSet<FormationSettings> FormationSettings { get; set; }

        [ActivitySettings]
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        public DbSet<CoWorking> WorkflowProviders { get; set; }

        private void AddTimestamps(string currentUsername)
    {
        var entities =
        ChangeTracker.Entries()
        .Where(x => x.Entity.GetType().GetInterface("IBaseTrackedEntity")!=null
        && (x.State == EntityState.Added || x.State == EntityState.Modified));


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
        public int SaveChanges(string userId) {
            AddTimestamps(userId);
            return base.SaveChanges();
        }

        public  async Task<int> SaveChangesAsync(string userId, CancellationToken ctoken = default(CancellationToken)) {
            AddTimestamps(userId);
            return await base.SaveChangesAsync();
        }

         public DbSet<Circle> Circle { get; set; }

         public DbSet<CircleAuthorizationToBlogPost> BlogACL { get; set; }

         public DbSet<CommandForm> CommandForm { get; set; }

         public DbSet<Form> Form { get; set; }

         public DbSet<Ban> Banlist { get ; set; }

         public DbSet<HairTaint> HairTaint { get; set; }

         public DbSet<Color> Color { get; set; }

         public DbSet<Notification> Notification { get; set; }

         public DbSet<DimissClicked> DimissClicked { get; set; }


         [ActivitySettings]
         public DbSet<BrusherProfile> BrusherProfile { get; set; }

         public DbSet<BankIdentity> BankIdentity { get; set; }

         public DbSet<PayPalPayment> PayPalPayments { get; set; }

         public DbSet<HyperLink> Links { get; set; }

         public DbSet<Period> Period { get; set; }

         public DbSet<BlogTag> BlogTags { get; set; }

         public DbSet<ApplicationUser> ApplicationUser { get; set; }

         public DbSet<Feature> Feature { get; set; }

         public DbSet<Bug> Bug { get; set; }

         public DbSet<Comment> Comment { get; set; }

         public DbSet<Announce> Announce { get; set; }
    }
}
