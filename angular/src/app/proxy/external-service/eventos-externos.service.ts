import type { EventoTicketmasterDto } from './models';
import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class EventosExternosService {
  apiName = 'Default';
  

  obtenerEventosPorUbicacion = (latitud: string, longitud: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, EventoTicketmasterDto[]>({
      method: 'POST',
      url: '/api/app/eventos-externos/obtener-eventos-por-ubicacion',
      params: { latitud, longitud },
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
