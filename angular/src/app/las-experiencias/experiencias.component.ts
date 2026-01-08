import { Component, Input, OnInit, OnChanges, SimpleChanges, inject, signal } from '@angular/core'; // 1. Agregamos OnChanges y SimpleChanges
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { ExperienciaService } from '../proxy/experiencias/experiencia.service';
import { ExperienciaDto, CreateUpdateExperienciaDto } from '../proxy/experiencias/models'; // Asegurate de importar el DTO de creación
import { TipoExperiencia } from '../proxy/experiencias';
import { ConfigStateService } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { ToasterService } from '@abp/ng.theme.shared';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-experiencias',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './experiencias.component.html'
})
export class ExperienciasComponent implements OnInit, OnChanges {
  // Inyecciones
  private service = inject(ExperienciaService);
  private fb = inject(FormBuilder);
  private configState = inject(ConfigStateService);
  private confirmation = inject(ConfirmationService);
  private toaster = inject(ToasterService);

  // --- INPUTS (MODIFICADOS) ---
  // Ahora son opcionales para permitir los dos modos de uso
  @Input() destinoId?: string; 
  @Input() userId?: string;

  // Estado
  experiencias = signal<ExperienciaDto[]>([]);
  cargando = signal(false);
  usuarioActualId: string | undefined;
  
  // Filtros
  filtroValoracion = signal<TipoExperiencia | null>(null);
  buscador = new FormControl('');
  
  // Modal y Formulario
  mostrarModal = signal(false);
  modoEdicion = signal(false);
  idEnEdicion: string | null = null;
  // GUARDAMOS EL DESTINO DE LA EXPERIENCIA EN EDICIÓN
  // (Fundamental para cuando estamos en modo "Usuario" y no tenemos destinoId global)
  destinoIdEnEdicion: string | null = null; 
  
  form: FormGroup;
  
  // Enums para usar en HTML
  TipoExperiencia = TipoExperiencia;

  constructor() {
    this.form = this.fb.group({
      valoracion: [null, Validators.required],
      comentario: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]]
    });
  }

  // 2. Reactividad ante cambios de inputs
  ngOnChanges(changes: SimpleChanges) {
    // Si cambia el destino O el usuario, recargamos
    if (changes['destinoId'] || changes['userId']) {
      this.cargarExperiencias();
    }
  }

  ngOnInit() {
    this.usuarioActualId = this.configState.getOne('currentUser')?.id;

    // Si ya tenemos datos al inicio, cargamos
    this.cargarExperiencias();

    this.buscador.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(() => this.cargarExperiencias());
  }

  cargarExperiencias() {
    const texto = this.buscador.value || undefined;
    const valoracion = this.filtroValoracion() !== null ? this.filtroValoracion()! : undefined;

    // --- LÓGICA HÍBRIDA ---
    if (this.destinoId) {
      // MODO 1: Por Destino (Comportamiento original)
      this.cargando.set(true);
      this.service.getList(this.destinoId, valoracion, texto).subscribe({
        next: (data) => {
          this.experiencias.set(data);
          this.cargando.set(false);
        },
        error: () => this.cargando.set(false)
      });
    } 
    else if (this.userId || this.usuarioActualId) {
      // MODO 2: Por Usuario (Nuevo método)
      // Usamos el userId pasado por input, o fallback al usuario logueado
      const targetUserId = this.userId || this.usuarioActualId;
      
      if (!targetUserId) return; // Si no hay ID de usuario, no hacemos nada

      this.cargando.set(true);
      // Acá usamos el nuevo método que agregaste al servicio
      this.service.getListPorUsuario(targetUserId, valoracion, texto).subscribe({
        next: (data) => {
          this.experiencias.set(data);
          this.cargando.set(false);
        },
        error: () => this.cargando.set(false)
      });
    }
  }

  // --- FILTROS ---
  cambiarFiltro(val: TipoExperiencia | null) {
    this.filtroValoracion.set(val);
    this.cargarExperiencias();
  }

  // --- CRUD ---
  
  abrirModalEditar(exp: ExperienciaDto) {
    this.modoEdicion.set(true);
    this.idEnEdicion = exp.id;
    
    // IMPORTANTE: Capturamos el destinoId de la experiencia que se va a editar.
    // Si estamos viendo el perfil del usuario, 'this.destinoId' es undefined,
    // así que necesitamos sacar el ID de la experiencia misma.
    this.destinoIdEnEdicion = exp.destinoId; 

    this.form.patchValue({
      valoracion: exp.valoracion,
      comentario: exp.comentario
    });
    this.mostrarModal.set(true);
  }

  guardar() {
    if (this.form.invalid) return;
    
    // Determinamos qué ID de destino usar
    const idDestinoFinal = this.destinoId || this.destinoIdEnEdicion;

    if (!idDestinoFinal) {
      this.toaster.error("No se pudo identificar el destino para esta experiencia.");
      return;
    }

    const data: CreateUpdateExperienciaDto = {
      valoracion: Number(this.form.value.valoracion),
      comentario: this.form.value.comentario,
      destinoId: idDestinoFinal // Usamos el ID resuelto
    };

    if (this.modoEdicion() && this.idEnEdicion) {
      this.service.update(this.idEnEdicion, data).subscribe({
        next: () => {
          this.toaster.success('Experiencia actualizada');
          this.cerrarModal();
          this.cargarExperiencias();
        }
      });
    }
  }

  eliminar(id: string) {
    this.confirmation.warn('¿Borrar esta experiencia?', 'Confirmar', {
        yesText: 'Borrar', cancelText: 'Cancelar'
    }).subscribe(status => {
      if (status === Confirmation.Status.confirm) {
        this.service.delete(id).subscribe(() => {
          this.toaster.info('Experiencia eliminada');
          this.cargarExperiencias();
        });
      }
    });
  }

  cerrarModal() {
    this.mostrarModal.set(false);
    this.idEnEdicion = null;
    this.destinoIdEnEdicion = null; // Limpiamos referencia
  }
}