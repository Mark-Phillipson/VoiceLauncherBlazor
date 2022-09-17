using AutoMapper;

using DataAccessLibrary.DTO;
using DataAccessLibrary.Models;

using VoiceLauncher.DTOs;

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
        }
    }
}
