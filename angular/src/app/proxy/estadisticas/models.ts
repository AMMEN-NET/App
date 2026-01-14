
export interface DashboardResumenDto {
  totalBusquedas: number;
  totalDestinosGuardados: number;
  totalLlamadasApi: number;
  totalErroresApi: number;
  tiempoPromedioRespuestaMs: number;
  topBusquedas: TerminoBusquedaDto[];
}

export interface TerminoBusquedaDto {
  termino?: string;
  cantidad: number;
}
