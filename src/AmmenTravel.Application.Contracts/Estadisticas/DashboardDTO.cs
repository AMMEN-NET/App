using System.Collections.Generic;

namespace AmmenTravel.Estadisticas
{
    public class DashboardResumenDto
    {
        // Métricas Generales
        public int TotalBusquedas { get; set; }
        public int TotalDestinosGuardados { get; set; } // Favoritos
        public int TotalOpiniones { get; set; }     // Cantidad de comentarios
        public int TotalUsuarios { get; set; }      // Cantidad de usuarios registrados
        public int TotalExperiencias { get; set; }  // Cantidad de experiencias redactadas

        // Métricas API Externa (GeoDB)
        public int TotalLlamadasApi { get; set; }
        public int TotalErroresApi { get; set; }
        public double TiempoPromedioRespuestaMs { get; set; }

        // Listas para Gráficos o Top Lists
        public List<TerminoBusquedaDto> TopBusquedas { get; set; } = new();
    }

    public class TerminoBusquedaDto
    {
        public string Termino { get; set; }
        public int Cantidad { get; set; }
    }
}