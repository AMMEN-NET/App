using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.DestinosDTO;

public class destinoDTO : AuditedEntityDto<Guid>
{
    public string nombre { get; set; }

    public string pais { get; set; }

    public int poblacion { get; set; }

    public string fotoURL { get; set; }
}
