import type { ValorPuntuacion } from '../valor-puntuacion.enum';

export interface OpinionDto {
  id?: string;
  destinoTuristicoId?: string;
  userId?: string;
  puntuacion?: ValorPuntuacion;
  comentario?: string;
  creationTime?: string;
}

export interface createUpdateOpinionDto {
  destinoTuristicoId: string;
  puntuacion: ValorPuntuacion;
  comentario: string;
}
