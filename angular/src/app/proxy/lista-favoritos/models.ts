import type { FullAuditedEntity } from '../volo/abp/domain/entities/auditing/models';

export interface ListaFavorito extends FullAuditedEntity<string> {
  userId?: string;
}
