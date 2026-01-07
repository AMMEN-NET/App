import type { TipoExperiencia } from './tipo-experiencia.enum';
import type { FullAuditedEntityDto } from '@abp/ng.core';

export interface CreateUpdateExperienciaDto {
  destinoId: string;
  valoracion: TipoExperiencia;
  comentario: string;
}

export interface ExperienciaDto extends FullAuditedEntityDto<string> {
  destinoId?: string;
  valoracion?: TipoExperiencia;
  comentario?: string;
  userName?: string;
}
