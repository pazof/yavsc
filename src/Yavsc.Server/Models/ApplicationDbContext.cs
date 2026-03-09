using IdentityServer8.EntityFramework.Entities;
using IdentityServer8.EntityFramework.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Yavsc.Models
{
    using Abstract.Identity;
    using Abstract.Models.Messaging;
    using Access;
    using Attributes;
    using Auth;
    using Bank;
    using Billing;
    using Blog;
    using Chat;
    using Drawing;
    using Forms;
    using Haircut;
    using Identity;
    using IT.Evolution;
    using IT.Fixing;
    using Market;
    using Messaging;
  using Microsoft.AspNetCore.Http;
  using Musical;
    using Musical.Profiles;
    using Payment;
    using Relationship;
    using Server.Models.Calendar;
    using Server.Models.EMailing;
    using Server.Models.IT;
    using Server.Models.IT.SourceCode;
    using Streaming;
    using Workflow;
    using Workflow.Profiles;

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>,
    IConfigurationDbContext, IPersistedGrantDbContext
    {
        public ApplicationDbContext()
        {
        }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.UseIdentityByDefaultColumns();

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Contact>().HasKey(x => new { x.OwnerId, x.UserId });
            builder.Entity<DeviceDeclaration>().Property(x => x.DeclarationDate).HasDefaultValueSql("LOCALTIMESTAMP");
            builder.Entity<BlogTag>().HasKey(x => new { x.PostId, x.TagId });

            builder.Entity<ApplicationUser>().Property(u => u.FullName).IsRequired(false);
            builder.Entity<ApplicationUser>().Property(u => u.DedicatedGoogleCalendar).IsRequired(false);
            builder.Entity<ApplicationUser>().HasMany<ChatConnection>(c => c.Connections);
            builder.Entity<ApplicationUser>().Property(u => u.Avatar).HasDefaultValue(Constants.DefaultAvatar);
            builder.Entity<ApplicationUser>().Property(u => u.DiskQuota).HasDefaultValue(Constants.DefaultFSQ);
            builder.Entity<ApplicationUser>().HasAlternateKey(u => u.Email);
            builder.Entity<BlackListed>().HasOne<ApplicationUser>(bl => bl.User);
            builder.Entity<BlackListed>().HasOne<ApplicationUser>(bl => bl.Owner);
            builder.Entity<UserActivity>().HasKey(u => new { u.DoesCode, u.UserId });
            builder.Entity<Instrumentation>().HasKey(u => new { u.InstrumentId, u.UserId });
            builder.Entity<CircleAuthorizationToBlogPost>().HasKey(a => new { a.CircleId, a.BlogPostId });
            builder.Entity<CircleMember>().HasKey(c => new { c.MemberId, c.CircleId });
            builder.Entity<DismissClicked>().HasKey(c => new { uid = c.UserId, notid = c.NotificationId });
            builder.Entity<HairTaintInstance>().HasKey(ti => new { ti.TaintId, ti.PrestationId });
            builder.Entity<HyperLink>().HasKey(l => new { l.HRef, l.Method });
            builder.Entity<Period>().HasKey(l => new { l.Start, l.End });
            builder.Entity<Cratie.Option>().HasKey(o => new { o.Code, o.CodeScrutin });
            builder.Entity<Notification>().Property(n => n.icon).HasDefaultValue("exclam");
            builder.Entity<ChatRoomAccess>().HasKey(p => new { room = p.ChannelName, user = p.UserId });
            builder.Entity<InstrumentRating>().HasAlternateKey(i => new { Instrument = i.InstrumentId, owner = i.OwnerId })
            ;

            builder.Entity<Activity>().Property(a => a.ParentCode).IsRequired(false);
            
            builder.Entity<Client>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientSecret>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientScope>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientIdPRestriction>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientProperty>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientPostLogoutRedirectUri>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientRedirectUri>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientCorsOrigin>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientGrantType>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ClientClaim>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ApiResource>().Property("Id").UseIdentityAlwaysColumn();
            builder.Entity<ApiScope>().Property("Id").UseIdentityAlwaysColumn();

            builder.Entity<DeviceFlowCodes>().HasKey(e => new { e.UserCode, e.DeviceCode });
            builder.Entity<PersistedGrant>().HasKey(e => e.Key);

            //    builder.Entity<IdentityUserLogin<String>>().HasKey(i=> new { i.LoginProvider, i.UserId, i.ProviderKey });
            builder.Entity<ClientSecret>().HasOne<Client>().WithMany(e => e.ClientSecrets).HasForeignKey(e => e.ClientId);
            builder.Entity<ClientScope>().HasOne<Client>().WithMany(e => e.AllowedScopes).HasForeignKey(e => e.ClientId);
            builder.Entity<ClientIdPRestriction>().HasOne<Client>().WithMany(e => e.IdentityProviderRestrictions).HasForeignKey(e => e.ClientId);
            builder.Entity<ClientProperty>().HasOne<Client>().WithMany(e => e.Properties).HasForeignKey(e => e.ClientId);
            builder.Entity<ClientPostLogoutRedirectUri>().HasOne<Client>().WithMany(e => e.PostLogoutRedirectUris).HasForeignKey(e => e.ClientId);
            builder.Entity<ClientRedirectUri>().HasOne<Client>().WithMany(e => e.RedirectUris).HasForeignKey(e => e.ClientId);
            builder.Entity<ClientCorsOrigin>().HasOne<Client>().WithMany(e => e.AllowedCorsOrigins).HasForeignKey(e => e.ClientId);
            builder.Entity<ClientGrantType>().HasOne<Client>().WithMany(e => e.AllowedGrantTypes).HasForeignKey(e => e.ClientId);
            builder.Entity<ApiResourceSecret>().HasOne<ApiResource>().WithMany(e => e.Secrets).HasForeignKey(e => e.ApiResourceId);
            builder.Entity<ApiResourceScope>().HasOne<ApiResource>().WithMany(e => e.Scopes).HasForeignKey(e => e.ApiResourceId);
            builder.Entity<ApiResourceClaim>().HasOne<ApiResource>().WithMany(e => e.UserClaims).HasForeignKey(e => e.ApiResourceId);
            builder.Entity<ApiResourceProperty>().HasOne<ApiResource>().WithMany(e => e.Properties).HasForeignKey(e => e.ApiResourceId);
            builder.Entity<ApiScopeClaim>().HasOne<ApiScope>().WithMany(e => e.UserClaims).HasForeignKey(e => e.ScopeId);
            builder.Entity<ApiScopeProperty>().HasOne<ApiScope>().WithMany(e => e.Properties).HasForeignKey(e => e.ScopeId);

            foreach (var et in builder.Model.GetEntityTypes())
            {
                if (et.ClrType.GetInterface("IBaseTrackedEntity") != null)
                    et.FindProperty("DateCreated").SetAfterSaveBehavior(Microsoft.EntityFrameworkCore.Metadata.PropertySaveBehavior.Ignore);
            }
        }

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
        public DbSet<BlogPost> BlogSpot { get; set; }

        /// <summary>
        /// Skills powered by this site
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
        public DbSet<BalanceImpact> BalanceImpact { get; set; }

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

        private void AddTimestamps(string userId)
        {
            var entities =
            ChangeTracker.Entries()
            .Where(x => x.Entity.GetType().GetInterface(nameof(ITrackedEntity)) != null
            && (x.State == EntityState.Added || x.State == EntityState.Modified));


            foreach (var entity in entities)
            {
                if (entity.State == EntityState.Added)
                {
                    ((ITrackedEntity)entity.Entity).DateCreated = DateTime.Now;
                    ((ITrackedEntity)entity.Entity).UserCreated = userId;
                }

                ((ITrackedEntity)entity.Entity).DateModified = DateTime.Now;
                ((ITrackedEntity)entity.Entity).UserModified = userId;
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

    public Task<int> SaveChangesAsync()
    {
        return base.SaveChangesAsync();
    }

    public DbSet<Circle> Circle { get; set; }

        public DbSet<CircleAuthorizationToBlogPost> CircleAuthorizationToBlogPost { get; set; }

        public DbSet<CommandForm> CommandForm { get; set; }

        public DbSet<Form> Form { get; set; }

        public DbSet<Ban> Ban { get; set; }

        public DbSet<HairTaint> HairTaint { get; set; }

        public DbSet<Color> Color { get; set; }

        public DbSet<Notification> Notification { get; set; }

        public DbSet<DismissClicked> DismissClicked { get; set; }


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

        [Obsolete("use signaled flows")]
        public DbSet<LiveFlow> LiveFlow { get; set; }

        public DbSet<ChatRoomAccess> ChatRoomAccess { get; set; }

        public DbSet<InstrumentRating> InstrumentRating { get; set; }


        public DbSet<BlogSpotPublication> blogSpotPublications { get; set; }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientIdPRestriction> ClientIdPRestrictions { get; set; }
        public DbSet<ClientProperty> ClientProperties { get; set; }

        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
        public DbSet<ClientSecret> ClientSecrets { get; set; }
        public DbSet<ClientScope> ClientScopes { get; set; }
        public DbSet<ClientGrantType> ClientGrantTypes { get; set; }
        public DbSet<ClientClaim> ClientClaims { get; set; }

        public DbSet<ClientRedirectUri> ClientRedirectUris { get; set; }


        public DbSet<ClientPostLogoutRedirectUri> ClientPostLogoutRedirectUris { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<IdentityResourceClaim> IdentityResourceClaims { get; set; }
        public DbSet<IdentityResourceProperty> IdentityResourceProperties { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<ApiResourceSecret> ApiResourceSecrets { get; set; }
        public DbSet<ApiResourceScope> ApiResourceScopes { get; set; }
        public DbSet<ApiResourceClaim> ApiResourceClaims { get; set; }
        public DbSet<ApiResourceProperty> ApiResourceProperties { get; set; }
        public DbSet<ApiScope> ApiScopes { get; set; }
        public DbSet<ApiScopeClaim> ApiScopeClaims { get; set; }
        public DbSet<ApiScopeProperty> ApiScopeProperties { get; set; }
        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }




    }
}
