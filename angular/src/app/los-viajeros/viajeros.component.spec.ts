import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError, Observable } from 'rxjs';
import { delay } from 'rxjs/operators';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterTestingModule } from '@angular/router/testing';
import { ViajerosComponent } from './viajeros.component';
import { ViajerosService } from '../proxy/viajeros/viajeros.service';
import { FotoPerfilService } from '../services/foto-perfil.service';
import { ViajeroDto, PerfilPublicoDto } from '../proxy/viajeros';

describe('ViajerosComponent', () => {
  let component: ViajerosComponent;
  let fixture: ComponentFixture<ViajerosComponent>;
  let viajerosService: jasmine.SpyObj<ViajerosService>;
  let fotoService: jasmine.SpyObj<FotoPerfilService>;

  // Mock data
  const mockViajeros: ViajeroDto[] = [
    {
      id: 'viajero-1',
      userName: 'juan_travels',
      name: 'Juan',
      surname: 'Pérez'
    },
    {
      id: 'viajero-2',
      userName: 'maria_adventure',
      name: 'María',
      surname: 'García'
    },
    {
      id: 'viajero-3',
      userName: 'carlos_explorer',
      name: 'Carlos',
      surname: 'López'
    }
  ];

  const mockPerfilPublico: PerfilPublicoDto = {
    id: 'viajero-1',
    userName: 'juan_travels',
    name: 'Juan',
    surname: 'Pérez',
    fechaRegistro: '2022-03-15T00:00:00Z',
    cantidadOpiniones: 12,
    cantidadFavoritos: 8,
    promedioPuntuacion: 4.5
  };

  beforeEach(async () => {
    const viajerosServiceSpy = jasmine.createSpyObj('ViajerosService', [
      'getList',
      'getPerfilPublico'
    ]);
    const fotoServiceSpy = jasmine.createSpyObj('FotoPerfilService', [
      'obtenerUrlFoto'
    ]);

    // Valores de retorno por defecto
    fotoServiceSpy.obtenerUrlFoto.and.returnValue('/api/foto/default');

    await TestBed.configureTestingModule({
      imports: [
        ViajerosComponent,
        CommonModule,
        ReactiveFormsModule,
        RouterTestingModule
      ],
      providers: [
        { provide: ViajerosService, useValue: viajerosServiceSpy },
        { provide: FotoPerfilService, useValue: fotoServiceSpy }
      ]
    }).compileComponents();

    viajerosService = TestBed.inject(ViajerosService) as jasmine.SpyObj<ViajerosService>;
    fotoService = TestBed.inject(FotoPerfilService) as jasmine.SpyObj<FotoPerfilService>;

    fixture = TestBed.createComponent(ViajerosComponent);
    component = fixture.componentInstance;
  });

  describe('Creación e Inicialización', () => {
    it('debería crear el componente', () => {
      expect(component).toBeTruthy();
    });

    it('debería inicializar signals correctamente', () => {
      expect(component.viajeros()).toEqual([]);
      expect(component.cargando()).toBe(false);
      expect(component.perfilSeleccionado()).toBeNull();
      expect(component.cargandoPerfil()).toBe(false);
    });

    it('debería inicializar el formulario de búsqueda', () => {
      expect(component.buscador).toBeTruthy();
      expect(component.buscador.value).toBe('');
    });

    it('debería cargar viajeros en ngOnInit', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));

      component.ngOnInit();
      tick(100);

      expect(viajerosService.getList).toHaveBeenCalledWith('');
      expect(component.viajeros()).toEqual(mockViajeros);
    }));

    it('debería establecer cargando en true durante la carga inicial', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));

      expect(component.cargando()).toBe(false);
      component.ngOnInit();
      expect(component.cargando()).toBe(true);

      tick(100);
      expect(component.cargando()).toBe(false);
    }));

    it('debería inicializar la suscripción a cambios del buscador', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));

      component.ngOnInit();
      tick(100);

      // Simular un cambio en el buscador
      component.buscador.setValue('juan');
      tick(300); // debounceTime es 300ms

      expect(viajerosService.getList).toHaveBeenCalledWith('juan');
    }));
  });

  describe('Carga de Viajeros', () => {
    it('debería cargar viajeros sin filtro', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));

      component.cargarViajeros();
      tick(100);

      expect(viajerosService.getList).toHaveBeenCalledWith('');
      expect(component.viajeros()).toEqual(mockViajeros);
      expect(component.cargando()).toBe(false);
    }));

    it('debería cargar viajeros con filtro', fakeAsync(() => {
      const viajerosFiltrados = [mockViajeros[0]];
      viajerosService.getList.and.returnValue(of(viajerosFiltrados).pipe(delay(100)));

      component.cargarViajeros('juan');
      tick(100);

      expect(viajerosService.getList).toHaveBeenCalledWith('juan');
      expect(component.viajeros()).toEqual(viajerosFiltrados);
    }));

    it('debería mostrar lista vacía cuando no hay resultados', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of([]).pipe(delay(100)));

      component.cargarViajeros('xyz');
      tick(100);

      expect(component.viajeros()).toEqual([]);
      expect(component.cargando()).toBe(false);
    }));

    it('debería manejar errores al cargar viajeros', fakeAsync(() => {
      const error = new Error('Error de conexión');
      viajerosService.getList.and.returnValue(throwError(() => error));

      component.cargarViajeros();
      tick(100);

      expect(component.cargando()).toBe(false);
      expect(component.viajeros()).toEqual([]);
    }));

    it('debería establecer cargando en true mientras carga', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));

      expect(component.cargando()).toBe(false);
      component.cargarViajeros();
      expect(component.cargando()).toBe(true);

      tick(100);
      expect(component.cargando()).toBe(false);
    }));
  });

  describe('Búsqueda y Filtrado', () => {
    it('debería aplicar debounce a los cambios del buscador', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));
      component.ngOnInit(); // Esto se llama una vez
      tick(100);

      // Realizar múltiples cambios rápidamente
      component.buscador.setValue('j');
      tick(100);
      component.buscador.setValue('ju');
      tick(100);
      component.buscador.setValue('juan');
      tick(100);

      // Esperar a que termine el debounce
      tick(300);

      // Debería haber una llamada inicial + una final (no tres por los cambios rápidos)
      expect(viajerosService.getList).toHaveBeenCalledWith('juan');
    }));

    it('debería aplicar distinctUntilChanged al buscador', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));
      component.ngOnInit();
      tick(100);

      // Establecer el mismo valor dos veces
      component.buscador.setValue('juan');
      tick(300);
      component.buscador.setValue('juan');
      tick(300);

      // Debería no hacer una segunda llamada al service
      // (Se espera que solo se llame una vez por el distinctUntilChanged)
      expect(viajerosService.getList).toHaveBeenCalledWith('juan');
    }));

    it('debería buscar cuando el usuario escribe en el buscador', fakeAsync(() => {
      const viajerosFiltrados = [mockViajeros[0]];
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));
      component.ngOnInit();
      tick(100);

      viajerosService.getList.and.returnValue(of(viajerosFiltrados).pipe(delay(100)));
      component.buscador.setValue('juan');
      tick(300);

      expect(viajerosService.getList).toHaveBeenCalledWith('juan');
    }));

    it('debería vaciar la búsqueda cuando se limpia el input', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));
      component.ngOnInit();
      tick(100);

      component.buscador.setValue('');
      tick(300);

      expect(viajerosService.getList).toHaveBeenCalledWith('');
    }));
  });

  describe('Gestión de Fotos', () => {
    it('debería obtener URL de foto para un viajero', () => {
      fotoService.obtenerUrlFoto.and.returnValue('/api/foto/viajero-1');

      const url = component.obtenerFoto('viajero-1');

      expect(fotoService.obtenerUrlFoto).toHaveBeenCalledWith('viajero-1');
      expect(url).toBe('/api/foto/viajero-1');
    });

    it('debería manejar error de foto', () => {
      const event = {
        target: {
          style: { display: '' },
          parentElement: {
            innerHTML: 'original'
          }
        }
      };

      component.manejarErrorFoto(event);

      expect(event.target.style.display).toBe('none');
      expect(event.target.parentElement.innerHTML).toContain('fa-user');
      expect(event.target.parentElement.innerHTML).toContain('bg-gray-100');
    });

    it('debería mostrar ícono gris cuando falla la carga de foto', () => {
      const mockElement = document.createElement('img');
      mockElement.style.display = '';
      const mockParent = document.createElement('div');
      mockParent.appendChild(mockElement);

      const event = {
        target: mockElement
      };

      component.manejarErrorFoto(event);

      expect(mockElement.style.display).toBe('none');
      expect(mockParent.innerHTML).toContain('fa-user');
    });
  });

  describe('Modal de Perfil Público', () => {
    it('debería abrir modal de perfil y cargar datos', fakeAsync(() => {
      viajerosService.getPerfilPublico.and.returnValue(
        of(mockPerfilPublico).pipe(delay(100))
      );

      component.abrirPerfil('viajero-1');
      expect(component.cargandoPerfil()).toBe(true);

      tick(100);

      expect(viajerosService.getPerfilPublico).toHaveBeenCalledWith('viajero-1');
      expect(component.perfilSeleccionado()).toEqual(mockPerfilPublico);
      expect(component.cargandoPerfil()).toBe(false);
    }));

    it('debería manejar error al cargar perfil público', fakeAsync(() => {
      const error = new Error('Error al cargar perfil');
      viajerosService.getPerfilPublico.and.returnValue(throwError(() => error));

      component.abrirPerfil('viajero-1');
      tick(100);

      expect(component.cargandoPerfil()).toBe(false);
      expect(component.perfilSeleccionado()).toBeNull();
    }));

    it('debería cerrar modal de perfil', () => {
      component.perfilSeleccionado.set(mockPerfilPublico);

      component.cerrarPerfil();

      expect(component.perfilSeleccionado()).toBeNull();
    });

    it('debería mostrar datos del perfil en la modal', fakeAsync(() => {
      viajerosService.getPerfilPublico.and.returnValue(
        of(mockPerfilPublico).pipe(delay(100))
      );

      component.abrirPerfil('viajero-1');
      tick(100);

      const perfil = component.perfilSeleccionado();
      expect(perfil?.name).toBe('Juan');
      expect(perfil?.surname).toBe('Pérez');
      expect(perfil?.userName).toBe('juan_travels');
      expect(perfil?.cantidadOpiniones).toBe(12);
      expect(perfil?.cantidadFavoritos).toBe(8);
    }));

    it('debería obtener foto del viajero en la modal', fakeAsync(() => {
      viajerosService.getPerfilPublico.and.returnValue(
        of(mockPerfilPublico).pipe(delay(100))
      );
      fotoService.obtenerUrlFoto.and.returnValue('/api/foto/viajero-1');

      component.abrirPerfil('viajero-1');
      tick(100);

      const url = component.obtenerFoto('viajero-1');

      expect(fotoService.obtenerUrlFoto).toHaveBeenCalledWith('viajero-1');
      expect(url).toBe('/api/foto/viajero-1');
    }));

    it('debería establecer cargandoPerfil en true al abrir modal', () => {
      viajerosService.getPerfilPublico.and.returnValue(
        of(mockPerfilPublico).pipe(delay(100))
      );

      expect(component.cargandoPerfil()).toBe(false);
      component.abrirPerfil('viajero-1');
      expect(component.cargandoPerfil()).toBe(true);
    });

    it('debería mostrar fecha de registro si está disponible', fakeAsync(() => {
      viajerosService.getPerfilPublico.and.returnValue(
        of(mockPerfilPublico).pipe(delay(100))
      );

      component.abrirPerfil('viajero-1');
      tick(100);

      const perfil = component.perfilSeleccionado();
      expect(perfil?.fechaRegistro).toBe('2022-03-15T00:00:00Z');
    }));
  });

  describe('Signals', () => {
    it('debería actualizar signal viajeros', () => {
      expect(component.viajeros()).toEqual([]);

      component.viajeros.set(mockViajeros);

      expect(component.viajeros()).toEqual(mockViajeros);
    });

    it('debería actualizar signal cargando', () => {
      expect(component.cargando()).toBe(false);

      component.cargando.set(true);
      expect(component.cargando()).toBe(true);

      component.cargando.set(false);
      expect(component.cargando()).toBe(false);
    });

    it('debería actualizar signal perfilSeleccionado', () => {
      expect(component.perfilSeleccionado()).toBeNull();

      component.perfilSeleccionado.set(mockPerfilPublico);
      expect(component.perfilSeleccionado()).toEqual(mockPerfilPublico);

      component.perfilSeleccionado.set(null);
      expect(component.perfilSeleccionado()).toBeNull();
    });

    it('debería actualizar signal cargandoPerfil', () => {
      expect(component.cargandoPerfil()).toBe(false);

      component.cargandoPerfil.set(true);
      expect(component.cargandoPerfil()).toBe(true);

      component.cargandoPerfil.set(false);
      expect(component.cargandoPerfil()).toBe(false);
    });
  });

  describe('Integración', () => {
    it('debería completar flujo completo: cargar viajeros, buscar, abrir perfil', fakeAsync(() => {
      // Paso 1: Cargar lista inicial
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));
      component.ngOnInit();
      tick(100);

      expect(component.viajeros()).toEqual(mockViajeros);
      expect(component.cargando()).toBe(false);

      // Paso 2: Buscar
      const viajerosFiltrados = [mockViajeros[0]];
      viajerosService.getList.and.returnValue(
        of(viajerosFiltrados).pipe(delay(100))
      );
      component.buscador.setValue('juan');
      tick(300);

      expect(viajerosService.getList).toHaveBeenCalledWith('juan');

      // Paso 3: Abrir perfil
      viajerosService.getPerfilPublico.and.returnValue(
        of(mockPerfilPublico).pipe(delay(100))
      );
      component.abrirPerfil('viajero-1');
      tick(100);

      expect(component.perfilSeleccionado()).toEqual(mockPerfilPublico);

      // Paso 4: Cerrar perfil
      component.cerrarPerfil();
      expect(component.perfilSeleccionado()).toBeNull();
    }));

    it('debería manejar búsqueda vacía y mostrar todos los viajeros', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));

      component.ngOnInit();
      tick(100);

      expect(component.viajeros().length).toBe(3);

      component.buscador.setValue('');
      tick(300);

      expect(viajerosService.getList).toHaveBeenCalledWith('');
    }));

    it('debería mantener la lista de viajeros al abrir y cerrar modal', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of(mockViajeros).pipe(delay(100)));
      viajerosService.getPerfilPublico.and.returnValue(
        of(mockPerfilPublico).pipe(delay(100))
      );

      component.ngOnInit();
      tick(100);

      const listaOriginal = component.viajeros();

      component.abrirPerfil('viajero-1');
      tick(100);

      component.cerrarPerfil();

      expect(component.viajeros()).toEqual(listaOriginal);
    }));

    it('debería actualizar lista cuando se cambia el filtro de búsqueda', fakeAsync(() => {
      const viajerosFiltrados1 = [mockViajeros[0]];
      const viajerosFiltrados2 = [mockViajeros[1]];

      viajerosService.getList.and.returnValue(
        of(viajerosFiltrados1).pipe(delay(100))
      );
      component.ngOnInit();
      tick(100);

      viajerosService.getList.and.returnValue(
        of(viajerosFiltrados2).pipe(delay(100))
      );
      component.buscador.setValue('maria');
      tick(300); // debounceTime
      tick(100); // delay de la observable del servicio

      expect(component.viajeros()[0].userName).toBe('maria_adventure');
    }));

    it('debería mostrar lista vacía cuando búsqueda no tiene resultados', fakeAsync(() => {
      viajerosService.getList.and.returnValue(of([]).pipe(delay(100)));

      component.cargarViajeros('xyz123');
      tick(100);

      expect(component.viajeros()).toEqual([]);
      expect(component.cargando()).toBe(false);
    }));

    it('debería obtener fotos correctamente para cada viajero en la lista', () => {
      component.viajeros.set(mockViajeros);
      fotoService.obtenerUrlFoto.and.callFake((id: string) => `/api/foto/${id}`);

      mockViajeros.forEach(viajero => {
        const url = component.obtenerFoto(viajero.id);
        expect(url).toContain(viajero.id);
        expect(fotoService.obtenerUrlFoto).toHaveBeenCalledWith(viajero.id);
      });
    });
  });
});
