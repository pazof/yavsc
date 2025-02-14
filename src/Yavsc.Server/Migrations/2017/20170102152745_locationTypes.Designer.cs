
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Yavsc.Models;

namespace Yavsc.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170102152745_locationTypes")]
    partial class locationTypes
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

            modelBuilder.Entity("Yavsc.Location", b =>
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

            modelBuilder.Entity("Yavsc.Model.Bank.BankIdentity", b =>
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

            modelBuilder.Entity("Yavsc.Models.Access.BlackListed", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("OwnerId");

                    b.Property<string>("UserId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.AccountBalance", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<long>("ContactCredits");

                    b.Property<decimal>("Credits");

                    b.HasKey("UserId");
                });

            modelBuilder.Entity("Yavsc.Models.Activity", b =>
                {
                    b.Property<string>("Code")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<string>("ActorDenomination");

                    b.Property<string>("Description");

                    b.Property<string>("ModeratorGroupName");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 512);

                    b.Property<string>("Photo");

                    b.HasKey("Code");
                });

            modelBuilder.Entity("Yavsc.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("Avatar")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<long?>("BankInfoId");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("DedicatedGoogleCalendar");

                    b.Property<long>("DiskQuota");

                    b.Property<long>("DiskUsage");

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<string>("FullName")
                        .HasAnnotation("MaxLength", 512);

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

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

            modelBuilder.Entity("Yavsc.Models.Billing.CommandLine", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("ArticleId");

                    b.Property<int>("Count");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 512);

                    b.Property<long>("EstimateId");

                    b.Property<long?>("EstimateTemplateId");

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

                    b.Property<string>("CommandType");

                    b.Property<string>("Description");

                    b.Property<string>("OwnerId")
                        .IsRequired();

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

            modelBuilder.Entity("Yavsc.Models.Blog", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AuthorId");

                    b.Property<string>("Content");

                    b.Property<DateTime>("Modified");

                    b.Property<string>("Photo");

                    b.Property<DateTime>("Posted")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Relational:GeneratedValueSql", "LOCALTIMESTAMP");

                    b.Property<int>("Rate");

                    b.Property<string>("Title");

                    b.Property<bool>("Visible");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Booking.BookQuery", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClientId")
                        .IsRequired();

                    b.Property<DateTime>("CreationDate")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Relational:GeneratedValueSql", "LOCALTIMESTAMP");

                    b.Property<DateTime>("EventDate");

                    b.Property<long?>("LocationId");

                    b.Property<long?>("LocationTypeId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<decimal?>("Previsional");

                    b.Property<string>("Reason");

                    b.Property<DateTime?>("ValidationDate");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Booking.MusicalPreference", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.Property<long>("OwnerId");

                    b.Property<int>("Rate");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Booking.MusicalTendency", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 255);

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.Connection", b =>
                {
                    b.Property<string>("ConnectionId");

                    b.Property<string>("ApplicationUserId");

                    b.Property<bool>("Connected");

                    b.Property<string>("UserAgent");

                    b.HasKey("ConnectionId");
                });

            modelBuilder.Entity("Yavsc.Models.Circle", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ApplicationUserId");

                    b.Property<string>("Name");

                    b.Property<string>("OwnerId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.CircleMember", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("CircleId");

                    b.Property<string>("MemberId")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Contact", b =>
                {
                    b.Property<string>("OwnerId");

                    b.Property<string>("UserId");

                    b.Property<string>("ApplicationUserId");

                    b.HasKey("OwnerId", "UserId");
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

                    b.Property<string>("Model");

                    b.Property<string>("Platform");

                    b.Property<string>("Version");

                    b.HasKey("DeviceId");
                });

            modelBuilder.Entity("Yavsc.Models.Market.BaseProduct", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("Name");

                    b.Property<bool>("Public");

                    b.HasKey("Id");

                    b.HasAnnotation("Relational:DiscriminatorProperty", "Discriminator");

                    b.HasAnnotation("Relational:DiscriminatorValue", "BaseProduct");
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

            modelBuilder.Entity("Yavsc.Models.Messaging.ClientProviderInfo", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("Avatar");

                    b.Property<long?>("BillingAddressId");

                    b.Property<string>("EMail");

                    b.Property<string>("Phone");

                    b.Property<string>("UserName");

                    b.HasKey("UserId");
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

            modelBuilder.Entity("Yavsc.Models.PostTag", b =>
                {
                    b.Property<long>("PostId");

                    b.Property<long>("TagId");

                    b.HasKey("PostId", "TagId");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.LocationType", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

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

            modelBuilder.Entity("Yavsc.Models.Tag", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.PerformerProfile", b =>
                {
                    b.Property<string>("PerformerId");

                    b.Property<bool>("AcceptGeoLocalization");

                    b.Property<bool>("AcceptNotifications");

                    b.Property<bool>("AcceptPublicContact");

                    b.Property<bool>("Active");

                    b.Property<string>("ActivityCode")
                        .IsRequired();

                    b.Property<int?>("MaxDailyCost");

                    b.Property<int?>("MinDailyCost");

                    b.Property<long?>("OfferId");

                    b.Property<long>("OrganizationAddressId");

                    b.Property<int>("Rate");

                    b.Property<string>("SIREN")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 14);

                    b.Property<string>("WebSite");

                    b.HasKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Market.Product", b =>
                {
                    b.HasBaseType("Yavsc.Models.Market.BaseProduct");

                    b.Property<decimal>("Depth");

                    b.Property<decimal>("Height");

                    b.Property<decimal?>("Price");

                    b.Property<decimal>("Weight");

                    b.Property<decimal>("Width");

                    b.HasAnnotation("Relational:DiscriminatorValue", "Product");
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

            modelBuilder.Entity("Yavsc.Models.AccountBalance", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithOne()
                        .HasForeignKey("Yavsc.Models.AccountBalance", "UserId");
                });

            modelBuilder.Entity("Yavsc.Models.ApplicationUser", b =>
                {
                    b.HasOne("Yavsc.Model.Bank.BankIdentity")
                        .WithMany()
                        .HasForeignKey("BankInfoId");

                    b.HasOne("Yavsc.Location")
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
                    b.HasOne("Yavsc.Models.Market.BaseProduct")
                        .WithMany()
                        .HasForeignKey("ArticleId");

                    b.HasOne("Yavsc.Models.Billing.Estimate")
                        .WithMany()
                        .HasForeignKey("EstimateId");

                    b.HasOne("Yavsc.Models.Billing.EstimateTemplate")
                        .WithMany()
                        .HasForeignKey("EstimateTemplateId");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.Estimate", b =>
                {
                    b.HasOne("Yavsc.Models.Booking.BookQuery")
                        .WithMany()
                        .HasForeignKey("CommandId");
                });

            modelBuilder.Entity("Yavsc.Models.Blog", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("AuthorId");
                });

            modelBuilder.Entity("Yavsc.Models.Booking.BookQuery", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ClientId");

                    b.HasOne("Yavsc.Location")
                        .WithMany()
                        .HasForeignKey("LocationId");

                    b.HasOne("Yavsc.Models.Relationship.LocationType")
                        .WithMany()
                        .HasForeignKey("LocationTypeId");

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.Connection", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Circle", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Yavsc.Models.CircleMember", b =>
                {
                    b.HasOne("Yavsc.Models.Circle")
                        .WithMany()
                        .HasForeignKey("CircleId");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("MemberId");
                });

            modelBuilder.Entity("Yavsc.Models.Contact", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
                });

            modelBuilder.Entity("Yavsc.Models.Identity.GoogleCloudMobileDeclaration", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("DeviceOwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.Market.Service", b =>
                {
                    b.HasOne("Yavsc.Models.Activity")
                        .WithMany()
                        .HasForeignKey("ContextId");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.ClientProviderInfo", b =>
                {
                    b.HasOne("Yavsc.Location")
                        .WithMany()
                        .HasForeignKey("BillingAddressId");
                });

            modelBuilder.Entity("Yavsc.Models.PostTag", b =>
                {
                    b.HasOne("Yavsc.Models.Blog")
                        .WithMany()
                        .HasForeignKey("PostId");
                });

            modelBuilder.Entity("Yavsc.Models.Workflow.PerformerProfile", b =>
                {
                    b.HasOne("Yavsc.Models.Activity")
                        .WithMany()
                        .HasForeignKey("ActivityCode");

                    b.HasOne("Yavsc.Models.Market.Service")
                        .WithMany()
                        .HasForeignKey("OfferId");

                    b.HasOne("Yavsc.Location")
                        .WithMany()
                        .HasForeignKey("OrganizationAddressId");

                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });
        }
    }
}
