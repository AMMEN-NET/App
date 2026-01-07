import { Component, Input, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ExperienciaService } from '../proxy/experiencias/experiencia.service';
import { ExperienciaDto } from '../proxy/experiencias/models';
import { TipoExperiencia } from '../proxy/experiencias';
import { ConfigStateService } from '@abp/ng.core';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { ToasterService } from '@abp/ng.theme.shared';
import { FormControl } from '@angular/forms';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-experiencias',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './experiencias.component.html'
})
export class ExperienciasComponent implements OnInit {
  // Inyecciones
  private service = inject(ExperienciaService);
  private fb = inject(FormBuilder);
  private configState = inject(ConfigStateService);
  private confirmation = inject(ConfirmationService);
  private toaster = inject(ToasterService);

  // Inputs
  @Input({ required: true }) destinoId!: string;

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
  form: FormGroup;
  
  // Enums para usar en HTML
  TipoExperiencia = TipoExperiencia;

  constructor() {
    this.form = this.fb.group({
      valoracion: [null, Validators.required],
      comentario: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(1000)]]
    });
  }

  ngOnInit() {
    // Obtener ID del usuario actual para saber qué botones mostrar
    this.usuarioActualId = this.configState.getOne('currentUser')?.id;

    // Cargar datos iniciales
    this.cargarExperiencias();

    // Escuchar buscador
    this.buscador.valueChanges.pipe(
      debounceTime(500),
      distinctUntilChanged()
    ).subscribe(() => this.cargarExperiencias());
  }

  cargarExperiencias() {
    this.cargando.set(true);
    const texto = this.buscador.value || undefined;
    const valoracion = this.filtroValoracion() !== null ? this.filtroValoracion()! : undefined;

    this.service.getList(this.destinoId, valoracion, texto).subscribe({
      next: (data) => {
        this.experiencias.set(data);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  // --- FILTROS ---
  cambiarFiltro(val: TipoExperiencia | null) {
    this.filtroValoracion.set(val);
    this.cargarExperiencias();
  }

  // --- CRUD ---
  abrirModalCrear() {
    this.modoEdicion.set(false);
    this.idEnEdicion = null;
    this.form.reset({ valoracion: TipoExperiencia.MuyBueno }); // Valor por defecto
    this.mostrarModal.set(true);
  }

  abrirModalEditar(exp: ExperienciaDto) {
    this.modoEdicion.set(true);
    this.idEnEdicion = exp.id;
    this.form.patchValue({
      valoracion: exp.valoracion,
      comentario: exp.comentario
    });
    this.mostrarModal.set(true);
  }

  guardar() {
    if (this.form.invalid) return;
    
    const data = {
      ...this.form.value,
      destinoId: this.destinoId
    };

    if (this.modoEdicion()) {
      this.service.update(this.idEnEdicion!, data).subscribe({
        next: () => {
          this.toaster.success('Experiencia actualizada');
          this.cerrarModal();
          this.cargarExperiencias();
        }
      });
    } else {
      this.service.create(data).subscribe({
        next: () => {
          this.toaster.success('Experiencia publicada');
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
  }
}