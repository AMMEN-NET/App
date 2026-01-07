import type { AuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateDestinoDTO {
  nombre: string;
  pais: string;
  poblacion: number;
  latitud: number;
  longitud: number;
  idExterno: string;
}

export interface guardarDestinoDTO extends AuditedEntityDto<string> {
  nombre: string;
  pais: string;
  poblacion: number;
  latitud: number;
  longitud: number;
}
