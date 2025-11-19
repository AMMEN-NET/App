using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.DestinosDTO;

public class guardarDestinoDTO : AuditedEntityDto<Guid>
{
    public required string Nombre { get; set; }

    public required string Pais { get; set; }

    public int Poblacion { get; set; }

    public  required string FotoURL { get; set; }
    public required float Latitud { get; set; }
    public required float Longitud { get; set; }
}
