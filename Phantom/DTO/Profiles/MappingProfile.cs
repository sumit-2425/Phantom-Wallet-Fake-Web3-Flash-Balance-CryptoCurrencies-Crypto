using AutoMapper;
using Backend.DTO;
using Backend.Models;

namespace Backend.Profiles;
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<OpeningHour, OpenedPharmacyDTO>()
            .ForMember(des => des.Id, opt => opt.MapFrom(src => src.Pharmacy.Id))
            .ForMember(des => des.Name, opt => opt.MapFrom(src => src.Pharmacy.Name));

        CreateMap<Mask, MaskInfoDTO>()
            .ForMember(des => des.Name, opt => opt.MapFrom(src => src.MaskType.Name))
            .ForMember(des => des.Color, opt => opt.MapFrom(src => src.MaskType.Color))
            .ForMember(des => des.QuantityPerPack, opt => opt.MapFrom(src => src.MaskType.Quantity));

        CreateMap<User, UserBaseDTO>();

        CreateMap<Pharmacy, PharmacyBaseDTO>();
        
        CreateMap<Transaction, TransactionGetDTO>()
            .ForMember(des => des.User, opt => opt.MapFrom(src => src.User))
            .ForMember(des => des.Pharmacy, opt => opt.MapFrom(src => src.Pharmacy))
            .ForMember(des => des.Mask, opt => opt.MapFrom(src => src.Mask));

        CreateMap<Pharmacy, PharmacyBaseDTO>();
    }
}