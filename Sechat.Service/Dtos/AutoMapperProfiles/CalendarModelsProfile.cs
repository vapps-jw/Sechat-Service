using AutoMapper;
using Sechat.Data.Models.CalendarModels;
using Sechat.Service.Dtos.CalendarDtos;

namespace Sechat.Service.Dtos.AutoMapperProfiles;

public class CalendarModelsProfile : Profile
{

    public CalendarModelsProfile()
    {
        _ = CreateMap<Calendar, Calendar>();
        _ = CreateMap<CalendarEvent, CalendarEventDto>();
    }
}
