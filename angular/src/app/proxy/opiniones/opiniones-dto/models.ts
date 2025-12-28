import type { ValorPuntuacion } from '../valor-puntuacion.enum';
import type { EntityDto } from '@abp/ng.core';

export interface OpinionDto {
  id?: string;
  destinoTuristicoId?: string;
  userId?: string;
  puntuacion?: ValorPuntuacion;
  comentario?: string;
  creationTime?: string;
}

export interface OpinionPublicaDto extends EntityDto<string> {
  nombreUsuario?: string;
  puntuacion: number;
  comentario?: string;
  creationTime?: string;
}

export interface createUpdateOpinionDto {
  destinoTuristicoId: string;
  puntuacion: ValorPuntuacion;
  comentario: string;
}
