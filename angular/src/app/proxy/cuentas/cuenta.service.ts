import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class CuentaService {
  apiName = 'Default';
  

  eliminarMiCuenta = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/cuenta/eliminar-mi-cuenta',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
