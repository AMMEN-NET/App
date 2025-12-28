import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToasterService } from '@abp/ng.theme.shared';
import { ConfigStateService } from '@abp/ng.core'; 

// PROXIES EXISTENTES
import { DestinoService } from '../proxy/destinos';
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos'; 
import { CiudadBuscadaDTO, CiudadResultadoDTO, CiudadDTO } from '../proxy/external-service/models';

// --- NUEVOS IMPORTS PARA LA CALIFICACIÓN ---
// Ajusta las rutas si tus archivos están en carpetas diferentes
import { OpinionService } from '../proxy/opiniones/opinion.service'; 
import { ValorPuntuacion } from '../proxy/opiniones/valor-puntuacion.enum';
import { createUpdateOpinionDto } from '../proxy/opiniones/opiniones-dto/models';
import { OpinionPublicaDto } from '../proxy/opiniones/opiniones-dto/models'; // Ajusta la ruta según tu generación

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
  private readonly opinionService = inject(OpinionService); // <--- Inyectamos el servicio de opiniones
  private configState = inject(ConfigStateService);

  // --- SEÑALES DE INPUTS (Búsqueda) ---
  public nombre = signal<string>('');
  public pais = signal<string>('');
  public poblacion = signal<number | null>(null);

  // --- SEÑALES DE ESTADO (Resultados) ---
  public ciudades = signal<CiudadResultadoDTO | null>(null);
  public estaCargando = signal<boolean>(false);
  public error = signal<string | null>(null);

  // --- SEÑAL PARA CONTROLAR QUÉ BOTONES ESTÁN CARGANDO (Favoritos) ---
  public favoritosEnProceso = signal<Set<string>>(new Set());

  // --- NUEVAS SEÑALES PARA LA MODAL DE CALIFICACIÓN ---
  public ciudadParaCalificar = signal<CiudadDTO | null>(null); // Controla si la modal se ve
  public ratingSeleccionado = signal<number>(0);               // Almacena las estrellas (1-5)
  public comentarioCalificacion = signal<string>('');          // Almacena el texto
  public enviandoCalificacion = signal<boolean>(false);        // Loading del botón Enviar
  public opinionesDelDestino = signal<OpinionPublicaDto[]>([]); // Opiniones cargadas
  public cargandoOpiniones = signal<boolean>(false);            // Loading de opiniones

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

  // =========================================================
  // LÓGICA DE LA MODAL DE CALIFICACIÓN
  // =========================================================

  /**
   * Abre la modal para la ciudad seleccionada y resetea el formulario
   */
  public abrirModalCalificar(ciudad: CiudadDTO): void {
    this.ratingSeleccionado.set(0);
    this.comentarioCalificacion.set('');
    this.ciudadParaCalificar.set(ciudad);
    
    // --- NUEVO: Cargar opiniones al abrir ---
    if (ciudad.geoDBId) {
        this.cargarOpiniones(ciudad.geoDBId);
    }
  }

  public cerrarModalCalificar(): void {
    this.ciudadParaCalificar.set(null);
    this.enviandoCalificacion.set(false);
    this.opinionesDelDestino.set([]); // Limpiamos al cerrar
  }

  // --- NUEVO MÉTODO PRIVADO ---
  private cargarOpiniones(idExterno: string): void {
      this.cargandoOpiniones.set(true);
      
      // Llamamos al nuevo endpoint (asegúrate de haber regenerado proxies o agrégalo a tu servicio manual)
      this.opinionService.obtenerListaPublicaPorDestino(idExterno).subscribe({
          next: (lista) => {
              this.opinionesDelDestino.set(lista);
              this.cargandoOpiniones.set(false);
          },
          error: (err) => {
              console.error('Error cargando opiniones', err);
              this.cargandoOpiniones.set(false);
          }
      });
  }

  /**
   * Envía la calificación al backend
   */
  public enviarCalificacion(): void {
    const ciudad = this.ciudadParaCalificar();
    const rating = this.ratingSeleccionado();
    const comentario = this.comentarioCalificacion();

    // Validaciones básicas del frontend
    if (!ciudad || !ciudad.geoDBId) {
      this.toaster.error('No se pudo identificar la ciudad.', 'Error');
      return;
    }

    if (rating === 0) {
      this.toaster.warn('Debes seleccionar al menos una estrella.', 'Atención');
      return;
    }

    this.enviandoCalificacion.set(true);

    // --- OBJETO PARA EL BACKEND ---
    // Mapeamos los datos de GeoDB a lo que espera tu API de Destinos
    const destinoParaGuardar = {
      nombre: ciudad.nombre,
      pais: ciudad.pais,
      idExterno: ciudad.geoDBId, // Importante: Mapeamos geoDBId a idExterno
      poblacion: ciudad.poblacion,
      latitud: ciudad.latitud,
      longitud: ciudad.longitud
    };

    // 1. Primero intentamos registrar el destino en tu BD
    this.destinoService.create(destinoParaGuardar as any).subscribe({
      next: (destinoGuardado: any) => {
        
        // 2. Si se guardó (o ya existía y devolvió el objeto), usamos su ID real (GUID)
        const guidReal = destinoGuardado.id; 

        const input: createUpdateOpinionDto = {
          destinoTuristicoId: guidReal, 
          puntuacion: rating as ValorPuntuacion,
          comentario: comentario || ''
        };

        // 3. Guardamos la opinión vinculada a ese GUID
        this.opinionService.crearOpinion(input).subscribe({
          next: () => {
            this.toaster.success('¡Gracias por tu opinión!', 'Enviado');
            this.cerrarModalCalificar();
            this.enviandoCalificacion.set(false);
          },
          error: (errOpinion) => {
            console.error('Error al guardar opinión:', errOpinion);
            this.toaster.error('Error al guardar la opinión.', 'Error');
            this.enviandoCalificacion.set(false);
          }
        });
      },
      error: (errDestino) => {
        console.error('Error al crear destino:', errDestino);
        
        // Si el error es 409 (Conflict), significa que la ciudad ya existe.
        // En ese caso, la lógica ideal sería "Si falla, busca el destino por IdExterno y usa ese ID".
        // Pero si tu backend devuelve el objeto incluso en error, o si necesitas esa lógica extra, avísame.
        this.toaster.error(
            'No se pudo procesar el destino en la base de datos.', 
            'Error'
        );
        this.enviandoCalificacion.set(false);
      }
    });
  }

  
  }