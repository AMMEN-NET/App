using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace AmmenTravel.Estadisticas
{
    // CreationAuditedEntity guarda automáticamente: Id, CreationTime y CreatorId
    public class HistorialBusqueda : CreationAuditedEntity<Guid>
    {
        public string TerminoBusqueda { get; set; }
        public bool EncontroResultados { get; set; }

        // Opcional: Para saber qué filtros aplicaron (ej: "minPoblacion=100000")
        public string FiltrosUtilizados { get; set; }

        protected HistorialBusqueda() { }

        public HistorialBusqueda(Guid id, string terminoBusqueda, bool encontroResultados, string filtrosUtilizados = null)
            : base(id)
        {
            TerminoBusqueda = terminoBusqueda;
            EncontroResultados = encontroResultados;
            FiltrosUtilizados = filtrosUtilizados;
        }
    }
}