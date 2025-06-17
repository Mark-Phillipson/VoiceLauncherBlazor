namespace RazorClassLibrary.Models
{
    public class TalonAnalysisResult
    {
        public int TotalCommands { get; set; }
        public int UniqueCommands { get; set; }
        public int TotalConflicts { get; set; }
        public int GlobalConflicts { get; set; }
        public int AppSpecificConflicts { get; set; }
        public List<RepositoryStats> RepositoryStats { get; set; } = new();
        public List<ConflictDetail> GlobalConflictDetails { get; set; } = new();
        public List<ConflictDetail> AppConflictDetails { get; set; } = new();
    }

    public class RepositoryStats
    {
        public string Repository { get; set; } = string.Empty;
        public int CommandCount { get; set; }
        public int ConflictCount { get; set; }
        public double ConflictPercentage => CommandCount > 0 ? (double)ConflictCount / CommandCount * 100 : 0;
    }    public class ConflictDetail
    {
        public string Command { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<string> Repositories { get; set; } = new();
        public int InstanceCount { get; set; }
        public bool HasDifferentImplementations { get; set; }
        public bool IsTrueConflict { get; set; } = true; // New property to distinguish real conflicts from alternatives
        public ConflictType ConflictType { get; set; } = ConflictType.TrueConflict;
        public List<ImplementationDetail> Implementations { get; set; } = new();
    }    public enum ConflictType
    {
        TrueConflict,           // Same repo duplicates or very similar implementations
        AlternativeImplementations, // Different approaches across repos (not actually conflicts)
        TagBasedMutuallyExclusive,  // Different tags, can't be active simultaneously
        OperatingSystemSpecific     // OS-specific commands that don't conflict
    }

    public class ImplementationDetail
    {
        public string Repository { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty; // "global" or app title
        public string Mode { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string CodeLanguage { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
    }    public class TalonCommand
    {
        public string Command { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string Script { get; set; } = string.Empty;
        public string Application { get; set; } = string.Empty;
        public string Repository { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string OperatingSystem { get; set; } = string.Empty;
        public string CodeLanguage { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
    }
}
