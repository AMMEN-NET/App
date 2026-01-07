import type { CreateUpdateExperienciaDto, ExperienciaDto } from './models';
import type { TipoExperiencia } from './tipo-experiencia.enum';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ExperienciaService {
  apiName = 'Default';
  

  create = (input: CreateUpdateExperienciaDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExperienciaDto>({
      method: 'POST',
      url: '/api/app/experiencia',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/experiencia/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (destinoId: string, filtroValoracion?: TipoExperiencia, filtroTexto?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExperienciaDto[]>({
      method: 'GET',
      url: '/api/app/experiencia',
      params: { destinoId, filtroValoracion, filtroTexto },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateExperienciaDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ExperienciaDto>({
      method: 'PUT',
      url: `/api/app/experiencia/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
