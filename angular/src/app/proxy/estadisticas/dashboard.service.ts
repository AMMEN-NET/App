import type { DashboardResumenDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  apiName = 'Default';
  

  getResumen = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, DashboardResumenDto>({
      method: 'GET',
      url: '/api/app/dashboard/resumen',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
