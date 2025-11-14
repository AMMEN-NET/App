
export interface CiudadBuscadaDTO {
  nombre: string;
}

export interface CiudadDTO {
  nombre: string;
  pais?: string;
}

export interface CiudadResultadoDTO {
  ciudades: CiudadDTO[];
}
