using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Migrations;
using Yavsc.Models;

namespace Yavsc.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20180625113528_Git")]
    partial class Git
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0-rc1-16348");

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasAnnotation("Relational:Name", "RoleNameIndex");

                    b.HasAnnotation("Relational:TableName", "AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:TableName", "AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasAnnotation("Relational:TableName", "AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasAnnotation("Relational:TableName", "AspNetUserRoles");
                });

            modelBuilder.Entity("Yavsc.Models.Access.Ban", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Access.BlackListed", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("OwnerId")
                        .IsRequired();

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Access.CircleAuthorizationToBlogPost", b =>
                {
                    b.Property<long>("CircleId");

                    b.Property<long>("BlogPostId");

                    b.HasKey("CircleId", "BlogPostId");
                });

            modelBuilder.Entity("Yavsc.Models.AccountBalance", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<long>("ContactCredits");

                    b.Property<decimal>("Credits");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<bool>("AllowMonthlyEmail");

                    b.Property<string>("Avatar")
                        .HasAnnotation("MaxLength", 512)
                        .HasAnnotation("Relational:DefaultValue", "/images/Users/icon_user.png")
                        .HasAnnotation("Relational:DefaultValueType", "System.String");

                    b.Property<long?>("BankInfoId");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("DedicatedGoogleCalendar")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<long>("DiskQuota")
                        .HasAnnotation("Relational:DefaultValue", "524288000")
                        .HasAnnotation("Relational:DefaultValueType", "System.Int64");

                    b.Property<long>("DiskUsage");

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FullName")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<long>("MaxFileSize");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<long?>("PostalAddressId");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasAnnotation("Relational:Name", "EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .HasAnnotation("Relational:Name", "UserNameIndex");

                    b.HasAnnotation("Relational:TableName", "AspNetUsers");
                });

            modelBuilder.Entity("Yavsc.Models.Auth.Client", b =>
                {
                    b.Property<string>("Id");

                    b.Property<bool>("Active");

                    b.Property<string>("DisplayName");

                    b.Property<string>("LogoutRedirectUri")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<string>("RedirectUri");

                    b.Property<int>("RefreshTokenLifeTime");

                    b.Property<string>("Secret");

                    b.Property<int>("Type");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Auth.RefreshToken", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ClientId")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.Property<DateTime>("ExpiresUtc");

                    b.Property<DateTime>("IssuedUtc");

                    b.Property<string>("ProtectedTicket")
                        .IsRequired();

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 50);

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.BalanceImpact", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("BalanceId")
                        .IsRequired();

                    b.Property<DateTime>("ExecDate");

                    b.Property<decimal>("Impact");

                    b.Property<string>("Reason")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Bank.BankIdentity", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AccountNumber")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("BIC")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("BankCode")
                        .HasAnnotation("MaxLength", 5);

                    b.Property<int>("BankedKey");

                    b.Property<string>("IBAN")
                        .HasAnnotation("MaxLength", 33);

                    b.Property<string>("WicketCode")
                        .HasAnnotation("MaxLength", 5);

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.CommandLine", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Count");

                    b.Property<string>("Currency");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 512);

                    b.Property<long>("EstimateId");

                    b.Property<long?>("EstimateTemplateId");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 256);

                    b.Property<decimal>("UnitaryCost");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.Estimate", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AttachedFilesString");

                    b.Property<string>("AttachedGraphicsString");

                    b.Property<string>("ClientId")
                        .IsRequired();

                    b.Property<DateTime>("ClientValidationDate");

                    b.Property<long?>("CommandId");

                    b.Property<string>("CommandType")
                        .IsRequired();

                    b.Property<string>("Description");

                    b.Property<string>("OwnerId");

                    b.Property<DateTime>("ProviderValidationDate");

                    b.Property<string>("Title");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.EstimateTemplate", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("OwnerId")
                        .IsRequired();

                    b.Property<string>("Title");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.ExceptionSIREN", b =>
                {
                    b.Property<string>("SIREN");

                    b.HasKey("SIREN");
                });

            modelBuilder.Entity("Yavsc.Models.Blog.BlogPost", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthorId");

                    b.Property<string>("Content")
                        .HasAnnotation("MaxLength", 56224);

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("Photo")
                        .HasAnnotation("MaxLength", 1024);

                    b.Property<int>("Rate");

                    b.Property<string>("Title")
                        .HasAnnotation("MaxLength", 1024);

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.Property<bool>("Visible");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Blog.BlogTag", b =>
                {
                    b.Property<long>("PostId");

                    b.Property<long>("TagId");

                    b.HasKey("PostId", "TagId");
                });

            modelBuilder.Entity("Yavsc.Models.Blog.Comment", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthorId")
                        .IsRequired();

                    b.Property<string>("Content");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<long?>("ParentId");

                    b.Property<long>("PostId");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.Property<bool>("Visible");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Calendar.Period", b =>
                {
                    b.Property<DateTime>("Start");

                    b.Property<DateTime>("End");

                    b.HasKey("Start", "End");
                });

            modelBuilder.Entity("Yavsc.Models.Calendar.Schedule", b =>
                {
                    b.Property<string>("OwnerId");

                    b.HasKey("OwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.Calendar.ScheduledEvent", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime?>("PeriodEnd");

                    b.Property<DateTime?>("PeriodStart");

                    b.Property<int>("Reccurence");

                    b.Property<string>("ScheduleOwnerId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.ChatConnection", b =>
                {
                    b.Property<string>("ConnectionId");

                    b.Property<string>("ApplicationUserId")
                        .IsRequired();

                    b.Property<bool>("Connected");

                    b.Property<string>("UserAgent");

                    b.HasKey("ConnectionId");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.ChatRoom", b =>
                {
                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("Topic")
                        .HasAnnotation("MaxLength", 1023);

                    b.HasKey("Name");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.ChatRoomPresence", b =>
                {
                    b.Property<string>("ChannelName");

                    b.Property<string>("ChatUserConnectionId");

                    b.Property<int>("Level");

                    b.HasKey("ChannelName", "ChatUserConnectionId");
                });

            modelBuilder.Entity("Yavsc.Models.Cratie.Option", b =>
                {
                    b.Property<string>("Code");

                    b.Property<string>("CodeScrutin");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("Description");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.HasKey("Code", "CodeScrutin");
                });

            modelBuilder.Entity("Yavsc.Models.Drawing.Color", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("Blue");

                    b.Property<byte>("Green");

                    b.Property<string>("Name");

                    b.Property<byte>("Red");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Forms.Form", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("Summary");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.BrusherProfile", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<int>("ActionDistance");

                    b.Property<decimal>("CarePrice");

                    b.Property<decimal>("FlatFeeDiscount");

                    b.Property<decimal>("HalfBalayagePrice");

                    b.Property<decimal>("HalfBrushingPrice");

                    b.Property<decimal>("HalfColorPrice");

                    b.Property<decimal>("HalfDefrisPrice");

                    b.Property<decimal>("HalfFoldingPrice");

                    b.Property<decimal>("HalfMechPrice");

                    b.Property<decimal>("HalfMultiColorPrice");

                    b.Property<decimal>("HalfPermanentPrice");

                    b.Property<decimal>("KidCutPrice");

                    b.Property<decimal>("LongBalayagePrice");

                    b.Property<decimal>("LongBrushingPrice");

                    b.Property<decimal>("LongColorPrice");

                    b.Property<decimal>("LongDefrisPrice");

                    b.Property<decimal>("LongFoldingPrice");

                    b.Property<decimal>("LongMechPrice");

                    b.Property<decimal>("LongMultiColorPrice");

                    b.Property<decimal>("LongPermanentPrice");

                    b.Property<decimal>("ManBrushPrice");

                    b.Property<decimal>("ManCutPrice");

                    b.Property<string>("ScheduleOwnerId");

                    b.Property<decimal>("ShampooPrice");

                    b.Property<decimal>("ShortBalayagePrice");

                    b.Property<decimal>("ShortBrushingPrice");

                    b.Property<decimal>("ShortColorPrice");

                    b.Property<decimal>("ShortDefrisPrice");

                    b.Property<decimal>("ShortFoldingPrice");

                    b.Property<decimal>("ShortMechPrice");

                    b.Property<decimal>("ShortMultiColorPrice");

                    b.Property<decimal>("ShortPermanentPrice");

                    b.Property<decimal>("WomenHalfCutPrice");

                    b.Property<decimal>("WomenLongCutPrice");

                    b.Property<decimal>("WomenShortCutPrice");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairCutQuery", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActivityCode")
                        .IsRequired();

                    b.Property<string>("AdditionalInfo")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<string>("ClientId")
                        .IsRequired();

                    b.Property<bool>("Consent");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<DateTime?>("EventDate");

                    b.Property<long?>("LocationId");

                    b.Property<string>("PaymentId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<long>("PrestationId");

                    b.Property<decimal?>("Previsional");

                    b.Property<bool>("Rejected");

                    b.Property<DateTime>("RejectedAt");

                    b.Property<string>("SelectedProfileUserId");

                    b.Property<int>("Status");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.Property<DateTime?>("ValidationDate");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairMultiCutQuery", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActivityCode")
                        .IsRequired();

                    b.Property<string>("ClientId")
                        .IsRequired();

                    b.Property<bool>("Consent");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<DateTime>("EventDate");

                    b.Property<long?>("LocationId");

                    b.Property<string>("PaymentId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<decimal?>("Previsional");

                    b.Property<bool>("Rejected");

                    b.Property<DateTime>("RejectedAt");

                    b.Property<int>("Status");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.Property<DateTime?>("ValidationDate");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairPrestation", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("Cares");

                    b.Property<bool>("Cut");

                    b.Property<int>("Dressing");

                    b.Property<int>("Gender");

                    b.Property<int>("Length");

                    b.Property<bool>("Shampoo");

                    b.Property<int>("Tech");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairPrestationCollectionItem", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("PrestationId");

                    b.Property<long>("QueryId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairTaint", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Brand");

                    b.Property<long>("ColorId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairTaintInstance", b =>
                {
                    b.Property<long>("TaintId");

                    b.Property<long>("PrestationId");

                    b.HasKey("TaintId", "PrestationId");
                });

            modelBuilder.Entity("Yavsc.Models.Identity.GoogleCloudMobileDeclaration", b =>
                {
                    b.Property<string>("DeviceId");

                    b.Property<DateTime>("DeclarationDate")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Relational:GeneratedValueSql", "LOCALTIMESTAMP");

                    b.Property<string>("DeviceOwnerId");

                    b.Property<string>("GCMRegistrationId")
                        .IsRequired();

                    b.Property<DateTime>("LatestActivityUpdate");

                    b.Property<string>("Model");

                    b.Property<string>("Platform");

                    b.Property<string>("Version");

                    b.HasKey("DeviceId");
                });

            modelBuilder.Entity("Yavsc.Models.IT.Fixing.Bug", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<long?>("FeatureId");

                    b.Property<int>("Status");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.IT.Maintaining.Feature", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("ShortName");

                    b.Property<int>("Status");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Market.Product", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Depth");

                    b.Property<string>("Description");

                    b.Property<decimal>("Height");

                    b.Property<string>("Name");

                    b.Property<decimal?>("Price");

                    b.Property<bool>("Public");

                    b.Property<decimal>("Weight");

                    b.Property<decimal>("Width");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Market.Service", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ContextId");

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.Property<bool>("Public");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.Announce", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<byte>("For");

                    b.Property<string>("Message");

                    b.Property<string>("OwnerId");

                    b.Property<string>("Sender");

                    b.Property<string>("Topic");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.ClientProviderInfo", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("Avatar");

                    b.Property<long>("BillingAddressId");

                    b.Property<string>("EMail");

                    b.Property<string>("Phone");

                    b.Property<string>("UserName");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.DimissClicked", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<long>("NotificationId");

                    b.HasKey("UserId", "NotificationId");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.Notification", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Target");

                    b.Property<string>("body")
                        .IsRequired();

                    b.Property<string>("click_action")
                        .IsRequired();

                    b.Property<string>("color");

                    b.Property<string>("icon")
                        .HasAnnotation("Relational:DefaultValue", "exclam")
                        .HasAnnotation("Relational:DefaultValueType", "System.String");

                    b.Property<string>("sound");

                    b.Property<string>("tag");

                    b.Property<string>("title")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.Instrument", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.MusicalPreference", b =>
                {
                    b.Property<string>("OwnerProfileId");

                    b.Property<string>("DjSettingsUserId");

                    b.Property<string>("GeneralSettingsUserId");

                    b.Property<int>("Rate");

                    b.Property<long>("TendencyId");

                    b.HasKey("OwnerProfileId");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.MusicalTendency", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.Profiles.DjSettings", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("SoundCloudId");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.Profiles.GeneralSettings", b =>
                {
                    b.Property<string>("UserId");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.Profiles.Instrumentation", b =>
                {
                    b.Property<long>("InstrumentId");

                    b.Property<string>("UserId");

                    b.HasKey("InstrumentId", "UserId");
                });

            modelBuilder.Entity("Yavsc.Models.OAuth.OAuth2Tokens", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("AccessToken");

                    b.Property<DateTime>("Expiration");

                    b.Property<string>("ExpiresIn");

                    b.Property<string>("RefreshToken");

                    b.Property<string>("TokenType");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Payment.PayPalPayment", b =>
                {
                    b.Property<string>("CreationToken");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("ExecutorId")
                        .IsRequired();

                    b.Property<string>("OrderReference");

                    b.Property<string>("PaypalPayerId");

                    b.Property<string>("State");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.HasKey("CreationToken");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.Circle", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("Name");

                    b.Property<string>("OwnerId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.CircleMember", b =>
                {
                    b.Property<string>("MemberId");

                    b.Property<long>("CircleId");

                    b.HasKey("MemberId", "CircleId");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.Contact", b =>
                {
                    b.Property<string>("OwnerId");

                    b.Property<string>("UserId");

                    b.Property<string>("ApplicationUserId");

                    b.HasKey("OwnerId", "UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.HyperLink", b =>
                {
                    b.Property<string>("HRef");

                    b.Property<string>("Method");

                    b.Property<string>("BrusherProfileUserId");

                    b.Property<string>("ContentType");

                    b.Property<string>("PayPalPaymentCreationToken");

                    b.Property<string>("Rel");

                    b.HasKey("HRef", "Method");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.Location", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 512);

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.Tag", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Skill", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("Rate");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.Activity", b =>
                {
                    b.Property<string>("Code")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("Description");

                    b.Property<bool>("Hidden");

                    b.Property<string>("ModeratorGroupName");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 512);

                    b.Property<string>("ParentCode")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<string>("Photo");

                    b.Property<int>("Rate");

                    b.Property<string>("SettingsClassName");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.HasKey("Code");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.CommandForm", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActionName");

                    b.Property<string>("ActivityCode")
                        .IsRequired();

                    b.Property<string>("Title");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.CoWorking", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FormationSettingsUserId");

                    b.Property<string>("PerformerId");

                    b.Property<string>("WorkingForId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.PerformerProfile", b =>
                {
                    b.Property<string>("PerformerId");

                    b.Property<bool>("AcceptNotifications");

                    b.Property<bool>("AcceptPublicContact");

                    b.Property<bool>("Active");

                    b.Property<int?>("MaxDailyCost");

                    b.Property<int?>("MinDailyCost");

                    b.Property<long>("OrganizationAddressId");

                    b.Property<int>("Rate");

                    b.Property<string>("SIREN")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 14);

                    b.Property<bool>("UseGeoLocalizationToReduceDistanceWithClients");

                    b.Property<string>("WebSite");

                    b.HasKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.Profiles.FormationSettings", b =>
                {
                    b.Property<string>("UserId");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.RdvQuery", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActivityCode")
                        .IsRequired();

                    b.Property<string>("ClientId")
                        .IsRequired();

                    b.Property<bool>("Consent");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<DateTime>("EventDate");

                    b.Property<long?>("LocationId");

                    b.Property<int>("LocationType");

                    b.Property<string>("PaymentId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<decimal?>("Previsional");

                    b.Property<string>("Reason");

                    b.Property<bool>("Rejected");

                    b.Property<DateTime>("RejectedAt");

                    b.Property<int>("Status");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.Property<DateTime?>("ValidationDate");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.UserActivity", b =>
                {
                    b.Property<string>("DoesCode");

                    b.Property<string>("UserId");

                    b.Property<int>("Weight");

                    b.HasKey("DoesCode", "UserId");
                });

            modelBuilder.Entity("Yavsc.Server.Models.EMailing.MailingTemplate", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Body")
                        .HasAnnotation("MaxLength", 65536);

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("ManagerId");

                    b.Property<string>("ReplyToAddress");

                    b.Property<int>("ToSend");

                    b.Property<string>("Topic")
                        .HasAnnotation("MaxLength", 128);

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Server.Models.IT.Project", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActivityCode")
                        .IsRequired();

                    b.Property<string>("ClientId")
                        .IsRequired();

                    b.Property<bool>("Consent");

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("OwnerId");

                    b.Property<string>("PaymentId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<decimal?>("Previsional");

                    b.Property<bool>("Rejected");

                    b.Property<DateTime>("RejectedAt");

                    b.Property<int>("Status");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.Property<DateTime?>("ValidationDate");

                    b.Property<string>("Version");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Server.Models.IT.ProjectBuildConfiguration", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<long>("ProjectId");

                    b.HasKey("Id");
                });

           

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNet.Identity.EntityFramework.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Microsoft.AspNet.Identity.EntityFramework.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNet.Identity.EntityFramework.IdentityRole")
                        .WithMany()
                        .HasForeignKey("RoleId");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Access.BlackListed", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.Access.CircleAuthorizationToBlogPost", b =>
                {
                    b.HasOne("Yavsc.Models.Blog.BlogPost")
                        .WithMany()
                        .HasForeignKey("BlogPostId");

                    b.HasOne("Yavsc.Models.Relationship.Circle")
                        .WithMany()
                        .HasForeignKey("CircleId");
                });

            modelBuilder.Entity("Yavsc.Models.AccountBalance", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithOne()
                        .HasForeignKey("Yavsc.Models.AccountBalance", "UserId");
                });

            modelBuilder.Entity("Yavsc.Models.ApplicationUser", b =>
                {
                    b.HasOne("Yavsc.Models.Bank.BankIdentity")
                        .WithMany()
                        .HasForeignKey("BankInfoId");

                    b.HasOne("Yavsc.Models.Relationship.Location")
                        .WithMany()
                        .HasForeignKey("PostalAddressId");
                });

            modelBuilder.Entity("Yavsc.Models.BalanceImpact", b =>
                {
                    b.HasOne("Yavsc.Models.AccountBalance")
                        .WithMany()
                        .HasForeignKey("BalanceId");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.CommandLine", b =>
                {
                    b.HasOne("Yavsc.Models.Billing.Estimate")
                        .WithMany()
                        .HasForeignKey("EstimateId");

                    b.HasOne("Yavsc.Models.Billing.EstimateTemplate")
                        .WithMany()
                        .HasForeignKey("EstimateTemplateId");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.Estimate", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("Yavsc.Models.Workflow.RdvQuery")
                        .WithMany()
                        .HasForeignKey("CommandId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.Blog.BlogPost", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("AuthorId");
                });

            modelBuilder.Entity("Yavsc.Models.Blog.BlogTag", b =>
                {
                    b.HasOne("Yavsc.Models.Blog.BlogPost")
                        .WithMany()
                        .HasForeignKey("PostId");

                    b.HasOne("Yavsc.Models.Relationship.Tag")
                        .WithMany()
                        .HasForeignKey("TagId");
                });

            modelBuilder.Entity("Yavsc.Models.Blog.Comment", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("AuthorId");

                    b.HasOne("Yavsc.Models.Blog.Comment")
                        .WithMany()
                        .HasForeignKey("ParentId");

                    b.HasOne("Yavsc.Models.Blog.BlogPost")
                        .WithMany()
                        .HasForeignKey("PostId");
                });

            modelBuilder.Entity("Yavsc.Models.Calendar.Schedule", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.Calendar.ScheduledEvent", b =>
                {
                    b.HasOne("Yavsc.Models.Calendar.Schedule")
                        .WithMany()
                        .HasForeignKey("ScheduleOwnerId");

                    b.HasOne("Yavsc.Models.Calendar.Period")
                        .WithMany()
                        .HasForeignKey("PeriodStart", "PeriodEnd");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.ChatConnection", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.ChatRoom", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.ChatRoomPresence", b =>
                {
                    b.HasOne("Yavsc.Models.Chat.ChatRoom")
                        .WithMany()
                        .HasForeignKey("ChannelName");

                    b.HasOne("Yavsc.Models.Chat.ChatConnection")
                        .WithMany()
                        .HasForeignKey("ChatUserConnectionId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.BrusherProfile", b =>
                {
                    b.HasOne("Yavsc.Models.Calendar.Schedule")
                        .WithMany()
                        .HasForeignKey("ScheduleOwnerId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairCutQuery", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ActivityCode");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("Yavsc.Models.Relationship.Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.HasOne("Yavsc.Models.Payment.PayPalPayment")
                        .WithMany()
                        .HasForeignKey("PaymentId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");

                    b.HasOne("Yavsc.Models.Haircut.HairPrestation")
                        .WithMany()
                        .HasForeignKey("PrestationId");

                    b.HasOne("Yavsc.Models.Haircut.BrusherProfile")
                        .WithMany()
                        .HasForeignKey("SelectedProfileUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairMultiCutQuery", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ActivityCode");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("Yavsc.Models.Relationship.Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.HasOne("Yavsc.Models.Payment.PayPalPayment")
                        .WithMany()
                        .HasForeignKey("PaymentId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairPrestationCollectionItem", b =>
                {
                    b.HasOne("Yavsc.Models.Haircut.HairPrestation")
                        .WithMany()
                        .HasForeignKey("PrestationId");

                    b.HasOne("Yavsc.Models.Haircut.HairMultiCutQuery")
                        .WithMany()
                        .HasForeignKey("QueryId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairTaint", b =>
                {
                    b.HasOne("Yavsc.Models.Drawing.Color")
                        .WithMany()
                        .HasForeignKey("ColorId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairTaintInstance", b =>
                {
                    b.HasOne("Yavsc.Models.Haircut.HairPrestation")
                        .WithMany()
                        .HasForeignKey("PrestationId");

                    b.HasOne("Yavsc.Models.Haircut.HairTaint")
                        .WithMany()
                        .HasForeignKey("TaintId");
                });

            modelBuilder.Entity("Yavsc.Models.Identity.GoogleCloudMobileDeclaration", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("DeviceOwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.IT.Fixing.Bug", b =>
                {
                    b.HasOne("Yavsc.Models.IT.Maintaining.Feature")
                        .WithMany()
                        .HasForeignKey("FeatureId");
                });

            modelBuilder.Entity("Yavsc.Models.Market.Service", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ContextId");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.Announce", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("OwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.DimissClicked", b =>
                {
                    b.HasOne("Yavsc.Models.Messaging.Notification")
                        .WithMany()
                        .HasForeignKey("NotificationId");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.MusicalPreference", b =>
                {
                    b.HasOne("Yavsc.Models.Musical.Profiles.DjSettings")
                        .WithMany()
                        .HasForeignKey("DjSettingsUserId");

                    b.HasOne("Yavsc.Models.Musical.Profiles.GeneralSettings")
                        .WithMany()
                        .HasForeignKey("GeneralSettingsUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Musical.Profiles.Instrumentation", b =>
                {
                    b.HasOne("Yavsc.Models.Musical.Instrument")
                        .WithMany()
                        .HasForeignKey("InstrumentId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Payment.PayPalPayment", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ExecutorId");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.Circle", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.CircleMember", b =>
                {
                    b.HasOne("Yavsc.Models.Relationship.Circle")
                        .WithMany()
                        .HasForeignKey("CircleId");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("MemberId");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.Contact", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.HyperLink", b =>
                {
                    b.HasOne("Yavsc.Models.Haircut.BrusherProfile")
                        .WithMany()
                        .HasForeignKey("BrusherProfileUserId");

                    b.HasOne("Yavsc.Models.Payment.PayPalPayment")
                        .WithMany()
                        .HasForeignKey("PayPalPaymentCreationToken");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.Activity", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ParentCode");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.CommandForm", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ActivityCode");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.CoWorking", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Profiles.FormationSettings")
                        .WithMany()
                        .HasForeignKey("FormationSettingsUserId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("WorkingForId");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.PerformerProfile", b =>
                {
                    b.HasOne("Yavsc.Models.Relationship.Location")
                        .WithMany()
                        .HasForeignKey("OrganizationAddressId");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.RdvQuery", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ActivityCode");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("Yavsc.Models.Relationship.Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.HasOne("Yavsc.Models.Payment.PayPalPayment")
                        .WithMany()
                        .HasForeignKey("PaymentId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.UserActivity", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("DoesCode");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Server.Models.EMailing.MailingTemplate", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ManagerId");
                });

            modelBuilder.Entity("Yavsc.Server.Models.IT.Project", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ActivityCode");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ClientId");


                    b.HasOne("Yavsc.Models.Payment.PayPalPayment")
                        .WithMany()
                        .HasForeignKey("PaymentId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Server.Models.IT.ProjectBuildConfiguration", b =>
                {
                    b.HasOne("Yavsc.Server.Models.IT.Project")
                        .WithMany()
                        .HasForeignKey("ProjectId");
                });

           
        }
    }
}
