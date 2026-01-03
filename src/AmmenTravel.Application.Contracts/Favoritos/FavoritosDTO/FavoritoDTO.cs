using System;
using Volo.Abp.Application.Dtos;

namespace AmmenTravel.Favoritos.FavoritosDTO
{
    // Este DTO es EXCLUSIVO para mostrar los datos lindos por pantalla
    public class FavoritoDto : EntityDto<Guid>
    {
        public string Nombre { get; set; }
        public string Pais { get; set; }
        public int Poblacion { get; set; }
        public float Latitud { get; set; }
        public float Longitud { get; set; }
        public string GeoDBId { get; set; }
        public double? PromedioPuntuacion { get; set; }
        public int CantidadOpiniones { get; set; }
    }
}