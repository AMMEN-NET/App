import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class FotoPerfilService {
  private apiUrl = environment.apis.default.url;

  /**
   * Construye la URL del backend para obtener la foto de perfil de un usuario.
   *
   * @param usuarioId ID del usuario cuya foto se quiere mostrar
   * @returns URL completa al endpoint del backend (con cache-buster)
   */
  obtenerUrlFoto(usuarioId?: string): string {
    if (!usuarioId) {
      return this.getPlaceholder();
    }
    // Sin cache-buster: el browser cachéa la imagen normalmente.
    // El perfil personalizado agrega su propio ?t= al actualizar tras subir.
    return `${this.apiUrl}/api/app/foto-perfil/obtener/${usuarioId}`;
  }

  /**
   * Devuelve un SVG placeholder gris cuando no hay foto disponible.
   */
  getPlaceholder(): string {
    return 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 100 100%22%3E%3Crect fill=%22%23e5e7eb%22 width=%22100%22 height=%22100%22/%3E%3Ctext x=%2250%22 y=%2270%22 font-size=%2250%22 fill=%22%239ca3af%22 text-anchor=%22middle%22%3E👤%3C/text%3E%3C/svg%3E';
  }
}