using AmmenTravel.DestinosDTO;
using AutoMapper;
using AmmenTravel.Destinos;
using AmmenTravel.Opiniones;
using AmmenTravel.OpinionesDTO;

namespace AmmenTravel;

public class AmmenTravelApplicationAutoMapperProfile : Profile
{
    public AmmenTravelApplicationAutoMapperProfile()
    {
        CreateMap<DestinoTuristico, guardarDestinoDTO>();
        CreateMap<CreateUpdateDestinoDTO, DestinoTuristico>();

        CreateMap<Opinion, OpinionDto>()
            .ForMember(d => d.DestinoId, opt => opt.MapFrom(s => s.DestinoTuristicoId));
    }

}