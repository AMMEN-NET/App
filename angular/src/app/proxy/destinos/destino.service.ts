import { RestService, Rest } from '@abp/ng.core';
import type { PagedAndSortedResultRequestDto, PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CreateUpdateDestinoDTO, guardarDestinoDTO } from '../destinos-dto/models';
import type { CiudadBuscadaDTO, CiudadResultadoDTO } from '../external-service/models';

@Injectable({
  providedIn: 'root',
})
export class DestinoService {
  apiName = 'Default';
  

  buscarCiudades = (request: CiudadBuscadaDTO, config?: Partial<Rest.Config>) =>
    this.restService.request<any, CiudadResultadoDTO>({
      method: 'POST',
      url: '/api/app/destino/buscar-ciudades',
      body: request,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CreateUpdateDestinoDTO, config?: Partial<Rest.Config>) =>
    this.restService.request<any, guardarDestinoDTO>({
      method: 'POST',
      url: '/api/app/destino',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/destino/${id}`,
    },
    { apiName: this.apiName,...config });
  

  get = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, guardarDestinoDTO>({
      method: 'GET',
      url: `/api/app/destino/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: PagedAndSortedResultRequestDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<guardarDestinoDTO>>({
      method: 'GET',
      url: '/api/app/destino',
      params: { sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (id: string, input: CreateUpdateDestinoDTO, config?: Partial<Rest.Config>) =>
    this.restService.request<any, guardarDestinoDTO>({
      method: 'PUT',
      url: `/api/app/destino/${id}`,
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
