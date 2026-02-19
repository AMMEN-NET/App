import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FotoPerfilService {
  private localStorageKey = 'foto-perfil-usuario';

  /**
   * Carga un archivo local y lo convierte a base64, lo guarda en localStorage
   * 
   * @param archivo Archivo seleccionado por el usuario
   * @returns Observable que emite cuando se completa la lectura
   */
  subirFoto(archivo: File): Observable<string> {
    return new Observable((observer) => {
      // Validar que sea una imagen
      if (!archivo.type.startsWith('image/')) {
        observer.error(new Error('El archivo debe ser una imagen'));
        return;
      }

      // Validar tamaño (máximo 5MB)
      const maxSize = 5 * 1024 * 1024; // 5MB
      if (archivo.size > maxSize) {
        observer.error(new Error('El archivo no debe superar 5MB'));
        return;
      }

      // Leer archivo y convertir a base64
      const reader = new FileReader();
      reader.onload = (event: any) => {
        const base64String = event.target.result;
        // Guardar en localStorage
        localStorage.setItem(this.localStorageKey, base64String);
        observer.next(base64String);
        observer.complete();
      };

      reader.onerror = () => {
        observer.error(new Error('Error al leer el archivo'));
      };

      reader.readAsDataURL(archivo);
    });
  }

  /**
   * Obtiene la foto de perfil desde localStorage o devuelve placeholder
   * 
   * @param usuarioId ID del usuario (opcional, no se usa en localStorage)
   * @returns URL en base64 o placeholder gris
   */
  obtenerUrlFoto(usuarioId?: string): string {
    const fotoGuardada = localStorage.getItem(this.localStorageKey);
    
    if (fotoGuardada) {
      return fotoGuardada;
    }

    // Placeholder gris si no hay foto
    return 'data:image/svg+xml,%3Csvg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 100 100%22%3E%3Crect fill=%22%23e5e7eb%22 width=%22100%22 height=%22100%22/%3E%3Ctext x=%2250%22 y=%2270%22 font-size=%2250%22 fill=%22%239ca3af%22 text-anchor=%22middle%22%3E👤%3C/text%3E%3C/svg%3E';
  }

  /**
   * Elimina la foto guardada en localStorage
   */
  eliminarFoto(): void {
    localStorage.removeItem(this.localStorageKey);
  }

  /**
   * Comprueba si hay foto guardada
   */
  tieneFoto(): boolean {
    return !!localStorage.getItem(this.localStorageKey);
  }
}