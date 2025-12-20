import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToasterService } from '@abp/ng.theme.shared';

// PROXIES
import { DestinoService } from '../proxy/destinos';
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos'; 
import { CiudadBuscadaDTO, CiudadResultadoDTO, CiudadDTO } from '../proxy/external-service/models';
import { ConfigStateService } from '@abp/ng.core'; 

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent {
  // --- INYECCIÓN DE SERVICIOS ---
  private readonly destinoService = inject(DestinoService);
  private readonly toaster = inject(ToasterService);
  private readonly listaFavoritosService = inject(ListaDeFavoritosService);
  private configState = inject(ConfigStateService);

  // --- SEÑALES DE INPUTS (Búsqueda) ---
  public nombre = signal<string>('');
  public pais = signal<string>('');
  public poblacion = signal<number | null>(null);

  // --- SEÑALES DE ESTADO (Resultados) ---
  public ciudades = signal<CiudadResultadoDTO | null>(null);
  public estaCargando = signal<boolean>(false);
  public error = signal<string | null>(null);

  // --- SEÑAL PARA CONTROLAR QUÉ BOTONES ESTÁN CARGANDO ---
  public favoritosEnProceso = signal<Set<string>>(new Set());

  get userName(): string {
    // Busca el usuario actual en el estado de ABP
    const currentUser = this.configState.getOne('currentUser');
    
    // Si existe y tiene nombre, lo devuelve. Si no, devuelve 'Viajero'
    return currentUser?.name || currentUser?.userName || 'Viajero';
  }

  /**
   * Llama al servicio para buscar ciudades
   */
  public buscarCiudades(): void {
    const nombreVal = this.nombre();
    const paisVal = this.pais();
    const poblacionVal = this.poblacion();

    if (!nombreVal && !paisVal && !poblacionVal) {
      this.ciudades.set(null); 
      return;
    }

    this.estaCargando.set(true);
    this.error.set(null);
    this.ciudades.set(null);

    const request: CiudadBuscadaDTO = { 
      nombre: nombreVal || undefined, 
      pais: paisVal || undefined,
      poblacionMinima: poblacionVal || undefined
    };

    this.destinoService.buscarCiudades(request).subscribe({
      next: (resultado) => {
        this.ciudades.set(resultado);
        this.estaCargando.set(false);
      },
      error: (err) => {
        console.error('Error al buscar:', err);
        this.error.set('No se pudieron cargar los resultados.');
        this.estaCargando.set(false);
      },
    });
  }

  /**
   * Guarda la ciudad en favoritos
   */
  public agregarAFavoritos(ciudad: CiudadDTO): void {
    const ciudadId = ciudad.geoDBId; 
    
    if (!ciudadId) {
        this.toaster.error('Error: La ciudad no tiene ID válido.', 'Error');
        return;
    }

    // 1. Añadimos el ID al Set para mostrar el spinner en el botón
    this.favoritosEnProceso.update(set => {
        const newSet = new Set(set);
        newSet.add(ciudadId);
        return newSet;
    });

    this.listaFavoritosService.agregarFavoritoDesdeBusqueda(ciudad).subscribe({
      next: () => {
        this.toaster.success(`¡"${ciudad.nombre}" agregado a favoritos!`, 'Éxito');
      },
      error: (error) => {
        this.toaster.error('No se pudo guardar. ¿Estás logueado?', 'Error');
        console.error(error);
      },
      // 2. Al finalizar (sea éxito o error), quitamos el ID del Set
      complete: () => {
        this.favoritosEnProceso.update(set => {
            const newSet = new Set(set);
            newSet.delete(ciudadId);
            return newSet;
        });
      }
    });
  }

  /**
   * Helper para el HTML: verifica si una ciudad específica se está guardando
   */
  public estaAgregando(geoDBId: string | undefined): boolean {
      if (!geoDBId) return false;
      return this.favoritosEnProceso().has(geoDBId);
  }
}