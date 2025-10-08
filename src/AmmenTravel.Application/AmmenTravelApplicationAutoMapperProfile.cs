using AmmenTravel.DestinosDTO;
using AutoMapper;
using AmmenTravel.Destinos;


namespace AmmenTravel;

public class AmmenTravelApplicationAutoMapperProfile : Profile
{
    public AmmenTravelApplicationAutoMapperProfile()
    {
        CreateMap<DestinoTuristico, guardarDestinoDTO>();
        CreateMap<CreateUpdateDestinoDTO, DestinoTuristico>();
    }

}
