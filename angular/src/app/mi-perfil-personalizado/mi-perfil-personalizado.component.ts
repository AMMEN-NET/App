import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { RestService, ConfigStateService } from '@abp/ng.core';
import { ToasterService } from '@abp/ng.theme.shared';
import { FotoPerfilService } from '../services/foto-perfil.service';
import { CommonModule } from '@angular/common';
import { ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { AuthService } from '@abp/ng.core';
import { CuentaService } from '../proxy/cuentas/cuenta.service';
import { PreferenciasNotificacionService, FrecuenciaNotificacion } from '../proxy/notificaciones';

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
  private confirmation = inject(ConfirmationService); 
  private authService = inject(AuthService);          
  private cuentaService = inject(CuentaService);
  private preferenciasService = inject(PreferenciasNotificacionService);      

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

    // 4. Cargamos las preferencias de notificación
    this.cargarPreferencias();
  }

  cargarPreferencias() {
    this.preferenciasService.getMiPreferencia().subscribe({
      next: (prefs) => {
        this.form.patchValue({
          notifyScreen: prefs.enPantalla,
          notifyEmail: prefs.porEmail,
          notifyFrequency: prefs.frecuencia === FrecuenciaNotificacion.Inmediata ? 'immediate' : 'weekly'
        });
      },
      error: () => {
        // Si falla, dejamos los valores por defecto del form
      }
    });
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
    if (this.form.invalid) return;

    this.estaCargando.set(true);

    // Detectamos QUÉ cambió para ejecutar solo las llamadas necesarias
    const perfilCambio = ['email', 'name', 'surname', 'phoneNumber']
        .some(f => this.form.get(f)?.dirty);
    const prefsCambio = ['notifyScreen', 'notifyEmail', 'notifyFrequency']
        .some(f => this.form.get(f)?.dirty);
    const hayFoto = !!this.archivoSeleccionado;

    // Contamos operaciones pendientes para saber cuándo terminó todo
    let operacionesPendientes = 0;
    if (perfilCambio) operacionesPendientes++;
    if (prefsCambio) operacionesPendientes++;
    if (hayFoto) operacionesPendientes++;

    // Si nada cambió (no debería pasar, pero por seguridad)
    if (operacionesPendientes === 0) {
        this.estaCargando.set(false);
        return;
    }

    let exitoAlguno = false;

    const finalizarOperacion = () => {
        operacionesPendientes--;
        if (operacionesPendientes <= 0) {
            if (exitoAlguno) {
                this.toaster.success('Cambios guardados correctamente.', 'Éxito');
            }
            this.form.markAsPristine();
            this.estaCargando.set(false);
        }
    };

    // --- OPERACIÓN 1: Datos del perfil ---
    if (perfilCambio) {
        const formValues = this.form.getRawValue();
        const datosParaApi = {
            userName: formValues.userName,
            email: formValues.email,
            name: formValues.name,
            surname: formValues.surname,
            phoneNumber: formValues.phoneNumber
        };

        this.rest.request<any, any>({
            method: 'PUT',
            url: '/api/account/my-profile',
            body: datosParaApi
        }).subscribe({
            next: () => { exitoAlguno = true; finalizarOperacion(); },
            error: () => {
                this.toaster.error('Error al guardar los datos personales.', 'Error');
                finalizarOperacion();
            }
        });
    }

    // --- OPERACIÓN 2: Preferencias de notificación (INDEPENDIENTE del perfil) ---
    if (prefsCambio) {
        this.guardarPreferencias(finalizarOperacion, () => { exitoAlguno = true; });
    }

    // --- OPERACIÓN 3: Foto de perfil ---
    if (hayFoto) {
        this.fotoService.subirFoto(this.archivoSeleccionado!).subscribe({
            next: () => {
                exitoAlguno = true;
                this.archivoSeleccionado = null;
                this.actualizarUrlImagen();
                finalizarOperacion();
            },
            error: () => {
                this.toaster.warn('Hubo un error al subir la imagen.', 'Atención');
                finalizarOperacion();
            }
        });
    }
  }

  guardarPreferencias(onComplete?: () => void, onSuccess?: () => void) {
    const formValues = this.form.getRawValue();
    
    this.preferenciasService.updateMiPreferencia({
      enPantalla: formValues.notifyScreen,
      porEmail: formValues.notifyEmail,
      frecuencia: formValues.notifyFrequency === 'immediate' 
        ? FrecuenciaNotificacion.Inmediata 
        : FrecuenciaNotificacion.ResumenSemanal
    }).subscribe({
      next: () => {
        onSuccess?.();
        onComplete?.();
      },
      error: () => {
        this.toaster.warn('No se pudieron guardar las preferencias de notificación.', 'Atención');
        onComplete?.();
      }
    });
  }

  cargarImagenPorDefecto() {
    this.urlImagen = null;
  }

  eliminarCuenta() {
    this.confirmation
      .warn(
        'Se eliminarán tus datos y comentarios. Podrás recuperarla contactando a soporte.',
        '¿Estás seguro de eliminar tu cuenta?',
        {
          yesText: 'Sí, eliminar cuenta',
          cancelText: 'Cancelar',
          
        }
      )
      .subscribe((status) => {
        if (status === Confirmation.Status.confirm) {
          this.procesarBaja();
        }
      });
  }

  procesarBaja() {
    this.estaCargando.set(true);
    this.cuentaService.eliminarMiCuenta().subscribe({
      next: () => {
        this.estaCargando.set(false);
        // Deslogueamos forzosamente al usuario y lo mandamos al home
        this.authService.logout().subscribe();
      },
      error: (err) => {
        this.toaster.error('Ocurrió un error al intentar eliminar la cuenta.', 'Error');
        this.estaCargando.set(false);
      }
    });
  }
}
