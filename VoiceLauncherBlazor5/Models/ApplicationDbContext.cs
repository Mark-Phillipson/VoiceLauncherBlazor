using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace VoiceLauncherBlazor.Models
{

    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
            : base(options)
        {
            _configuration = configuration;
        }
        private IConfiguration _configuration;
        public virtual DbSet<AspNetRoleClaims> AspNetRoleClaims { get; set; }
        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
        public virtual DbSet<AspNetUserTokens> AspNetUserTokens { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Computer> Computers { get; set; }
        public virtual DbSet<CurrentWindow> CurrentWindow { get; set; }
        public virtual DbSet<CustomIntelliSense> CustomIntelliSense { get; set; }
        public virtual DbSet<GeneralLookup> GeneralLookups { get; set; }
        public virtual DbSet<HtmlTags> HtmlTags { get; set; }
        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<Launcher> Launcher { get; set; }
        public virtual DbSet<LauncherMultipleLauncherBridge> LauncherMultipleLauncherBridge { get; set; }
        public virtual DbSet<Logins> Logins { get; set; }
        public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
        public virtual DbSet<MousePositions> MousePositions { get; set; }
        public virtual DbSet<MultipleLauncher> MultipleLauncher { get; set; }
        public virtual DbSet<PropertyTabPositions> PropertyTabPositions { get; set; }
        public virtual DbSet<SavedMousePosition> SavedMousePosition { get; set; }
        public virtual DbSet<ValuesToInsert> ValuesToInsert { get; set; }
        public virtual DbSet<ViewCategories> ViewCategories { get; set; }
        public virtual DbSet<ViewComputers> ViewComputers { get; set; }
        public virtual DbSet<ViewCustomIntelliSense> ViewCustomIntelliSense { get; set; }
        public virtual DbSet<ViewHtmlTags> ViewHtmlTags { get; set; }
        public virtual DbSet<ViewLanguages> ViewLanguages { get; set; }
        public virtual DbSet<ViewLauncher> ViewLauncher { get; set; }
        public virtual DbSet<ViewLauncherMultipleLauncherBridge> ViewLauncherMultipleLauncherBridge { get; set; }
        public virtual DbSet<ViewMousePositions> ViewMousePositions { get; set; }
        public virtual DbSet<ViewMultipleLauncher> ViewMultipleLauncher { get; set; }
        public virtual DbSet<ViewPropertyTabPositions> ViewPropertyTabPositions { get; set; }
        public virtual DbSet<ViewSavedMousePosition> ViewSavedMousePosition { get; set; }
        public virtual DbSet<ViewValuesToInsert> ViewValuesToInsert { get; set; }
        public virtual DbSet<Example> Examples { get; set; }
        //public virtual DbSet<Todo> Todos { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoleClaims>(entity =>
            {
                entity.HasIndex(e => e.RoleId);
            });

            modelBuilder.Entity<AspNetRoles>(entity =>
            {
                entity.HasIndex(e => e.NormalizedName)
                    .HasName("RoleNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedName] IS NOT NULL)");
            });

            modelBuilder.Entity<AspNetUserClaims>(entity =>
            {
                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AspNetUserLogins>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

                entity.HasIndex(e => e.UserId);
            });

            modelBuilder.Entity<AspNetUserRoles>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.RoleId });

                entity.HasIndex(e => e.RoleId);
            });

            modelBuilder.Entity<AspNetUserTokens>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });
            });

            modelBuilder.Entity<AspNetUsers>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail)
                    .HasName("EmailIndex");

                entity.HasIndex(e => e.NormalizedUserName)
                    .HasName("UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");
            });

            modelBuilder.Entity<CustomIntelliSense>(entity =>
            {
                entity.HasIndex(e => e.CategoryId)
                    .HasName("IX_CategoryID");

                entity.HasIndex(e => e.ComputerId)
                    .HasName("IX_ComputerID");

                entity.HasIndex(e => e.LanguageId)
                    .HasName("IX_LanguageID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.CustomIntelliSense)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_dbo.CustomIntelliSense_dbo.Categories_CategoryID");

                entity.HasOne(d => d.Computer)
                    .WithMany(p => p.CustomIntelliSense)
                    .HasForeignKey(d => d.ComputerId)
                    .HasConstraintName("FK_dbo.CustomIntelliSense_dbo.Computers_ComputerID");

                entity.HasOne(d => d.Language)
                    .WithMany(p => p.CustomIntelliSense)
                    .HasForeignKey(d => d.LanguageId)
                    .HasConstraintName("FK_dbo.CustomIntelliSense_dbo.Languages_LanguageID");
            });

            modelBuilder.Entity<Launcher>(entity =>
            {
                entity.HasIndex(e => e.CategoryId)
                    .HasName("IX_CategoryID");

                entity.HasIndex(e => e.ComputerId)
                    .HasName("IX_ComputerID");

                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Launcher)
                    .HasForeignKey(d => d.CategoryId)
                    .HasConstraintName("FK_dbo.Launcher_dbo.Categories_CategoryID");

                entity.HasOne(d => d.Computer)
                    .WithMany(p => p.Launcher)
                    .HasForeignKey(d => d.ComputerId)
                    .HasConstraintName("FK_dbo.Launcher_dbo.Computers_ComputerID");
            });

            modelBuilder.Entity<MigrationHistory>(entity =>
            {
                entity.HasKey(e => new { e.MigrationId, e.ContextKey })
                    .HasName("PK_dbo.__MigrationHistory");
            });

            modelBuilder.Entity<ViewCategories>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_Categories");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewComputers>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_Computers");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewCustomIntelliSense>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_CustomIntelliSense");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewHtmlTags>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_HtmlTags");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewLanguages>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_Languages");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewLauncher>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_Launcher");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewLauncherMultipleLauncherBridge>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_LauncherMultipleLauncherBridge");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewMousePositions>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_MousePositions");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewMultipleLauncher>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_MultipleLauncher");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewPropertyTabPositions>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_PropertyTabPositions");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewSavedMousePosition>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_SavedMousePosition");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            modelBuilder.Entity<ViewValuesToInsert>(entity =>
            {
                entity.HasNoKey();

                entity.ToView("View_ValuesToInsert");

                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
