import type { OpinionDto, createUpdateOpinionDto } from './opiniones-dto/models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class OpinionService {
  apiName = 'Default';
  

  crearOpinion = (input: createUpdateOpinionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpinionDto>({
      method: 'POST',
      url: '/api/app/opinion/crear-opinion',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  obtenerPorUsuario = (usuarioId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpinionDto[]>({
      method: 'POST',
      url: `/api/app/opinion/obtener-por-usuario/${usuarioId}`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
