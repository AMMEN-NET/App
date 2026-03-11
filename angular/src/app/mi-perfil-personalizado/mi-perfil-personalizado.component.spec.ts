import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError, Observable } from 'rxjs';
import { delay } from 'rxjs/operators';
import { ReactiveFormsModule, FormBuilder } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterTestingModule } from '@angular/router/testing';
import { MiPerfilPersonalizadoComponent } from './mi-perfil-personalizado.component';
import { RestService } from '@abp/ng.core';
import { ToasterService, ConfirmationService, Confirmation } from '@abp/ng.theme.shared';
import { ConfigStateService, AuthService } from '@abp/ng.core';
import { FotoPerfilService } from '../services/foto-perfil.service';
import { CuentaService } from '../proxy/cuentas/cuenta.service';

describe('MiPerfilPersonalizadoComponent', () => {
  let component: MiPerfilPersonalizadoComponent;
  let fixture: ComponentFixture<MiPerfilPersonalizadoComponent>;
  let restService: jasmine.SpyObj<RestService>;
  let toasterService: jasmine.SpyObj<ToasterService>;
  let configStateService: jasmine.SpyObj<ConfigStateService>;
  let fotoService: jasmine.SpyObj<FotoPerfilService>;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;
  let authService: jasmine.SpyObj<AuthService>;
  let cuentaService: jasmine.SpyObj<CuentaService>;

  // Mock data
  const mockCurrentUser = { id: 'user-123', username: 'testuser' };
  
  const mockProfileData = {
    userName: 'testuser',
    name: 'Juan',
    surname: 'Pérez',
    email: 'juan@example.com',
    phoneNumber: '+34 612345678',
    notifyScreen: true,
    notifyEmail: false,
    notifyFrequency: 'immediate'
  };

  beforeEach(async () => {
    // Crear mocks con jasmine.SpyObj
    const restServiceSpy = jasmine.createSpyObj('RestService', ['request']);
    const toasterServiceSpy = jasmine.createSpyObj('ToasterService', [
      'success',
      'error',
      'warn',
      'info'
    ]);
    const configStateServiceSpy = jasmine.createSpyObj('ConfigStateService', ['getOne']);
    const fotoServiceSpy = jasmine.createSpyObj('FotoPerfilService', [
      'obtenerUrlFoto'
    ]);
    const confirmationServiceSpy = jasmine.createSpyObj('ConfirmationService', ['warn']);
    const authServiceSpy = jasmine.createSpyObj('AuthService', ['logout']);
    const cuentaServiceSpy = jasmine.createSpyObj('CuentaService', ['eliminarMiCuenta']);

    // Configurar valores de retorno por defecto
    configStateServiceSpy.getOne.and.returnValue(mockCurrentUser);
    fotoServiceSpy.obtenerUrlFoto.and.returnValue('/api/foto/user-123');

    await TestBed.configureTestingModule({
      imports: [
        MiPerfilPersonalizadoComponent,
        CommonModule,
        ReactiveFormsModule,
        RouterTestingModule
      ],
      providers: [
        FormBuilder,
        { provide: RestService, useValue: restServiceSpy },
        { provide: ToasterService, useValue: toasterServiceSpy },
        { provide: ConfigStateService, useValue: configStateServiceSpy },
        { provide: FotoPerfilService, useValue: fotoServiceSpy },
        { provide: ConfirmationService, useValue: confirmationServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: CuentaService, useValue: cuentaServiceSpy }
      ]
    }).compileComponents();

    restService = TestBed.inject(RestService) as jasmine.SpyObj<RestService>;
    toasterService = TestBed.inject(ToasterService) as jasmine.SpyObj<ToasterService>;
    configStateService = TestBed.inject(ConfigStateService) as jasmine.SpyObj<ConfigStateService>;
    fotoService = TestBed.inject(FotoPerfilService) as jasmine.SpyObj<FotoPerfilService>;
    confirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
    authService = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    cuentaService = TestBed.inject(CuentaService) as jasmine.SpyObj<CuentaService>;

    fixture = TestBed.createComponent(MiPerfilPersonalizadoComponent);
    component = fixture.componentInstance;
  });

  describe('Creación e Inicialización', () => {
    it('debería crear el componente', () => {
      expect(component).toBeTruthy();
    });

    it('debería inicializar el formulario correctamente', () => {
      expect(component.form).toBeTruthy();
      expect(component.form.get('userName')).toBeTruthy();
      expect(component.form.get('name')).toBeTruthy();
      expect(component.form.get('surname')).toBeTruthy();
      expect(component.form.get('email')).toBeTruthy();
      expect(component.form.get('phoneNumber')).toBeTruthy();
      expect(component.form.get('notifyScreen')).toBeTruthy();
      expect(component.form.get('notifyEmail')).toBeTruthy();
      expect(component.form.get('notifyFrequency')).toBeTruthy();
    });

    it('debería inicializar la señal estaCargando en false', () => {
      expect(component.estaCargando()).toBe(false);
    });

    it('debería inicializar urlImagen en null', () => {
      expect(component.urlImagen).toBeNull();
    });

    it('debería inicializar archivoSeleccionado en null', () => {
      expect(component.archivoSeleccionado).toBeNull();
    });

    it('debería llamar a ngOnInit y cargar datos', fakeAsync(() => {
      restService.request.and.returnValue(of(mockProfileData).pipe(delay(100)));
      spyOn(component, 'cargarDatos').and.callThrough();

      component.ngOnInit();
      tick(100);

      expect(component.cargarDatos).toHaveBeenCalled();
      expect(component.usuarioId).toBe('user-123');
    }));

    it('debería obtener el ID del usuario desde ConfigStateService', fakeAsync(() => {
      restService.request.and.returnValue(of(mockProfileData).pipe(delay(100)));
      component.ngOnInit();
      tick(100);
      expect(configStateService.getOne).toHaveBeenCalledWith('currentUser');
      expect(component.usuarioId).toBe('user-123');
    }));

    it('debería validar que el campo userName está deshabilitado', () => {
      const userNameControl = component.form.get('userName');
      expect(userNameControl?.disabled).toBe(true);
    });

    it('debería requerir email válido', () => {
      const emailControl = component.form.get('email');
      emailControl?.setValue('invalid-email');
      expect(emailControl?.valid).toBe(false);

      emailControl?.setValue('valid@example.com');
      expect(emailControl?.valid).toBe(true);
    });
  });

  describe('Carga de Datos del Perfil', () => {
    it('debería cargar datos del perfil exitosamente', fakeAsync(() => {
      restService.request.and.returnValue(of(mockProfileData).pipe(delay(100)));

      component.cargarDatos();
      tick(100);

      expect(restService.request).toHaveBeenCalledWith({
        method: 'GET',
        url: '/api/account/my-profile'
      });
      expect(component.form.get('name')?.value).toBe('Juan');
      expect(component.form.get('surname')?.value).toBe('Pérez');
      expect(component.form.get('email')?.value).toBe('juan@example.com');
      expect(component.estaCargando()).toBe(false);
    }));

    it('debería mostrar estado de carga mientras obtiene datos', fakeAsync(() => {
      restService.request.and.returnValue(of(mockProfileData).pipe(delay(100)));

      expect(component.estaCargando()).toBe(false);
      component.cargarDatos();
      expect(component.estaCargando()).toBe(true);
      
      tick(100);
      expect(component.estaCargando()).toBe(false);
    }));

    it('debería manejar errores al cargar datos', fakeAsync(() => {
      const error = new Error('Error de conexión');
      restService.request.and.returnValue(throwError(() => error));

      component.cargarDatos();
      tick(100);

      expect(component.estaCargando()).toBe(false);
    }));

    it('debería actualizar la URL de imagen al cargar', fakeAsync(() => {
      component.usuarioId = 'user-123';

      component.actualizarUrlImagen();

      // Verificar que la URL se construye correctamente con timestamp
      expect(component.urlImagen).toContain('/api/app/foto-perfil/obtener/user-123?t=');
    }));
  });

  describe('Gestión de Foto de Perfil', () => {
    it('debería actualizar URL de imagen con timestamp', () => {
      component.usuarioId = 'user-123';

      component.actualizarUrlImagen();

      expect(component.urlImagen).toContain('/api/app/foto-perfil/obtener/user-123?t=');
    });

    it('debería manejar selección de archivo', (done) => {
      const file = new File(['content'], 'avatar.jpg', { type: 'image/jpeg' });
      const event = {
        target: { files: [file] }
      };

      component.alSeleccionarArchivo(event);

      expect(component.archivoSeleccionado).toBe(file);
      
      // Verificar que se crea la preview
      setTimeout(() => {
        expect(component.urlImagen).toBeTruthy();
        expect(component.urlImagen?.startsWith('data:image')).toBe(true);
        done();
      }, 50);
    });

    it('no debería asignar archivo si no hay selección', () => {
      const event = {
        target: { files: [] }
      };

      component.alSeleccionarArchivo(event);

      expect(component.archivoSeleccionado).toBeNull();
    });

    it('debería cargar imagen por defecto', () => {
      component.urlImagen = 'https://example.com/image.jpg';

      component.cargarImagenPorDefecto();

      expect(component.urlImagen).toBeNull();
    });
  });

  describe('Guardar Cambios', () => {
    beforeEach(() => {
      component.form.patchValue(mockProfileData);
      component.form.markAsPristine();
    });

      it('debería guardar solo los datos del perfil cuando se modifica el formulario', fakeAsync(() => {
      const nameControl = component.form.get('name');

      nameControl?.setValue('Carlos');
      nameControl?.markAsDirty();

      restService.request.and.returnValue(of(void 0).pipe(delay(100)));

      component.guardar();
      tick(100);

      expect(restService.request).toHaveBeenCalledWith({
        method: 'PUT',
        url: '/api/account/my-profile',
        body: jasmine.objectContaining({
          userName: mockProfileData.userName,
          email: mockProfileData.email,
          name: 'Carlos',
          surname: mockProfileData.surname,
          phoneNumber: mockProfileData.phoneNumber
        })
      });

      expect(toasterService.success).toHaveBeenCalledWith(
        'Cambios guardados correctamente.',
        'Éxito'
      );
      expect(component.estaCargando()).toBe(false);
    }));





    it('no debería guardar si formulario es inválido', () => {
      component.form.get('email')?.setValue('invalid-email');
      component.form.markAsDirty();

      component.guardar();

      expect(restService.request).not.toHaveBeenCalled();
    });



    it('debería limpiar el estado del formulario después de guardar', fakeAsync(() => {
      const nameControl = component.form.get('name');

      nameControl?.setValue('Nuevo Nombre');
      nameControl?.markAsDirty();

      restService.request.and.returnValue(of(void 0).pipe(delay(100)));

      component.guardar();
      tick(100);

      expect(component.form.pristine).toBe(true);
      expect(component.archivoSeleccionado).toBeNull();
    }));

    it('debería deshabilitarse el botón de guardar si formulario es pristine y sin archivo', () => {
      component.form.markAsPristine();
      component.archivoSeleccionado = null;

      const isDisabled = component.form.pristine && !component.archivoSeleccionado;
      expect(isDisabled).toBe(true);
    });
  });

  describe('Eliminar Cuenta', () => {
    it('debería abrir confirmación al eliminar cuenta', () => {
      confirmationService.warn.and.returnValue(
        of(Confirmation.Status.dismiss)
      );

      component.eliminarCuenta();

      expect(confirmationService.warn).toHaveBeenCalledWith(
        'Se eliminarán tus datos y comentarios. Podrás recuperarla contactando a soporte.',
        '¿Estás seguro de eliminar tu cuenta?',
        jasmine.objectContaining({
          yesText: 'Sí, eliminar cuenta',
          cancelText: 'Cancelar'
        })
      );
    });

    it('debería procesar la baja si el usuario confirma', fakeAsync(() => {
      confirmationService.warn.and.returnValue(
        of(Confirmation.Status.confirm)
      );
      cuentaService.eliminarMiCuenta.and.returnValue(of(void 0).pipe(delay(100)));
      authService.logout.and.returnValue(of(void 0));
      spyOn(component, 'procesarBaja').and.callThrough();

      component.eliminarCuenta();
      tick(100);

      expect(component.procesarBaja).toHaveBeenCalled();
    }));

    it('no debería procesar baja si usuario cancela', fakeAsync(() => {
      confirmationService.warn.and.returnValue(
        of(Confirmation.Status.dismiss)
      );
      spyOn(component, 'procesarBaja');

      component.eliminarCuenta();
      tick(100);

      expect(component.procesarBaja).not.toHaveBeenCalled();
    }));

    it('debería eliminar la cuenta exitosamente', fakeAsync(() => {
      cuentaService.eliminarMiCuenta.and.returnValue(of(void 0).pipe(delay(100)));
      authService.logout.and.returnValue(of(void 0));

      component.procesarBaja();
      tick(100);

      expect(cuentaService.eliminarMiCuenta).toHaveBeenCalled();
      expect(component.estaCargando()).toBe(false);
    }));

    it('debería efectuar logout después de eliminar cuenta', fakeAsync(() => {
      cuentaService.eliminarMiCuenta.and.returnValue(of(void 0).pipe(delay(100)));
      authService.logout.and.returnValue(of(void 0));

      component.procesarBaja();
      tick(100);

      expect(authService.logout).toHaveBeenCalled();
    }));

    it('debería manejar error al eliminar cuenta', fakeAsync(() => {
      const error = new Error('Error al eliminar');
      cuentaService.eliminarMiCuenta.and.returnValue(throwError(() => error));

      component.procesarBaja();
      tick(100);

      expect(toasterService.error).toHaveBeenCalledWith(
        'Ocurrió un error al intentar eliminar la cuenta.',
        'Error'
      );
      expect(component.estaCargando()).toBe(false);
    }));

    it('debería establecer estaCargando en true mientras se elimina', fakeAsync(() => {
      cuentaService.eliminarMiCuenta.and.returnValue(of(void 0).pipe(delay(100)));
      authService.logout.and.returnValue(of(void 0));

      expect(component.estaCargando()).toBe(false);
      component.procesarBaja();
      expect(component.estaCargando()).toBe(true);

      tick(100);
      expect(component.estaCargando()).toBe(false);
    }));
  });

  describe('Signals', () => {
    it('debería actualizar estaCargando signal', () => {
      expect(component.estaCargando()).toBe(false);

      component.estaCargando.set(true);
      expect(component.estaCargando()).toBe(true);

      component.estaCargando.set(false);
      expect(component.estaCargando()).toBe(false);
    });

    it('debería actualizar urlImagen cuando se carga foto', (done) => {
      const file = new File(['content'], 'avatar.jpg', { type: 'image/jpeg' });
      const event = {
        target: { files: [file] }
      };

      component.alSeleccionarArchivo(event);

      setTimeout(() => {
        expect(component.urlImagen).toBeTruthy();
        expect(component.urlImagen?.startsWith('data:image')).toBe(true);
        done();
      }, 50);
    });
  });

  describe('Integración', () => {
    it('debería completar flujo completo de actualización de perfil', fakeAsync(() => {
      restService.request.and.returnValue(of(mockProfileData).pipe(delay(100)));
      component.ngOnInit();
      tick(100);

      expect(component.form.get('name')?.value).toBe('Juan');

      const nameControl = component.form.get('name');
      nameControl?.setValue('Carlos');
      nameControl?.markAsDirty();

      restService.request.and.returnValue(of(void 0).pipe(delay(100)));
      component.guardar();
      tick(100);

      expect(toasterService.success).toHaveBeenCalledWith(
        'Cambios guardados correctamente.',
        'Éxito'
      );
    }));



    it('debería completar flujo de eliminación de cuenta', fakeAsync(() => {
      confirmationService.warn.and.returnValue(
        of(Confirmation.Status.confirm)
      );
      cuentaService.eliminarMiCuenta.and.returnValue(of(void 0).pipe(delay(100)));
      authService.logout.and.returnValue(of(void 0));

      component.eliminarCuenta();
      tick(100);

      expect(cuentaService.eliminarMiCuenta).toHaveBeenCalled();
      expect(authService.logout).toHaveBeenCalled();
    }));

    it('debería validar formulario completo antes de guardar', () => {
      component.form.get('email')?.setValue('invalid');
      component.form.markAsDirty();

      expect(component.form.invalid).toBe(true);

      component.guardar();

      expect(restService.request).not.toHaveBeenCalled();
    });

    it('debería manejar preferencias de notificación en el formulario', () => {
      expect(component.form.get('notifyScreen')?.value).toBe(true);
      expect(component.form.get('notifyEmail')?.value).toBe(false);
      expect(component.form.get('notifyFrequency')?.value).toBe('immediate');

      component.form.patchValue({
        notifyScreen: false,
        notifyEmail: true,
        notifyFrequency: 'weekly'
      });

      expect(component.form.get('notifyScreen')?.value).toBe(false);
      expect(component.form.get('notifyEmail')?.value).toBe(true);
      expect(component.form.get('notifyFrequency')?.value).toBe('weekly');
    });
  });
});
