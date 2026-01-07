import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ToasterService } from '@abp/ng.theme.shared';
import { ConfigStateService } from '@abp/ng.core'; 

// PROXIES EXISTENTES
import { DestinoService } from '../proxy/destinos';
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos'; 
import { CiudadBuscadaDTO, CiudadResultadoDTO, CiudadDTO } from '../proxy/external-service/models';

// OPINIONES
import { OpinionService } from '../proxy/opiniones/opinion.service'; 
import { ValorPuntuacion } from '../proxy/opiniones/valor-puntuacion.enum';
import { createUpdateOpinionDto } from '../proxy/opiniones/opiniones-dto/models';
import { OpinionPublicaDto } from '../proxy/opiniones/opiniones-dto/models'; 

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit {

  protected Math = Math;

  // --- INYECCIÓN DE SERVICIOS ---
  private readonly destinoService = inject(DestinoService);
  private readonly toaster = inject(ToasterService);
  private readonly listaFavoritosService = inject(ListaDeFavoritosService);
  private readonly opinionService = inject(OpinionService); 
  private configState = inject(ConfigStateService);

  // --- SEÑALES DE INPUTS (Búsqueda) ---
  public nombre = signal<string>('');
  public pais = signal<string>('');
  public poblacion = signal<number | null>(null);

  // --- SEÑALES DE ESTADO (Resultados) ---
  public ciudades = signal<CiudadResultadoDTO | null>(null);
  public estaCargando = signal<boolean>(false);
  public error = signal<string | null>(null);

  // --- SEÑAL FAVORITOS ---
  public favoritosEnProceso = signal<Set<string>>(new Set());
  // Almacena los IDs de las ciudades que YA son favoritas para pintar el corazón
  public favoritosIds = signal<Set<string>>(new Set());

  // --- SEÑALES MODAL OPINIÓN ---
  public ciudadParaCalificar = signal<CiudadDTO | null>(null); 
  public ratingSeleccionado = signal<number>(0);               
  public comentarioCalificacion = signal<string>('');          
  public enviandoCalificacion = signal<boolean>(false);        
  public opinionesDelDestino = signal<OpinionPublicaDto[]>([]); 
  public cargandoOpiniones = signal<boolean>(false);            

  get userName(): string {
    const currentUser = this.configState.getOne('currentUser');
    return currentUser?.name || currentUser?.userName || 'Viajero';
  }

  ngOnInit() {
    // Al iniciar, cargamos los favoritos que el usuario ya tiene
    this.cargarFavoritosExistentes();
  }

  // --- LÓGICA DE CARGA INICIAL DE FAVORITOS (CORREGIDA) ---
  private cargarFavoritosExistentes() {
    this.listaFavoritosService.obtenerFavoritos().subscribe({
      next: (resultado: any[]) => {
        // Aseguramos que sea un array
        const lista = Array.isArray(resultado) ? resultado : (resultado['items'] || []);
        
        const ids = new Set<string>();
        
        lista.forEach((item: any) => {
             // BUSCAMOS EL ID EXTERNO (GeoDB) EN TODAS PARTES
             // El backend puede devolverlo directo, o dentro de 'destino' o 'destinoTuristico'
             const idExterno = item.idExterno 
                            || item.destino?.idExterno 
                            || item.destinoTuristico?.idExterno;

             if (idExterno) {
                ids.add(idExterno);
             }
        });

        this.favoritosIds.set(ids);
        console.log(`💙 Favoritos cargados al inicio: ${ids.size}`, ids);
      },
      error: (err) => {
        console.log('No se pudieron cargar favoritos iniciales (Usuario anónimo o error API).');
      }
    });
  }

  // Helper para el HTML: ¿Esta ciudad ya es favorita?
  public esFavorito(geoDBId: string | undefined): boolean {
    if (!geoDBId) return false;
    return this.favoritosIds().has(geoDBId);
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

    // Si ya es favorito, avisamos y no hacemos nada
    if (this.esFavorito(ciudadId)) {
        this.toaster.info('Esta ciudad ya está en tu lista de deseos.', 'Información');
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
        
        // Actualizamos localmente para que se pinte rojo al instante
        this.favoritosIds.update(ids => {
            const nuevosIds = new Set(ids);
            nuevosIds.add(ciudadId);
            return nuevosIds;
        });
      },
      error: (error) => {
        this.toaster.error('No se pudo guardar. ¿Estás logueado?', 'Error');
        console.error(error);
      },
      // 2. Al finalizar (sea éxito o error), quitamos el ID del Set de carga
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
    
    // Cargar opiniones al abrir
    if (ciudad.geoDBId) {
        this.cargarOpiniones(ciudad.geoDBId);
    }
  }

  public cerrarModalCalificar(): void {
    this.ciudadParaCalificar.set(null);
    this.enviandoCalificacion.set(false);
    this.opinionesDelDestino.set([]); // Limpiamos al cerrar
  }

  // Método privado para cargar opiniones
  private cargarOpiniones(idExterno: string): void {
      this.cargandoOpiniones.set(true);
      
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
    const destinoParaGuardar = {
      nombre: ciudad.nombre,
      pais: ciudad.pais,
      idExterno: ciudad.geoDBId, 
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
        next: (opinionCreada) => { 
            const fechaCreacion = new Date(opinionCreada.creationTime || new Date());
            const ahora = new Date();
            const diferenciaMilisegundos = ahora.getTime() - fechaCreacion.getTime();
            
            if (diferenciaMilisegundos > 60000) { 
                 this.toaster.info(
                    'Hemos detectado una calificación previa. Se ha restaurado y actualizado correctamente.', 
                    'Calificación Restaurada'
                  );
            } else {
                 this.toaster.success('¡Gracias por tu opinión!', 'Enviado');
            }

            this.cerrarModalCalificar();
            this.enviandoCalificacion.set(false);
          },
          error: (errOpinion) => {
            console.error('Error al guardar opinión:', errOpinion);
            
            if (errOpinion.error?.error?.message) {
                 this.toaster.warn(errOpinion.error.error.message, 'Atención');
            } else {
                 this.toaster.error('Error al guardar la opinión.', 'Error');
            }
            
            this.enviandoCalificacion.set(false);
          }
        });
      },
      error: (errDestino) => {
        console.error('Error al crear destino:', errDestino);
        
        this.toaster.error(
            'No se pudo procesar el destino en la base de datos.', 
            'Error'
        );
        this.enviandoCalificacion.set(false);
      }
    });
  }

  public crearExperiencia(ciudad: CiudadDTO): void {
      // 1. Guardar el destino en base de datos si no existe
      // 2. Abrir la modal de app-experiencias
      console.log('Abriendo modal para crear experiencia en:', ciudad.nombre);
      this.toaster.info('¡Próximamente! Aquí podrás crear tu experiencia detallada.', 'En construcción');
      
      // TODO: Conectar con el componente ExperienciasComponent
  }

}