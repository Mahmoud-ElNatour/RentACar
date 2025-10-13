using AutoMapper;
using RentACar.Application.DTOs;
using RentACar.Core.Entities;

namespace RentACar.Application.Managers;

public class TravelActionProfile : Profile
{
    public TravelActionProfile()
    {
        CreateMap<TravelActionLog, TravelActionLogDto>();
    }
}
