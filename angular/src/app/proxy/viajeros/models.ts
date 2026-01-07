import type { EntityDto } from '@abp/ng.core';

export interface PerfilPublicoDto extends EntityDto<string> {
  userName?: string;
  name?: string;
  surname?: string;
  fechaRegistro?: string;
  cantidadOpiniones: number;
  cantidadFavoritos: number;
  promedioPuntuacion: number;
}

export interface ViajeroDto extends EntityDto<string> {
  userName?: string;
  name?: string;
  surname?: string;
}
