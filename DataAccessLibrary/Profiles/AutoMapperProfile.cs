using AutoMapper;

using DataAccessLibrary.DTO;
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

        }
    }
}
