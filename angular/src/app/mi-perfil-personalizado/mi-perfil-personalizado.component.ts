import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RestService, ConfigStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { FotoPerfilService } from '../services/foto-perfil.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-mi-perfil-personalizado',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './mi-perfil-personalizado.component.html'
})
export class MiPerfilPersonalizadoComponent implements OnInit {
  private fb = inject(FormBuilder);
  private rest = inject(RestService);
  private fotoService = inject(FotoPerfilService);
  private toaster = inject(ToasterService);
  private configState = inject(ConfigStateService);

  form: FormGroup;
  estaCargando = signal(false);
  urlImagen: string | null = null;
  usuarioId: string | null = null;
  archivoSeleccionado: File | null = null;

  constructor() {
    this.form = this.fb.group({
      userName: [{ value: '', disabled: true }, Validators.required],
      email: ['', [Validators.required, Validators.email]],
      name: ['', Validators.required],
      surname: ['', Validators.required],
      phoneNumber: [''],
      
      // --- NUEVOS CAMPOS DE PREFERENCIAS (Simulados por ahora) ---
      notifyScreen: [true],        // Por defecto activado
      notifyEmail: [false],        // Por defecto desactivado
      notifyFrequency: ['immediate'] // Valores: 'immediate' o 'weekly'
    });
  }

  ngOnInit(): void {
    // 1. Obtenemos ID de la configuración global
    const currentUser = this.configState.getOne('currentUser');
    this.usuarioId = currentUser?.id;

    // 2. Cargamos la imagen si hay ID
    if (this.usuarioId) {
      this.actualizarUrlImagen();
    }

    // 3. Cargamos los datos del formulario
    this.cargarDatos();
  }

  cargarDatos() {
    this.estaCargando.set(true);
    
    this.rest.request<any, any>({
      method: 'GET',
      url: '/api/account/my-profile', 
    }).subscribe({
      next: (perfil) => {
        this.form.patchValue(perfil);
        this.estaCargando.set(false);
      },
      error: () => this.estaCargando.set(false)
    });
  }

  actualizarUrlImagen() {
    if (this.usuarioId) {
        // Timestamp para evitar caché del navegador
        this.urlImagen = `${this.fotoService.obtenerUrlFoto(this.usuarioId)}?t=${new Date().getTime()}`;
    }
  }

  alSeleccionarArchivo(event: any) {
    const file = event.target.files[0];
    if (file) {
        this.archivoSeleccionado = file;
        // Previsualización inmediata
        const reader = new FileReader();
        reader.onload = (e: any) => this.urlImagen = e.target.result;
        reader.readAsDataURL(file);
    }
  }

  guardar() {
    // Validamos: Si el formulario es inválido y se intentó cambiar texto, paramos.
    if (this.form.invalid && !this.form.pristine) return;

    this.estaCargando.set(true);

    // ESCENARIO 1: Solo se seleccionó foto nueva (Texto sin cambios)
    if (this.form.pristine && this.archivoSeleccionado) {
        this.subirSoloFoto();
        return;
    }

    // ESCENARIO 2: Se cambió texto (y quizás también foto)
    
    // --- CAMBIO IMPORTANTE AQUÍ ---
    // Obtenemos todos los valores del formulario
    const formValues = this.form.getRawValue();

    // Filtramos SOLO los datos que el Backend actual entiende (ProfileUpdateDto)
    // Dejamos fuera 'notifyScreen', 'notifyEmail', etc. para que no rompa la API.
    const datosParaApi = {
        userName: formValues.userName,
        email: formValues.email,
        name: formValues.name,
        surname: formValues.surname,
        phoneNumber: formValues.phoneNumber
    };
    
    // Aquí podrías guardar las preferencias en LocalStorage temporalmente si quisieras
    // console.log('Preferencias seleccionadas:', { 
    //    screen: formValues.notifyScreen, 
    //    email: formValues.notifyEmail,
    //    freq: formValues.notifyFrequency 
    // });

    this.rest.request<any, any>({
      method: 'PUT',
      url: '/api/account/my-profile',
      body: datosParaApi // <--- Enviamos el objeto filtrado, no el form completo
    }).subscribe({
      next: () => {
        // Si además había foto seleccionada, la subimos ahora
        if (this.archivoSeleccionado) {
            this.subirSoloFoto();
        } else {
            // Solo texto cambiado
            this.toaster.success('Perfil actualizado correctamente.', 'Éxito');
            this.form.markAsPristine(); // Reseteamos estado del form
            this.estaCargando.set(false);
        }
      },
      error: (err) => {
        this.toaster.error('Error al guardar los datos personales.', 'Error');
        this.estaCargando.set(false);
      }
    });
  }

  // Método auxiliar para no repetir la lógica de subida
  subirSoloFoto() {
    if (!this.archivoSeleccionado) return;

    this.fotoService.subirFoto(this.archivoSeleccionado).subscribe({
        next: () => {
            this.toaster.success('Foto de perfil actualizada.', 'Éxito');
            this.estaCargando.set(false);
            
            this.archivoSeleccionado = null; 
            this.form.markAsPristine(); 
            this.actualizarUrlImagen(); 
        },
        error: () => {
            this.toaster.warn('Datos guardados pero hubo error con la imagen.', 'Atención');
            this.estaCargando.set(false);
        }
    });
  }
  
  cargarImagenPorDefecto() {
    this.urlImagen = null;
  }
}