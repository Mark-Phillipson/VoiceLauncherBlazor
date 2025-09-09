// Lightweight shim types to unblock RazorClassLibrary compilation during incremental migration.
// These are placeholders only and should be replaced by proper SharedContracts types or removed
// once each RCL file is migrated.
namespace SharedContracts.Shims.DataAccessLibrary.DTO
{
    public class CategoryDTO { public int Id { get; set; } public System.Collections.Generic.List<LauncherDTO>? Launchers { get; set; } }
    public class LauncherDTO { public string? Name { get; set; } public string? CommandLine { get; set; } public ComputerDTO? Computer { get; set; } }
    public class ComputerDTO { public string? ComputerName { get; set; } }
    public class LanguageDTO { public string? LanguageName { get; set; } public System.Collections.Generic.List<CustomIntelliSenseDTO>? CustomIntelliSense { get; set; } }
    public class CustomIntelliSenseDTO { public int Id { get; set; } public LanguageDTO? Language { get; set; } public string? DisplayValue { get; set; } public string? SendKeysValue { get; set; } public string? Remarks { get; set; } public string? DeliveryType { get; set; } public string? CommandType { get; set; } }
    public class TransactionDTO { }
    public class PromptDTO { }
    public class QuickPromptDTO { }
    public class ValueToInsertDTO { }
    public class GrammarItemDTO { }
    public class ApplicationDetailDTO { }
    public class SavedMousePositionDTO { }
    public class MicrophoneDTO { }
    public class CursorlessCheatsheetItemDTO { }
    public class CssPropertyDTO { }
    public class ExampleDTO { }
    public class CustomWindowsSpeechCommandDTO { }
    public class WindowsSpeechVoiceCommandDTO { }
    public class SpokenFormDTO { }
    public class CategoryGroupedByLanguageDTO { }
}

namespace SharedContracts.Shims.DataAccessLibrary.Models
{
    public class Category { public int Id { get; set; } public System.Collections.Generic.List<Launcher>? Launchers { get; set; } }
    public class Launcher { public string? Name { get; set; } public string? CommandLine { get; set; } public Computer? Computer { get; set; } }
    public class Computer { public string? ComputerName { get; set; } }
    public class Language { public string? LanguageName { get; set; } public System.Collections.Generic.List<object>? CustomIntelliSense { get; set; } }
    public class CustomIntelliSense { public int Id { get; set; } public Language? Language { get; set; } public string? DisplayValue { get; set; } public string? SendKeysValue { get; set; } public string? Remarks { get; set; } public string? DeliveryType { get; set; } }
}

namespace SharedContracts.Shims.DataAccessLibrary.Models.KnowbrainerCommands
{
    public class CommandSet { }
    public class VoiceCommand { public string? Name { get; set; } public TargetApplication? TargetApplication { get; set; } }
    public class TargetApplication { public string? CommandSource { get; set; } }
}

namespace SharedContracts.Shims.DataAccessLibrary.Services
{
    public interface ICustomIntelliSenseDataService { }
    public interface ILanguageDataService { }
    public interface ICategoryDataService { }
    public interface ILauncherDataService { }
    public interface ITalonVoiceCommandDataService { }
    public interface ITodoData { }
    // Add empty service interfaces as needed
}
