
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
}

export interface CiudadResultadoDTO {
  ciudades: CiudadDTO[];
}
