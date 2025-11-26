import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

// Importa el servicio y los DTOs
import { DestinoService } from '../proxy/destinos';
// Asegúrate de que CiudadBuscadaDTO tenga las propiedades 'pais' y 'poblacion' (o 'poblacionMinima')
import { CiudadBuscadaDTO, CiudadResultadoDTO } from '../proxy/external-service/models';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  // Inyecta el servicio
  private readonly destinoService = inject(DestinoService);

  // --- SEÑALES DE ESTADO (INPUTS) ---
  // Hemos separado 'terminoBusqueda' en 3 variables para coincidir con tus 3 inputs
  public nombre = signal<string>('');
  public pais = signal<string>('');
  public poblacion = signal<number | null>(null);

  // --- SEÑALES DE RESULTADO ---
  public ciudades = signal<CiudadResultadoDTO | null>(null);
  public estaCargando = signal<boolean>(false);
  public error = signal<string | null>(null);

  /**
   * Llama al servicio para buscar ciudades aplicando los filtros
   */
  public buscarCiudades(): void {
    // Obtenemos los valores actuales de las señales
    const nombreVal = this.nombre();
    const paisVal = this.pais();
    const poblacionVal = this.poblacion();

    // Validamos que al menos haya un campo con datos para buscar
    if (!nombreVal && !paisVal && !poblacionVal) {
      this.ciudades.set(null); 
      return;
    }

    this.estaCargando.set(true);
    this.error.set(null);
    this.ciudades.set(null);

    // Prepara el DTO de solicitud con los 3 campos
    const request: CiudadBuscadaDTO = { 
      nombre: nombreVal || undefined, // Enviamos undefined si está vacío
      pais: paisVal || undefined,
      poblacionMinima: poblacionVal || undefined // Asumiendo que el DTO se llama así
    };

    // Llama al método del servicio
    this.destinoService.buscarCiudades(request).subscribe({
      next: (resultado) => {
        this.ciudades.set(resultado);
        this.estaCargando.set(false);
      },
      error: (err) => {
        console.error('Error al buscar ciudades:', err);
        this.error.set('No se pudieron cargar los resultados. Intente más tarde.');
        this.estaCargando.set(false);
      },
    });
  }
}