import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FotoPerfilService {
  private http = inject(HttpClient);
  // Esta ruta debe coincidir con el Route del Controller en C#
  private urlApi = environment.apis.default.url + '/api/app/foto-perfil';

  subirFoto(archivo: File): Observable<any> {
    const formData = new FormData();
    // 'archivo' debe coincidir con el nombre del parámetro en C# (IFormFile archivo)
    formData.append('archivo', archivo); 
    return this.http.post(`${this.urlApi}/subir`, formData);
  }

  obtenerUrlFoto(usuarioId: string): string {
    return `${this.urlApi}/obtener/${usuarioId}`;
  }
}