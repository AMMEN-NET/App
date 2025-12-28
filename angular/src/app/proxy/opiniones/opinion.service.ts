import type { OpinionDto, createUpdateOpinionDto, OpinionPublicaDto } from './opiniones-dto/models';
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
  
  actualizarOpinion = (opinionId: string, input: createUpdateOpinionDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpinionDto>({
      method: 'PUT',
      url: `/api/app/opinion/actualizar-opinion/${opinionId}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  eliminarOpinion = (opinionId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/opinion/eliminar-opinion/${opinionId}`,
    },
    { apiName: this.apiName,...config });
  
  obtenerPorUsuario = (usuarioId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpinionDto[]>({
      method: 'POST',
      url: `/api/app/opinion/obtener-por-usuario/${usuarioId}`,
    },
    { apiName: this.apiName,...config });

  obtenerListaPublicaPorDestino = (idExternoGeoDB: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, OpinionPublicaDto[]>({
      method: 'POST',
      url: `/api/app/opinion/obtener-lista-publica-por-destino`,
      params: { idExternoGeoDB },
    },
    { apiName: this.apiName, ...config });

  esOpinionado = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, boolean>({
      method: 'POST',
      url: `/api/app/opinion/es-opinionado/${destinoId}`,
    },
    { apiName: this.apiName, ...config });

  constructor(private restService: RestService) {}
}