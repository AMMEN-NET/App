using AmmenTravel.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Entities.Auditing;


namespace AmmenTravel.ListaFavoritos
{
    public class ListaFavorito : FullAuditedEntity<Guid>, IUserOwned
    {
        public Guid UserId { get; set; }

    }
}
