
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Yavsc.Models;

namespace Yavsc.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20160802143258_bcontentornot")]
    partial class bcontentornot
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
                        .IsRequired();

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

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

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("DedicatedGoogleCalendar");

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

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

                    b.Property<long?>("BookQueryId");

                    b.Property<string>("Comment");

                    b.Property<int>("Count");

                    b.Property<long?>("EstimateId");

                    b.Property<decimal>("UnitaryCost");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Billing.Estimate", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AttachedFilesString");

                    b.Property<string>("AttachedGraphicsString");

                    b.Property<long?>("CommandId");

                    b.Property<string>("Description");

                    b.Property<int?>("Status");

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

                    b.Property<DateTime>("modified");

                    b.Property<string>("photo");

                    b.Property<DateTime>("posted")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("Relational:GeneratedValueSql", "LOCALTIMESTAMP");

                    b.Property<int>("rate");

                    b.Property<string>("title");

                    b.Property<bool>("visible");

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

                    b.Property<int>("Lag");

                    b.Property<long?>("LocationId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<decimal?>("Previsional");

                    b.Property<DateTime?>("ValidationDate");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Circle", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<string>("OwnerId");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.CircleMember", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("CircleId")
                        .IsRequired();

                    b.Property<string>("MemberId")
                        .IsRequired();

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Contact", b =>
                {
                    b.Property<string>("OwnerId");

                    b.Property<string>("UserId");

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

                    b.Property<string>("Name");

                    b.Property<bool>("Public");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Market.Service", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("Billing");

                    b.Property<string>("ContextId");

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.Property<decimal?>("Pricing");

                    b.Property<bool>("Public");

                    b.HasKey("Id");
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

            modelBuilder.Entity("Yavsc.Models.Skill", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int>("Rate");

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

            modelBuilder.Entity("Yavsc.Models.AccountBalance", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithOne()
                        .HasForeignKey("Yavsc.Models.AccountBalance", "UserId");
                });

            modelBuilder.Entity("Yavsc.Models.ApplicationUser", b =>
                {
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

                    b.HasOne("Yavsc.Models.Booking.BookQuery")
                        .WithMany()
                        .HasForeignKey("BookQueryId");

                    b.HasOne("Yavsc.Models.Billing.Estimate")
                        .WithMany()
                        .HasForeignKey("EstimateId");
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

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Circle", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("OwnerId");
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
                        .HasForeignKey("OwnerId");
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
