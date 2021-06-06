
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Data.Entity;
using System.Threading;
using Yavsc.Models.Haircut;
using Yavsc.Models.IT.Evolution;
using Yavsc.Models.IT.Fixing;
using Yavsc.Server.Models.EMailing;
using Yavsc.Server.Models.IT.SourceCode;
using Yavsc.Server.Models.IT;
using Yavsc.Models.Streaming;
using Yavsc.Models.Musical;

namespace Yavsc.Models
{
    using Relationship;
    using Forms;
    using Yavsc;
    using Auth;
    using Billing;
    using Musical;
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
    using Calendar;
    using Blog;
    using Yavsc.Abstract.Identity;
    using Yavsc.Server.Models.Blog;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Contact>().HasKey(x => new { x.OwnerId, x.UserId });
            builder.Entity<DeviceDeclaration>().Property(x => x.DeclarationDate).HasDefaultValueSql("LOCALTIMESTAMP");
            builder.Entity<BlogTag>().HasKey(x => new { x.PostId, x.TagId });
            builder.Entity<ApplicationUser>().HasMany<ChatConnection>(c => c.Connections);
            builder.Entity<ApplicationUser>().Property(u => u.Avatar).HasDefaultValue(Constants.DefaultAvatar);
            builder.Entity<ApplicationUser>().Property(u => u.DiskQuota).HasDefaultValue(Constants.DefaultFSQ);
            builder.Entity<UserActivity>().HasKey(u => new { u.DoesCode, u.UserId });
            builder.Entity<Instrumentation>().HasKey(u => new { u.InstrumentId, u.UserId });
            builder.Entity<CircleAuthorizationToBlogPost>().HasKey(a => new { a.CircleId, a.BlogPostId });
            builder.Entity<CircleMember>().HasKey(c => new { c.MemberId, c.CircleId });
            builder.Entity<DimissClicked>().HasKey(c => new { uid = c.UserId, notid = c.NotificationId });
            builder.Entity<HairTaintInstance>().HasKey(ti => new { ti.TaintId, ti.PrestationId });
            builder.Entity<HyperLink>().HasKey(l => new { l.HRef, l.Method });
            builder.Entity<Period>().HasKey(l => new { l.Start, l.End });
            builder.Entity<Models.Cratie.Option>().HasKey(o => new { o.Code, o.CodeScrutin });
            builder.Entity<Notification>().Property(n => n.icon).HasDefaultValue("exclam");
            builder.Entity<ChatRoomAccess>().HasKey(p => new { room = p.ChannelName, user = p.UserId });
            builder.Entity<BlackListed>().HasOne<ApplicationUser>(bl => bl.User);
            builder.Entity<BlackListed>().HasOne<ApplicationUser>(bl => bl.Owner);
            builder.Entity<BlogTrad>().HasKey(tr => new { post = tr.PostId, lang = tr.Lang });
            builder.Entity<InstrumentRating>().HasAlternateKey(i => new { Instrument= i.InstrumentId, owner = i.OwnerId });

            foreach (var et in builder.Model.GetEntityTypes())
            {
                if (et.ClrType.GetInterface("IBaseTrackedEntity") != null)
                    et.FindProperty("DateCreated").IsReadOnlyAfterSave = true;
            }
        }

        // this is not a failback procedure.
       
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!string.IsNullOrWhiteSpace(Startup.ConnectionString))
                {
                    optionsBuilder.UseNpgsql(Startup.ConnectionString);
                    return;
                }

            var appSetup = (string) AppDomain.CurrentDomain.GetData(Constants.YavscConnectionStringEnvName);

            
            if (!string.IsNullOrWhiteSpace(appSetup)) 
            {
                optionsBuilder.UseNpgsql(appSetup);
                return;
            }
            var envSetup = Environment.GetEnvironmentVariable(Constants.YavscConnectionStringEnvName);
            if (envSetup!=null) 
                optionsBuilder.UseNpgsql(envSetup);
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

        /// <summary>
        /// Google Calendar offline
        /// open auth tokens
        /// </summary>
        /// <returns>tokens</returns>
        public DbSet<OAuth2Tokens> OAuth2Tokens { get; set; }

        /// <summary>
        /// References all declared external NativeConfidential devices
        /// </summary>
        /// <returns></returns>
        public DbSet<DeviceDeclaration> DeviceDeclaration { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Product> Products { get; set; }

        public DbSet<ExceptionSIREN> ExceptionsSIREN { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<BlogTag> TagsDomain { get; set; }

        public DbSet<EstimateTemplate> EstimateTemplates { get; set; }

        public DbSet<Contact> Contact { get; set; }

        public DbSet<ClientProviderInfo> ClientProviderInfo { get; set; }

        public DbSet<BlackListed> BlackListed { get; set; }

        public DbSet<MusicalPreference> MusicalPreference { get; set; }

        public DbSet<MusicalTendency> MusicalTendency { get; set; }

        public DbSet<Instrument> Instrument { get; set; }

        [ActivitySettings]
        public DbSet<DjSettings> DjSettings { get; set; }

        [ActivitySettings]
        public DbSet<Instrumentation> Instrumentation { get; set; }

        [ActivitySettings]
        public DbSet<FormationSettings> FormationSettings { get; set; }

        [ActivitySettings]
        public DbSet<GeneralSettings> GeneralSettings { get; set; }
        public DbSet<CoWorking> CoWorking { get; set; }

        private void AddTimestamps(string currentUsername)
        {
            var entities =
            ChangeTracker.Entries()
            .Where(x => x.Entity.GetType().GetInterface("IBaseTrackedEntity") != null
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
        public int SaveChanges(string userId)
        {
            AddTimestamps(userId);
            return base.SaveChanges();
        }

        public async Task<int> SaveChangesAsync(string userId, CancellationToken ctoken = default(CancellationToken))
        {
            AddTimestamps(userId);
            return await base.SaveChangesAsync(ctoken);
        }

        public DbSet<Circle> Circle { get; set; }

        public DbSet<CircleAuthorizationToBlogPost> CircleAuthorizationToBlogPost { get; set; }

        public DbSet<CommandForm> CommandForm { get; set; }

        public DbSet<Form> Form { get; set; }

        public DbSet<Ban> Ban { get; set; }

        public DbSet<HairTaint> HairTaint { get; set; }

        public DbSet<Color> Color { get; set; }

        public DbSet<Notification> Notification { get; set; }

        public DbSet<DimissClicked> DimissClicked { get; set; }


        [ActivitySettings]
        public DbSet<BrusherProfile> BrusherProfile { get; set; }

        public DbSet<BankIdentity> BankIdentity { get; set; }

        public DbSet<PayPalPayment> PayPalPayment { get; set; }

        public DbSet<HyperLink> HyperLink { get; set; }

        public DbSet<Period> Period { get; set; }

        public DbSet<BlogTag> BlogTag { get; set; }

        public DbSet<ApplicationUser> ApplicationUser { get; set; }

        public DbSet<Feature> Feature { get; set; }

        public DbSet<Bug> Bug { get; set; }

        public DbSet<Comment> Comment { get; set; }

        public DbSet<Announce> Announce { get; set; }
        
        // TODO remove and opt for for memory only storing,
        // as long as it must be set empty each time the service is restarted,
        // and that chatting should be kept as must as possible independent from db context
        public DbSet<ChatConnection> ChatConnection { get; set; }

        public DbSet<ChatRoom> ChatRoom { get; set; }

        public DbSet<MailingTemplate> MailingTemplate { get; set; }

        public DbSet<GitRepositoryReference> GitRepositoryReference { get; set; }

        public DbSet<Project> Project { get; set; }
        
        public DbSet<BlogTrad> BlogTrad { get; set; }

        [Obsolete("use signaled flows")]
        public DbSet<LiveFlow> LiveFlow { get; set; }

        public DbSet<ChatRoomAccess> ChatRoomAccess { get; set; }

        public DbSet<InstrumentRating> InstrumentRating { get; set; }

    }
}
