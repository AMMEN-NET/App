//
import { Component, Input, OnInit, OnChanges, SimpleChanges, inject, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators, FormControl } from '@angular/forms';
import { ExperienciaService } from '../proxy/experiencias/experiencia.service';
import { ExperienciaDto, CreateUpdateExperienciaDto } from '../proxy/experiencias/models';
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

  // --- INPUTS ---
  @Input() destinoId?: string; // Si viene del Home (filtro fijo)
  
  // Estado
  experiencias = signal<ExperienciaDto[]>([]);
  cargando = signal(false);
  usuarioActualId: string | undefined;

  // Filtros y Modos
  modoVisualizacion = signal<'mias' | 'todas'>('todas'); // Por defecto 'todas' o 'mias' según prefieras
  filtroValoracion = signal<TipoExperiencia | null>(null);
  buscador = new FormControl('');

  // Modal Crear/Editar
  mostrarModal = signal(false);
  modoEdicion = signal(false);
  idEnEdicion: string | null = null;
  destinoIdEnEdicion: string | null = null;
  
  form: FormGroup;
  TipoExperiencia = TipoExperiencia;

  constructor() {
    this.form = this.fb.group({
      valoracion: [null, Validators.required],
      comentario: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]]
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['destinoId']) {
      // Si nos pasan un destino fijo (desde Home), forzamos modo "todas" para ver las de ese lugar
      this.modoVisualizacion.set('todas');
      this.cargarExperiencias();
    }
  }

  ngOnInit() {
    this.usuarioActualId = this.configState.getOne('currentUser')?.id;
    
    // Si no hay destino fijo, iniciamos en 'mias' (opcional)
    if (!this.destinoId && this.usuarioActualId) {
        this.modoVisualizacion.set('mias');
    }

    this.cargarExperiencias();

    this.buscador.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(() => this.cargarExperiencias());
  }

  // --- CAMBIO DE MODO (MÍAS vs TODAS) ---
  cambiarModo(modo: 'mias' | 'todas') {
    this.modoVisualizacion.set(modo);
    this.cargarExperiencias();
  }

  cargarExperiencias() {
    this.cargando.set(true);
    const texto = this.buscador.value || undefined;
    const valoracion = this.filtroValoracion() !== null ? this.filtroValoracion()! : undefined;

    // LÓGICA PRINCIPAL DE CARGA
    if (this.modoVisualizacion() === 'mias' && this.usuarioActualId) {
        // Opción A: Ver solo MIS experiencias
        this.service.getListPorUsuario(this.usuarioActualId, valoracion, texto).subscribe({
            next: (data) => {
                this.experiencias.set(data);
                this.cargando.set(false);
            },
            error: () => this.cargando.set(false)
        });
    } else {
        // Opción B: Ver experiencias PÚBLICAS (Globales o de un Destino específico)
        // Si this.destinoId tiene valor, el backend filtrará por ciudad. Si es null, busca globalmente.
        const idDestinoParaEnviar = this.destinoId || null; 
        
        this.service.getList(idDestinoParaEnviar, valoracion, texto).subscribe({
            next: (data) => {
                this.experiencias.set(data);
                this.cargando.set(false);
            },
            error: () => this.cargando.set(false)
        });
    }
  }

  // --- MÉTODOS AUXILIARES (Filtros) ---
  cambiarFiltro(val: TipoExperiencia | null) {
    this.filtroValoracion.set(val);
    this.cargarExperiencias();
  }

  // --- CRUD (Abrir modal para CREAR nueva experiencia desde aquí si se desea) ---
  abrirModalCrear() {
      // Solo permitimos crear si estamos en el contexto de un destino (desde Home)
      // O si implementas un selector de destinos en el modal.
      if (!this.destinoId) {
          this.toaster.warn('Debes seleccionar un destino desde el inicio para agregar una experiencia.');
          return;
      }
      this.modoEdicion.set(false);
      this.form.reset({ valoracion: TipoExperiencia.MuyBueno });
      this.mostrarModal.set(true);
  }

  abrirModalEditar(exp: ExperienciaDto) {
    this.modoEdicion.set(true);
    this.idEnEdicion = exp.id;
    this.destinoIdEnEdicion = exp.destinoId; 
    this.form.patchValue({
      valoracion: exp.valoracion,
      comentario: exp.comentario
    });
    this.mostrarModal.set(true);
  }

  guardar() {
    if (this.form.invalid) return;
    
    // Si estamos editando, usamos el ID guardado. Si creamos, usamos el Input destinoId.
    const idDestinoFinal = this.modoEdicion() ? this.destinoIdEnEdicion : this.destinoId;

    if (!idDestinoFinal) {
      this.toaster.error("Error: No hay destino seleccionado.");
      return;
    }

    const data: CreateUpdateExperienciaDto = {
      valoracion: Number(this.form.value.valoracion),
      comentario: this.form.value.comentario,
      destinoId: idDestinoFinal
    };

    if (this.modoEdicion() && this.idEnEdicion) {
      this.service.update(this.idEnEdicion, data).subscribe({
        next: () => this.finalizarGuardado('Experiencia actualizada')
      });
    } else {
       this.service.create(data).subscribe({
        next: () => this.finalizarGuardado('Experiencia creada')
       });
    }
  }

  finalizarGuardado(mensaje: string) {
      this.toaster.success(mensaje);
      this.cerrarModal();
      this.cargarExperiencias();
  }

  eliminar(id: string) {
    this.confirmation.warn('¿Borrar esta experiencia?', 'Confirmar').subscribe(status => {
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
    this.destinoIdEnEdicion = null;
  }
}