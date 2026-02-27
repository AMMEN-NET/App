
export interface CiudadBuscadaDTO {
  nombre: string;
  pais?: string;
  poblacionMinima?: number;
}

export interface CiudadDTO {
  nombre: string;
  pais?: string;
  poblacion: number;
  latitud: number;
  longitud: number;
  geoDBId?: string;
  promedioPuntuacion?: number;
  cantidadOpiniones?: number;
}

export interface CiudadResultadoDTO {
  ciudades: CiudadDTO[];
}

export interface EventoTicketmasterDto {
  id?: string;
  nombre?: string;
  urlTicket?: string;
  fechaInicio?: string;
  imagenUrl?: string;
}
