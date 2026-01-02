import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToasterService } from '@abp/ng.theme.shared';
import { ConfigStateService } from '@abp/ng.core';

// Servicios y Modelos
// Asegúrate de que la ruta al servicio sea correcta según tu estructura de carpetas
import { OpinionService } from '../proxy/opiniones/opinion.service'; 
import { OpinionDto, createUpdateOpinionDto } from '../proxy/opiniones/opiniones-dto/models';
import { ValorPuntuacion } from '../proxy/opiniones/valor-puntuacion.enum';

@Component({
  selector: 'app-mis-calificaciones',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './mis-calificaciones.html', // Asegúrate de que coincida con tu nombre de archivo real
})
export class MisCalificacionesComponent implements OnInit {
  // Inyección de dependencias
  private opinionService = inject(OpinionService);
  private toaster = inject(ToasterService);
  private configState = inject(ConfigStateService);

  // --- SEÑALES ---
  // He cambiado el tipo a OpinionDto[] para tener mejor autocompletado
  public misOpiniones = signal<OpinionDto[]>([]); 
  public estaCargando = signal<boolean>(true);
  
  // Para Edición
  public opinionEditando = signal<OpinionDto | null>(null);
  public ratingEditando = signal<number>(0);
  public comentarioEditando = signal<string>('');
  public guardandoEdicion = signal<boolean>(false);
  
  // Para Eliminación
  public eliminandoId = signal<string | null>(null);

  ngOnInit() {
    this.cargarOpiniones();
  }

  // Obtener el ID del usuario logueado desde el estado de ABP
  get currentUserId(): string {
    return this.configState.getOne('currentUser')?.id;
  }

  cargarOpiniones() {
    this.estaCargando.set(true);
    
    if (!this.currentUserId) {
      this.estaCargando.set(false);
      return;
    }

    // Usamos el método 'obtenerPorUsuario' definido en el Service
    this.opinionService.obtenerPorUsuario(this.currentUserId).subscribe({
      next: (lista) => {
        const listaDeOpinionesActivas = lista.filter(opinion => !opinion.isDeleted);
        this.misOpiniones.set(listaDeOpinionesActivas); 
        this.estaCargando.set(false);
      },
      error: (err) => {
        console.error('Error cargando opiniones:', err);
        this.toaster.error('Error al cargar las reseñas', 'Error');
        this.estaCargando.set(false);
      }
    });
  }

  // --- LÓGICA DE BORRADO ---
  eliminar(opinion: OpinionDto) {
    if (!confirm(`¿Estás seguro de querer borrar tu reseña sobre este destino?`)) return;

    // Marcamos el ID que se está eliminando para mostrar el spinner en el botón
    this.eliminandoId.set(opinion.id);
    
    // Usamos 'eliminarOpinion' definido en tu Service
    this.opinionService.eliminarOpinion(opinion.id).subscribe({
      next: () => {
        this.toaster.success('Reseña eliminada correctamente', 'Éxito');
        
        // Actualizamos la señal localmente filtrando el elemento eliminado
        this.misOpiniones.update(lista => lista.filter(o => o.id !== opinion.id));
        
        this.eliminandoId.set(null);
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('Ocurrió un error al intentar eliminar', 'Error');
        this.eliminandoId.set(null);
      }
    });
  }

  // --- LÓGICA DE EDICIÓN (MODAL) ---
  abrirModalEdicion(opinion: OpinionDto) {
    // Cargamos los datos actuales en las señales de edición
    this.ratingEditando.set(opinion.puntuacion);
    this.comentarioEditando.set(opinion.comentario || '');
    this.opinionEditando.set(opinion);
  }

  cerrarModalEdicion() {
    this.opinionEditando.set(null);
    this.guardandoEdicion.set(false);
    this.ratingEditando.set(0);
    this.comentarioEditando.set('');
  }

  guardarEdicion() {
    const opinionActual = this.opinionEditando();
    if (!opinionActual) return;

    this.guardandoEdicion.set(true);

    // Preparamos el DTO tal como lo espera el método 'actualizarOpinion'
    const input: createUpdateOpinionDto = {
      destinoTuristicoId: opinionActual.destinoTuristicoId,
      puntuacion: this.ratingEditando() as ValorPuntuacion,
      comentario: this.comentarioEditando()
    };

    // Llamamos a 'actualizarOpinion' (PUT)
    this.opinionService.actualizarOpinion(opinionActual.id, input).subscribe({
      next: (resultado) => {
        this.toaster.success('Reseña actualizada con éxito', 'Guardado');
        
        // Actualizamos la lista localmente para reflejar los cambios sin recargar
        this.misOpiniones.update(lista => {
            return lista.map(item => {
                if (item.id === opinionActual.id) {
                    // Retornamos el item actualizado fusionando los nuevos valores
                    // Mantenemos properties como 'nombreDestino' o 'creationTime' que no vienen en el input
                    return { 
                      ...item, 
                      puntuacion: input.puntuacion, 
                      comentario: input.comentario 
                    };
                }
                return item;
            });
        });

        this.cerrarModalEdicion();
      },
      error: (err) => {
        console.error(err);
        this.toaster.error('No se pudieron guardar los cambios', 'Error');
        this.guardandoEdicion.set(false);
      }
    });
  }
}