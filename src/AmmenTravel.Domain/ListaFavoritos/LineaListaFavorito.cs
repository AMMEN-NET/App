using AmmenTravel.Destinos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.ListaFavoritos
{
    public class LineaListaFavorito : FullAuditedEntity<Guid>
    {
        public Guid ListaFavoritoId { get; set; } // foreign key
        public Guid DestinoTuristicoId { get; set; } // foreign key

        // Esto le dice a EF: "Este ID corresponde a este objeto"
        public virtual ListaFavorito ListaFavorito { get; set; }
        public virtual DestinoTuristico DestinoTuristico { get; set; }

    }
}
