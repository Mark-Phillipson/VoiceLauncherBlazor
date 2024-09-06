using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;

namespace DataAccessLibrary.Models
{

	public partial class ApplicationDbContext : DbContext
	{
		//public ApplicationDbContext()
		//{
		//}
		//public ApplicationDbContext(string connectionString)
		//{
		//	_connectionString = connectionString;
		//}

		private IConfiguration _configuration;
		//public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		//	 : base(options)
		//{
		//	_configuration =  null ;
		//}
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration)
			 : base(options)
		{
			_configuration = configuration;
		}
		public virtual DbSet<Transaction> Transactions { get; set; }
		public virtual DbSet<TransactionTypeMapping> TransactionTypeMappings { get; set; }
		public virtual DbSet<CursorlessCheatsheetItem> CursorlessCheatsheetItems { get; set; }
		public virtual DbSet<CssProperty> CssProperties { get; set; }
		public virtual DbSet<TalonAlphabet> TalonAlphabets { get; set; }
		public virtual DbSet<Prompt> Prompts { get; set; }
		public virtual DbSet<Microphone> Microphones { get; set; }
		public virtual DbSet<ApplicationDetail> ApplicationDetails { get; set; }
		public virtual DbSet<Idiosyncrasy> Idiosyncrasies { get; set; }
		public virtual DbSet<PhraseListGrammar> PhraseListGrammars { get; set; }
		public virtual DbSet<GrammarName> GrammarNames { get; set; }
		public virtual DbSet<GrammarItem> GrammarItems { get; set; }
		public virtual DbSet<CustomWindowsSpeechCommand> CustomWindowsSpeechCommands { get; set; }
		public virtual DbSet<WindowsSpeechVoiceCommand> WindowsSpeechVoiceCommands { get; set; }
		public virtual DbSet<SpokenForm> SpokenForms { get; set; }
		public virtual DbSet<Category> Categories { get; set; }
		public virtual DbSet<Computer> Computers { get; set; }
		public virtual DbSet<CurrentWindow> CurrentWindow { get; set; }
		public virtual DbSet<CustomIntelliSense> CustomIntelliSenses { get; set; }
		public virtual DbSet<GeneralLookup> GeneralLookups { get; set; }
		public virtual DbSet<HtmlTag> HtmlTags { get; set; }
		public virtual DbSet<Language> Languages { get; set; }
		public virtual DbSet<Launcher> Launcher { get; set; }
		public virtual DbSet<LauncherMultipleLauncherBridge> LauncherMultipleLauncherBridge { get; set; }
		public virtual DbSet<Logins> Logins { get; set; }
		public virtual DbSet<MigrationHistory> MigrationHistory { get; set; }
		public virtual DbSet<MousePositions> MousePositions { get; set; }
		public virtual DbSet<MultipleLauncher> MultipleLauncher { get; set; }
		public virtual DbSet<PhraseListGrammar> PhraseListGrammarStorages { get; set; }
		public virtual DbSet<PropertyTabPositions> PropertyTabPositions { get; set; }
		public virtual DbSet<SavedMousePosition> SavedMousePosition { get; set; }
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
		public virtual DbSet<Todo> Todos { get; set; }
		public virtual DbSet<Appointment> Appointments { get; set; }
		public virtual DbSet<AdditionalCommand> AdditionalCommands { get; set; }
		public virtual DbSet<ValuesToInsert> ValuesToInserts { get; set; }
		public virtual DbSet<VisualStudioCommand> VisualStudioCommands { get; set; }
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			if (!optionsBuilder.IsConfigured)
			{
				if (_configuration != null)
				{
					optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
				}
				else
				{
					optionsBuilder.UseSqlServer("Data Source=Localhost;Initial Catalog=VoiceLauncher;Integrated Security=True;Connect Timeout=120;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
				}
#if DEBUG
				optionsBuilder.LogTo(message => Debug.WriteLine(message));
				optionsBuilder.ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
#endif
			}
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<TransactionTypeMapping>()
				.HasIndex(t => new { t.MyTransactionType, t.Type })
				.IsUnique();
			modelBuilder.Entity<Transaction>()
						.Property(c => c.MoneyIn).HasColumnType("decimal(10,2)");

			modelBuilder.Entity<Transaction>()
			.Property(c => c.Balance).HasColumnType("decimal(10,2)");

			modelBuilder.Entity<Transaction>()
			.Property(c => c.MoneyOut).HasColumnType("decimal(10,2)");

			modelBuilder.Entity<AdditionalCommand>()
					.Property(c => c.WaitBefore).HasColumnType("decimal(10,2)");

			modelBuilder.Entity<CustomIntelliSense>(entity =>
			{
				entity.HasIndex(e => e.CategoryId)
						 .HasDatabaseName("IX_CategoryID");

				entity.HasIndex(e => e.ComputerId)
						 .HasDatabaseName("IX_ComputerID");

				entity.HasIndex(e => e.LanguageId)
						 .HasDatabaseName("IX_LanguageID");

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
						 .HasDatabaseName("IX_CategoryID");

				entity.HasIndex(e => e.ComputerId)
						 .HasDatabaseName("IX_ComputerID");

				entity.HasOne(d => d.Category)
						 .WithMany(p => p.Launchers)
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
