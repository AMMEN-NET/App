import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { IFormFile } from '../microsoft/asp-net-core/http/models';
import type { IActionResult } from '../microsoft/asp-net-core/mvc/models';

@Injectable({
  providedIn: 'root',
})
export class FotoPerfilService {
  apiName = 'Default';
  

  obtener = (usuarioId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IActionResult>({
      method: 'GET',
      url: `/api/app/foto-perfil/obtener/${usuarioId}`,
    },
    { apiName: this.apiName,...config });
  

  obtenerMiFoto = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, IActionResult>({
      method: 'GET',
      url: '/api/app/foto-perfil/mi-foto',
    },
    { apiName: this.apiName,...config });
  

  subir = (archivo: IFormFile, config?: Partial<Rest.Config>) =>
    this.restService.request<any, IActionResult>({
      method: 'POST',
      url: '/api/app/foto-perfil/subir',
      body: archivo,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
