
export interface DashboardResumenDto {
  totalBusquedas: number;
  totalDestinosGuardados: number;
  totalOpiniones: number;
  totalUsuarios: number;
  totalExperiencias: number;
  totalLlamadasApi: number;
  totalErroresApi: number;
  tiempoPromedioRespuestaMs: number;
  topBusquedas: TerminoBusquedaDto[];
}

export interface TerminoBusquedaDto {
  termino?: string;
  cantidad: number;
}
