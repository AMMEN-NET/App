using System;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.Viajeros
{
    public class PerfilPublicoDto : EntityDto<Guid>
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime FechaRegistro { get; set; } 
        public int CantidadOpiniones { get; set; }
        public int CantidadFavoritos { get; set; }
        public double PromedioPuntuacion { get; set; } 
    }
}