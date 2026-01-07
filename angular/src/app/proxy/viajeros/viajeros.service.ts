import type { PerfilPublicoDto, ViajeroDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class ViajerosService {
  apiName = 'Default';
  

  getList = (filtro?: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ViajeroDto[]>({
      method: 'GET',
      url: '/api/app/viajeros',
      params: { filtro },
    },
    { apiName: this.apiName,...config });
  

  getPerfilPublico = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PerfilPublicoDto>({
      method: 'GET',
      url: `/api/app/viajeros/${id}/perfil-publico`,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
