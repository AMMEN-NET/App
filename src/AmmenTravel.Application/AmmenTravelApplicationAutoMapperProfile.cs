using AmmenTravel.Destinos;
using AmmenTravel.DestinosDTO;
using AmmenTravel.Experiencias;
using AmmenTravel.ExternalService;
using AmmenTravel.Favoritos.FavoritosDTO;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones;
using AmmenTravel.Opiniones.OpinionesDTO;
using AutoMapper;

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

        // Maps de Experiencias (FALTABAN ESTOS)
        CreateMap<Experiencia, ExperienciaDto>()
            .ForMember(x => x.DestinoNombre, opt => opt.MapFrom(src => src.Destino.Nombre)); // Mapeo automático del nombre

        CreateMap<CreateUpdateExperienciaDto, Experiencia>();
    }

}