using AutoMapper;

using DataAccessLibrary.DTO;
using DataAccessLibrary.DTOs;
using DataAccessLibrary.Models;


namespace DataAccessLibrary.Profiles
{
	public class AutoMapperProfile : Profile
	{
		public AutoMapperProfile()
		{
			CreateMap<SavedMousePosition, SavedMousePositionDTO>(); CreateMap<SavedMousePositionDTO, SavedMousePosition>();
			CreateMap<CustomWindowsSpeechCommand, CustomWindowsSpeechCommandDTO>(); CreateMap<CustomWindowsSpeechCommandDTO, CustomWindowsSpeechCommand>();
			CreateMap<WindowsSpeechVoiceCommand, WindowsSpeechVoiceCommandDTO>(); CreateMap<WindowsSpeechVoiceCommandDTO, WindowsSpeechVoiceCommand>();
			CreateMap<GrammarName, GrammarNameDTO>(); CreateMap<GrammarNameDTO, GrammarName>();
			CreateMap<GrammarItem, GrammarItemDTO>(); CreateMap<GrammarItemDTO, GrammarItem>();
			CreateMap<HtmlTag, HtmlTagDTO>(); CreateMap<HtmlTagDTO, HtmlTag>();
			CreateMap<ApplicationDetail, ApplicationDetailDTO>(); CreateMap<ApplicationDetailDTO, ApplicationDetail>();
			CreateMap<Idiosyncrasy, IdiosyncrasyDTO>(); CreateMap<IdiosyncrasyDTO, Idiosyncrasy>();
			CreateMap<PhraseListGrammar, PhraseListGrammarDTO>(); CreateMap<PhraseListGrammarDTO, PhraseListGrammar>();
			CreateMap<Launcher, LauncherDTO>(); CreateMap<LauncherDTO, Launcher>();
			CreateMap<Category, CategoryDTO>(); CreateMap<CategoryDTO, Category>();
			CreateMap<ValuesToInsert, ValueToInsertDTO>(); CreateMap<ValueToInsertDTO, ValuesToInsert>();
			CreateMap<SpokenForm, SpokenFormDTO>();
			CreateMap<SpokenFormDTO, SpokenForm>();
			CreateMap<Microphone, MicrophoneDTO>();
			CreateMap<MicrophoneDTO, Microphone>();
			CreateMap<CustomIntelliSense, CustomIntelliSenseDTO>();
			CreateMap<CustomIntelliSenseDTO, CustomIntelliSense>();
			CreateMap<Prompt, PromptDTO>();
			CreateMap<PromptDTO, Prompt>();
			CreateMap<TalonAlphabet, TalonAlphabetDTO>();
			CreateMap<TalonAlphabetDTO, TalonAlphabet>();
			CreateMap<Language, LanguageDTO>();
			CreateMap<LanguageDTO, Language>();
			CreateMap<CursorlessCheatsheetItem, CursorlessCheatsheetItemDTO>();
			CreateMap<CursorlessCheatsheetItemDTO, CursorlessCheatsheetItem>();
			CreateMap<CssProperty, CssPropertyDTO>();
			CreateMap<CssPropertyDTO, CssProperty>();
			CreateMap<Transaction, TransactionDTO>();
			CreateMap<TransactionDTO, Transaction>();
			CreateMap<TransactionTypeMapping, TransactionTypeMappingDTO>();
			CreateMap<TransactionTypeMappingDTO, TransactionTypeMapping>();
			CreateMap<Example, ExampleDTO>();
			CreateMap<ExampleDTO, Example>();
		}
	}
}
