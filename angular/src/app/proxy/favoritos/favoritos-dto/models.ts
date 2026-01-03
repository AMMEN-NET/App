import type { EntityDto } from '@abp/ng.core';

export interface FavoritoDto extends EntityDto<string> {
  nombre?: string;
  pais?: string;
  poblacion: number;
  latitud: number;
  longitud: number;
  geoDBId?: string;
  promedioPuntuacion?: number;
  cantidadOpiniones: number;
}
