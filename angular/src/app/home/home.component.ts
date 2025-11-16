import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms'; // Necesario para ngModel
import { CommonModule } from '@angular/common'; // Necesario para @if, @for

// Importa el servicio y los DTOs
// Se corrige la ruta de importación para usar los barrel files (index.ts)
// de las carpetas proxy, que es la forma estándar en ABP.
import { DestinoService } from '../proxy/destinos';
import { CiudadBuscadaDTO, CiudadResultadoDTO } from '../proxy/external-service/models';

@Component({
  selector: 'app-home',
  standalone: true,
  // Importa los módulos necesarios para la plantilla
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
}) // <-- Se agrega el cierre del decorador que faltaba
export class HomeComponent {
  // Inyecta el servicio
  private readonly destinoService = inject(DestinoService);

  // Señales para manejar el estado de la búsqueda
  public terminoBusqueda = signal<string>('');
  public ciudades = signal<CiudadResultadoDTO | null>(null);
  public estaCargando = signal<boolean>(false);
  public error = signal<string | null>(null);

  /**
   * Llama al servicio para buscar ciudades
   */
  public buscarCiudades(): void {
    const termino = this.terminoBusqueda();
    if (!termino) {
      this.ciudades.set(null); // Limpia resultados si no hay término
      return;
    }

    this.estaCargando.set(true);
    this.error.set(null);
    this.ciudades.set(null);

    // Prepara el DTO de solicitud
    const request: CiudadBuscadaDTO = { nombre: termino };

    // Llama al método del servicio
    this.destinoService.buscarCiudades(request).subscribe({
      next: (resultado) => {
        // Éxito: actualiza la señal de ciudades
        this.ciudades.set(resultado);
        this.estaCargando.set(false);
      },
      error: (err) => {
        // Error: maneja el error
        console.error('Error al buscar ciudades:', err);
        this.error.set('No se pudieron cargar los resultados. Intente más tarde.');
        this.estaCargando.set(false);
      },
    });
  }
}