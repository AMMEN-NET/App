import { ChangeDetectionStrategy, Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
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

// EXPERIENCIAS (NUEVO)
import { ExperienciaService } from '../proxy/experiencias/experiencia.service';
import { TipoExperiencia } from '../proxy/experiencias/tipo-experiencia.enum';
import { CreateUpdateExperienciaDto } from '../proxy/experiencias/models'; 

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent implements OnInit {

  protected Math = Math;
  // Enum para usar en el HTML de la modal de experiencia
  TipoExperiencia = TipoExperiencia; 

  // --- INYECCIÓN DE SERVICIOS ---
  private readonly destinoService = inject(DestinoService);
  private readonly toaster = inject(ToasterService);
  private readonly listaFavoritosService = inject(ListaDeFavoritosService);
  private readonly opinionService = inject(OpinionService); 
  private readonly experienciaService = inject(ExperienciaService); // Inyectado
  private readonly configState = inject(ConfigStateService);
  private readonly fb = inject(FormBuilder); // Inyectado para el formulario

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
  public favoritosIds = signal<Set<string>>(new Set());

  // --- SEÑALES MODAL OPINIÓN (CALIFICAR) ---
  public ciudadParaCalificar = signal<CiudadDTO | null>(null); 
  public ratingSeleccionado = signal<number>(0);               
  public comentarioCalificacion = signal<string>('');          
  public enviandoCalificacion = signal<boolean>(false);        
  public opinionesDelDestino = signal<OpinionPublicaDto[]>([]); 
  public cargandoOpiniones = signal<boolean>(false);            

  // --- SEÑALES MODAL EXPERIENCIA (LO QUE TE FALTABA) ---
  public ciudadExperiencia = signal<CiudadDTO | null>(null); 
  public enviandoExperiencia = signal<boolean>(false);
  public formExperiencia: FormGroup;

  constructor() {
    // Inicializamos el formulario de experiencia
    this.formExperiencia = this.fb.group({
      valoracion: [null, Validators.required],
      comentario: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]]
    });
  }

  get userName(): string {
    const currentUser = this.configState.getOne('currentUser');
    return currentUser?.name || currentUser?.userName || 'Viajero';
  }

  ngOnInit() {
    this.cargarFavoritosExistentes();
  }

  // --- LÓGICA DE CARGA INICIAL DE FAVORITOS ---
  private cargarFavoritosExistentes() {
    this.listaFavoritosService.obtenerFavoritos().subscribe({
      next: (resultado: any[]) => {
        const lista = Array.isArray(resultado) ? resultado : (resultado['items'] || []);
        
        const ids = new Set<string>();
        lista.forEach((item: any) => {
             const idExterno = item.geoDBId || item.geoDbId || item.idExterno || item.destino?.idExterno || item.destinoTuristico?.idExterno;
             if (idExterno) ids.add(idExterno);
        });

        this.favoritosIds.set(ids);
        console.log(`💙 Favoritos cargados al inicio: ${ids.size}`, ids);
      },
      error: (err) => {
        console.log('No se pudieron cargar favoritos iniciales.');
      }
    });
  }

  public esFavorito(geoDBId: string | undefined): boolean {
    if (!geoDBId) return false;
    return this.favoritosIds().has(geoDBId);
  }

  // --- BÚSQUEDA ---
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

  // --- FAVORITOS ---
  public agregarAFavoritos(ciudad: CiudadDTO): void {
    const ciudadId = ciudad.geoDBId; 
    
    if (!ciudadId) {
        this.toaster.error('Error: La ciudad no tiene ID válido.', 'Error');
        return;
    }

    if (this.esFavorito(ciudadId)) {
        this.toaster.info('Esta ciudad ya está en tu lista de deseos.', 'Información');
        return;
    }

    this.favoritosEnProceso.update(set => {
        const newSet = new Set(set);
        newSet.add(ciudadId);
        return newSet;
    });

    this.listaFavoritosService.agregarFavoritoDesdeBusqueda(ciudad).subscribe({
      next: () => {
        this.toaster.success(`¡"${ciudad.nombre}" agregado a favoritos!`, 'Éxito');
        
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
      complete: () => {
        this.favoritosEnProceso.update(set => {
            const newSet = new Set(set);
            newSet.delete(ciudadId);
            return newSet;
        });
      }
    });
  }

  public estaAgregando(geoDBId: string | undefined): boolean {
      if (!geoDBId) return false;
      return this.favoritosEnProceso().has(geoDBId);
  }

  // --- MODAL CALIFICAR (OPINIÓN SIMPLE) ---
  public abrirModalCalificar(ciudad: CiudadDTO): void {
    this.ratingSeleccionado.set(0);
    this.comentarioCalificacion.set('');
    this.ciudadParaCalificar.set(ciudad);
    
    if (ciudad.geoDBId) {
        this.cargarOpiniones(ciudad.geoDBId);
    }
  }

  public cerrarModalCalificar(): void {
    this.ciudadParaCalificar.set(null);
    this.enviandoCalificacion.set(false);
    this.opinionesDelDestino.set([]); 
  }

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

  public enviarCalificacion(): void {
    const ciudad = this.ciudadParaCalificar();
    const rating = this.ratingSeleccionado();
    const comentario = this.comentarioCalificacion();

    if (!ciudad || !ciudad.geoDBId) return;

    if (rating === 0) {
      this.toaster.warn('Debes seleccionar al menos una estrella.', 'Atención');
      return;
    }

    this.enviandoCalificacion.set(true);

    const destinoParaGuardar = {
      nombre: ciudad.nombre,
      pais: ciudad.pais,
      idExterno: ciudad.geoDBId, 
      poblacion: ciudad.poblacion,
      latitud: ciudad.latitud,
      longitud: ciudad.longitud
    };

    this.destinoService.create(destinoParaGuardar as any).subscribe({
      next: (destinoGuardado: any) => {
        const guidReal = destinoGuardado.id; 
        const input: createUpdateOpinionDto = {
          destinoTuristicoId: guidReal, 
          puntuacion: rating as ValorPuntuacion,
          comentario: comentario || ''
        };

        this.opinionService.crearOpinion(input).subscribe({
        next: (opinionCreada) => { 
            const fechaCreacion = new Date(opinionCreada.creationTime || new Date());
            const ahora = new Date();
            if ((ahora.getTime() - fechaCreacion.getTime()) > 60000) { 
                 this.toaster.info('Calificación actualizada.', 'Info');
            } else {
                 this.toaster.success('¡Gracias por tu opinión!', 'Enviado');
            }
            this.cerrarModalCalificar();
            this.enviandoCalificacion.set(false);
          },
          error: (errOpinion) => {
            if (errOpinion.error?.error?.message) {
                 this.toaster.warn(errOpinion.error.error.message, 'Atención');
            } else {
                 this.toaster.error('Error al guardar.', 'Error');
            }
            this.enviandoCalificacion.set(false);
          }
        });
      },
      error: () => {
        this.toaster.error('Error al procesar destino.', 'Error');
        this.enviandoCalificacion.set(false);
      }
    });
  }

  // --- MODAL EXPERIENCIA (NUEVO MÉTODO COMPLETO) ---
  public crearExperiencia(ciudad: CiudadDTO): void {
    this.formExperiencia.reset({
      valoracion: TipoExperiencia.MuyBueno
    });
    this.ciudadExperiencia.set(ciudad);
  }

  public cerrarModalExperiencia(): void {
    this.ciudadExperiencia.set(null);
    this.enviandoExperiencia.set(false);
  }

  public guardarExperiencia(): void {
    if (this.formExperiencia.invalid) return;

    const ciudad = this.ciudadExperiencia();
    if (!ciudad || !ciudad.geoDBId) return;

    this.enviandoExperiencia.set(true);

    const destinoParaGuardar = {
      nombre: ciudad.nombre,
      pais: ciudad.pais,
      idExterno: ciudad.geoDBId,
      poblacion: ciudad.poblacion,
      latitud: ciudad.latitud,
      longitud: ciudad.longitud
    };

    this.destinoService.create(destinoParaGuardar as any).subscribe({
      next: (destinoGuardado: any) => {
        const destinoId = destinoGuardado.id;

        const input: CreateUpdateExperienciaDto = {
          destinoId: destinoId,
          valoracion: this.formExperiencia.value.valoracion,
          comentario: this.formExperiencia.value.comentario
        };

        this.experienciaService.create(input).subscribe({
          next: () => {
            this.toaster.success('¡Experiencia publicada con éxito!', 'Genial');
            this.cerrarModalExperiencia();
          },
          error: (err) => {
            console.error(err);
            this.toaster.error('Ocurrió un error al publicar.', 'Error');
            this.enviandoExperiencia.set(false);
          }
        });
      },
      error: (err) => {
        console.error('Error destino:', err);
        this.toaster.error('Error al procesar el destino.', 'Error');
        this.enviandoExperiencia.set(false);
      }
    });
  }
}