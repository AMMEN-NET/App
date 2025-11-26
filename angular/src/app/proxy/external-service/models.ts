
export interface CiudadBuscadaDTO {
  nombre: string;
  pais?: string;
  poblacionMinima?: number;
}

export interface CiudadDTO {
  nombre: string;
  pais?: string;
}

export interface CiudadResultadoDTO {
  ciudades: CiudadDTO[];
}
