import type { PreferenciasNotificacionDto, UpdatePreferenciasDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class PreferenciasNotificacionService {
  apiName = 'Default';

  getMiPreferencia = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, PreferenciasNotificacionDto>({
      method: 'GET',
      url: '/api/app/preferencias-notificacion/mi-preferencia',
    },
    { apiName: this.apiName, ...config });

  updateMiPreferencia = (input: UpdatePreferenciasDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PreferenciasNotificacionDto>({
      method: 'PUT',
      url: '/api/app/preferencias-notificacion/mi-preferencia',
      body: input,
    },
    { apiName: this.apiName, ...config });

  constructor(private restService: RestService) {}
}
