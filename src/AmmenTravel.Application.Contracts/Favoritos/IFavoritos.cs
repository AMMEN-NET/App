using AmmenTravel.Common;
using AmmenTravel.ListaFavoritos;
using AmmenTravel.Opiniones.OpinionesDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Favoritos
{
    public interface IFavoritos : IApplicationService
    {

        Task<ListaFavorito> GetOrCreateListaAsync();

        Task AgregarAFavoritosAsync(Guid destinoId);

        Task EliminarDeFavoritosAsync(Guid destinoId);

        Task<IList<Guid>> ObtenerFavoritosAsync();

        Task<bool> EsFavoritoAsync(Guid destinoId);

        Task<int> ContarFavoritosAsync();

        Task VaciarFavoritosAsync();

    }
}
