import { RestService, Rest } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { CiudadDTO } from '../external-service/models';
import type { FavoritoDto } from '../favoritos/favoritos-dto/models';
import type { ListaFavorito } from '../lista-favoritos/models';

@Injectable({
  providedIn: 'root',
})
export class ListaDeFavoritosService {
  apiName = 'Default';
  

  agregarAFavoritos = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/lista-de-favoritos/agregar-aFavoritos/${destinoId}`,
    },
    { apiName: this.apiName,...config });
  

  agregarFavoritoDesdeBusqueda = (ciudadExterna: CiudadDTO, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/lista-de-favoritos/agregar-favorito-desde-busqueda',
      body: ciudadExterna,
    },
    { apiName: this.apiName,...config });
  

  contarFavoritos = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, number>({
      method: 'POST',
      url: '/api/app/lista-de-favoritos/contar-favoritos',
    },
    { apiName: this.apiName,...config });
  

  eliminarDeFavoritos = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: `/api/app/lista-de-favoritos/eliminar-de-favoritos/${destinoId}`,
    },
    { apiName: this.apiName,...config });
  

  esFavorito = (destinoId: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, boolean>({
      method: 'POST',
      url: `/api/app/lista-de-favoritos/es-favorito/${destinoId}`,
    },
    { apiName: this.apiName,...config });
  

  getOrCreateLista = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, ListaFavorito>({
      method: 'GET',
      url: '/api/app/lista-de-favoritos/or-create-lista',
    },
    { apiName: this.apiName,...config });
  

  obtenerFavoritos = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, FavoritoDto[]>({
      method: 'POST',
      url: '/api/app/lista-de-favoritos/obtener-favoritos',
    },
    { apiName: this.apiName,...config });
  

  vaciarFavoritos = (config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'POST',
      url: '/api/app/lista-de-favoritos/vaciar-favoritos',
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
