using System;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Yavsc.Models;

namespace Yavsc.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170227151759_hairPrestations")]
    partial class hairPrestations
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

                    b.Property<string>("Avatar")
                        .IsRequired()
                        .HasAnnotation("MaxLength", 512)
                        .HasAnnotation("Relational:DefaultValue", "/images/Users/icon_user.png")
                        .HasAnnotation("Relational:DefaultValueType", "System.String");

                    b.Property<long?>("BankInfoId");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("DedicatedGoogleCalendar");

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

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<string>("Photo");

                    b.Property<int>("Rate");

                    b.Property<string>("Title");

                    b.Property<string>("UserCreated");

                    b.Property<string>("UserModified");

                    b.Property<bool>("Visible");

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

            modelBuilder.Entity("Yavsc.Models.Haircut.HairCutQuery", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ActivityCode")
                        .IsRequired();

                    b.Property<string>("ClientId")
                        .IsRequired();

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<DateTime>("EventDate");

                    b.Property<long?>("LocationId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<long?>("PrestationId");

                    b.Property<decimal?>("Previsional");

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

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<DateTime>("EventDate");

                    b.Property<long?>("LocationId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<decimal?>("Previsional");

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

                    b.Property<long?>("HairMultiCutQueryId");

                    b.Property<int>("Length");

                    b.Property<bool>("Shampoo");

                    b.Property<int>("Tech");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairTaint", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Brand");

                    b.Property<long>("ColorId");

                    b.Property<long?>("HairPrestationId");

                    b.HasKey("Id");
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

                    b.Property<string>("body")
                        .IsRequired();

                    b.Property<string>("click_action")
                        .IsRequired();

                    b.Property<string>("color");

                    b.Property<string>("icon");

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

            modelBuilder.Entity("Yavsc.Models.Relationship.LocationType", b =>
                {
                    b.Property<long>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.HasKey("Id");
                });

            modelBuilder.Entity("Yavsc.Models.Relationship.PostTag", b =>
                {
                    b.Property<long>("PostId");

                    b.Property<long>("TagId");

                    b.HasKey("PostId", "TagId");
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

                    b.Property<string>("ActorDenomination");

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

                    b.Property<string>("Action");

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

                    b.Property<DateTime>("DateCreated");

                    b.Property<DateTime>("DateModified");

                    b.Property<DateTime>("EventDate");

                    b.Property<long?>("LocationId");

                    b.Property<long?>("LocationTypeId");

                    b.Property<string>("PerformerId")
                        .IsRequired();

                    b.Property<decimal?>("Previsional");

                    b.Property<string>("Reason");

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
                    b.HasOne("Yavsc.Models.Blog")
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

            modelBuilder.Entity("Yavsc.Models.Blog", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("AuthorId");
                });

            modelBuilder.Entity("Yavsc.Models.Chat.Connection", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("ApplicationUserId");
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

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");

                    b.HasOne("Yavsc.Models.Haircut.HairPrestation")
                        .WithMany()
                        .HasForeignKey("PrestationId");
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

                    b.HasOne("Yavsc.Models.Workflow.PerformerProfile")
                        .WithMany()
                        .HasForeignKey("PerformerId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairPrestation", b =>
                {
                    b.HasOne("Yavsc.Models.Haircut.HairMultiCutQuery")
                        .WithMany()
                        .HasForeignKey("HairMultiCutQueryId");
                });

            modelBuilder.Entity("Yavsc.Models.Haircut.HairTaint", b =>
                {
                    b.HasOne("Yavsc.Models.Drawing.Color")
                        .WithMany()
                        .HasForeignKey("ColorId");

                    b.HasOne("Yavsc.Models.Haircut.HairPrestation")
                        .WithMany()
                        .HasForeignKey("HairPrestationId");
                });

            modelBuilder.Entity("Yavsc.Models.Identity.GoogleCloudMobileDeclaration", b =>
                {
                    b.HasOne("Yavsc.Models.ApplicationUser")
                        .WithMany()
                        .HasForeignKey("DeviceOwnerId");
                });

            modelBuilder.Entity("Yavsc.Models.Market.Service", b =>
                {
                    b.HasOne("Yavsc.Models.Workflow.Activity")
                        .WithMany()
                        .HasForeignKey("ContextId");
                });

            modelBuilder.Entity("Yavsc.Models.Messaging.ClientProviderInfo", b =>
                {
                    b.HasOne("Yavsc.Models.Relationship.Location")
                        .WithMany()
                        .HasForeignKey("BillingAddressId");
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

            modelBuilder.Entity("Yavsc.Models.Relationship.PostTag", b =>
                {
                    b.HasOne("Yavsc.Models.Blog")
                        .WithMany()
                        .HasForeignKey("PostId");
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

                    b.HasOne("Yavsc.Models.Relationship.LocationType")
                        .WithMany()
                        .HasForeignKey("LocationTypeId");

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
        }
    }
}
