using AutoMapper;
using CarRental.Business.DTOs.Admin;
using CarRental.Domain.Entities;

namespace CarRental.Business.Mappings;

public class AdminMappingProfile : Profile
{
    public AdminMappingProfile()
    {
        CreateMap<User, AdminUserDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.FirstName + " " + src.LastName))
            .ForMember(dest => dest.Role,
                opt => opt.MapFrom(src => src.Role.ToString()));

        CreateMap<Car, AdminCarDto>()
            .ForMember(dest => dest.OwnerName,
                opt => opt.MapFrom(src => src.Owner.FirstName + " " + src.Owner.LastName))
            .ForMember(dest => dest.OwnerEmail,
                opt => opt.MapFrom(src => src.Owner.Email));
    }
}