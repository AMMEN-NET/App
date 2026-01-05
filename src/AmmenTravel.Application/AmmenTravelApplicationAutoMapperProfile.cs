using AmmenTravel.DestinosDTO;
using AutoMapper;
using AmmenTravel.Destinos;
using AmmenTravel.Opiniones;
using AmmenTravel.Opiniones.OpinionesDTO;
using AmmenTravel.ExternalService;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Favoritos.FavoritosDTO;

namespace AmmenTravel;

public class AmmenTravelApplicationAutoMapperProfile : Profile
{
    public AmmenTravelApplicationAutoMapperProfile()
    {
        CreateMap<DestinoTuristico, guardarDestinoDTO>();
        CreateMap<CreateUpdateDestinoDTO, DestinoTuristico>();

        CreateMap<Opinion, OpinionDto>()
            .ForMember(d => d.DestinoTuristicoId, opt => opt.MapFrom(s => s.DestinoTuristicoId));

        CreateMap<LineaListaFavorito, LineaFavoritosDTO>()
            .ForMember(d => d.DestinoTuristicoId, opt => opt.MapFrom(s => s.DestinoTuristicoId));
    }

}