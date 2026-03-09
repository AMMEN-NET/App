
export interface NotificacionDto {
  id?: string;
  titulo?: string;
  mensaje?: string;
  leida: boolean;
  icono?: string;
  linkReferencia?: string;
  fecha?: string;
  color?: string;
}

export enum FrecuenciaNotificacion {
  Inmediata = 0,
  ResumenSemanal = 1,
}

export interface PreferenciasNotificacionDto {
  id?: string;
  enPantalla: boolean;
  porEmail: boolean;
  frecuencia: FrecuenciaNotificacion;
}

export interface UpdatePreferenciasDto {
  enPantalla: boolean;
  porEmail: boolean;
  frecuencia: FrecuenciaNotificacion;
}
