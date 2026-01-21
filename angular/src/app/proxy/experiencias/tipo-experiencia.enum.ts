import { mapEnumToOptions } from '@abp/ng.core';

export enum TipoExperiencia {
  Malo = 0,
  Neutral = 1,
  MuyBueno = 2,
}

export const tipoExperienciaOptions = mapEnumToOptions(TipoExperiencia);
