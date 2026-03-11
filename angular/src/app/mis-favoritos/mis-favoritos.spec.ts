import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError, Observable } from 'rxjs';
import { delay } from 'rxjs/operators';
import { RouterTestingModule } from '@angular/router/testing';
import { MisFavoritosComponent } from './mis-favoritos';
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos';
import { ToasterService } from '@abp/ng.theme.shared';
import { FavoritoDto } from '../proxy/favoritos/favoritos-dto';
import { EventosExternosService } from '../proxy/external-service';
import { EventoTicketmasterDto } from '../proxy/external-service/models';


describe('MisFavoritosComponent', () => {
  let component: MisFavoritosComponent;
  let fixture: ComponentFixture<MisFavoritosComponent>;
  let listadeFavoritosService: jasmine.SpyObj<ListaDeFavoritosService>;
  let toasterService: jasmine.SpyObj<ToasterService>;

  // Mock data
  const mockFavoritos: FavoritoDto[] = [
    {
      id: '1',
      nombre: 'París',
      pais: 'Francia',
      poblacion: 2161000,
      latitud: 48.8566,
      longitud: 2.3522,
      geoDBId: 'geo-1',
      promedioPuntuacion: 4.5,
      cantidadOpiniones: 120
    },
    {
      id: '2',
      nombre: 'Barcelona',
      pais: 'España',
      poblacion: 1620000,
      latitud: 41.3874,
      longitud: 2.1686,
      geoDBId: 'geo-2',
      promedioPuntuacion: 4.8,
      cantidadOpiniones: 95
    }
  ];

  beforeEach(async () => {
    // Crear mocks con jasmine.SpyObj para mejor type safety
    const listadeFavoritosSpy = jasmine.createSpyObj('ListaDeFavoritosService', [
      'obtenerFavoritos',
      'eliminarDeFavoritos',
      'agregarAFavoritos',
      'esFavorito',
      'contarFavoritos'
    ]);

    const toasterSpy = jasmine.createSpyObj('ToasterService', [
      'success',
      'error',
      'info',
      'warn'
    ]);

    const eventosExternosSpy = jasmine.createSpyObj('EventosExternosService', [
      'obtenerEventosPorUbicacion'
    ]);

    await TestBed.configureTestingModule({
      imports: [MisFavoritosComponent, RouterTestingModule],
      providers: [
        { provide: ListaDeFavoritosService, useValue: listadeFavoritosSpy },
        { provide: ToasterService, useValue: toasterSpy },
        { provide: EventosExternosService, useValue: eventosExternosSpy }
      ]
    }).compileComponents();

    listadeFavoritosService = TestBed.inject(ListaDeFavoritosService) as jasmine.SpyObj<ListaDeFavoritosService>;
    toasterService = TestBed.inject(ToasterService) as jasmine.SpyObj<ToasterService>;
    //eventosExternosService = TestBed.inject(EventosExternosService) as jasmine.SpyObj<EventosExternosService>;

    fixture = TestBed.createComponent(MisFavoritosComponent);
    component = fixture.componentInstance;
  });

  describe('Creación y Inicialización', () => {
    it('debería crear el componente', () => {
      expect(component).toBeTruthy();
    });

    it('debería inicializar las señales correctamente', () => {
      expect(component.favoritos()).toEqual([]);
      expect(component.estaCargando()).toBe(true);
      expect(component.eliminandoId()).toBe(null);
    });

    it('debería llamar a cargarFavoritos en ngOnInit', () => {
      spyOn(component, 'cargarFavoritos');
      component.ngOnInit();
      expect(component.cargarFavoritos).toHaveBeenCalled();
    });
  });

  describe('Carga de Favoritos', () => {
    it('debería cargar favoritos exitosamente', fakeAsync(() => {
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of(mockFavoritos));

      component.cargarFavoritos();
      tick(100);

      expect(component.favoritos()).toEqual(mockFavoritos);
      expect(component.estaCargando()).toBe(false);
    }));

    it('debería mostrar estado de carga mientras obtiene favoritos', fakeAsync(() => {
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of(mockFavoritos));

      expect(component.estaCargando()).toBe(true);

      component.cargarFavoritos();
      tick(100);

      expect(component.estaCargando()).toBe(false);
    }));

    it('debería manejar errores al cargar favoritos', fakeAsync(() => {
      const error = new Error('Error de conexión');
      listadeFavoritosService.obtenerFavoritos.and.returnValue(throwError(() => error));

      component.cargarFavoritos();
      tick(100); // Avanzar el tiempo simulado 100ms

      expect(component.estaCargando()).toBe(false);
      expect(toasterService.error).toHaveBeenCalledWith('Error al cargar la lista.', 'Error');
      expect(component.favoritos().length).toBe(0);
    }));

    it('debería mostrar lista vacía cuando no hay favoritos', fakeAsync(() => {
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of([]));

      component.cargarFavoritos();
      tick(100);

      expect(component.favoritos().length).toBe(0);
      expect(component.estaCargando()).toBe(false);
    }));
  });

  describe('Eliminación de Favoritos', () => {
    beforeEach(() => {
      component.favoritos.set([...mockFavoritos]);
    });

    it('debería eliminar un favorito exitosamente', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      listadeFavoritosService.eliminarDeFavoritos.and.returnValue(of(void 0).pipe(delay(100)));

      component.eliminar('1', 'París');

      // El valor puede establecerse en la cola de microtasks dependiendo del entorno;
      // avanzar microtasks para asegurar el valor antes de las comprobaciones.
      tick(0);
      expect(component.eliminandoId()).toBe('1');

      tick(100);

      expect(listadeFavoritosService.eliminarDeFavoritos).toHaveBeenCalledWith('1');
      expect(toasterService.info).toHaveBeenCalledWith('Destino eliminado.', 'Adiós');
      expect(component.favoritos().length).toBe(1);
      expect(component.favoritos()[0].id).toBe('2');
      expect(component.eliminandoId()).toBe(null);
    }));

    it('debería mostrar estado de eliminación mientras se procesa', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      // Usar una Observable que no se complete inmediatamente
      listadeFavoritosService.eliminarDeFavoritos.and.returnValue(
        new Observable(() => {}) // Observable infinita que nunca completa
      );

      component.eliminar('1', 'París');

      expect(component.eliminandoId()).toBe('1');
    });

    it('no debería eliminar si el usuario cancela la confirmación', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      component.eliminar('1', 'París');

      expect(listadeFavoritosService.eliminarDeFavoritos).not.toHaveBeenCalled();
    });

    it('debería mostrar confirmar con el nombre de la ciudad', () => {
      const confirmSpy = spyOn(window, 'confirm').and.returnValue(false);
      const nombreCiudad = 'Barcelona';

      component.eliminar('2', nombreCiudad);

      expect(confirmSpy).toHaveBeenCalledWith(`¿Deseas quitar a ${nombreCiudad} de tus favoritos?`);
    });

    it('debería manejar errores al eliminar un favorito', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      const error = new Error('Error del servidor');
      listadeFavoritosService.eliminarDeFavoritos.and.returnValue(throwError(() => error));

      const favoritoAntes = component.favoritos().length;
      component.eliminar('1', 'París');
      tick(100);

      expect(toasterService.error).toHaveBeenCalledWith('No se pudo eliminar.', 'Error');
      expect(component.favoritos().length).toBe(favoritoAntes); // No debe eliminar del estado
      expect(component.eliminandoId()).toBe(null);
    }));

    it('debería limpiar el estado de eliminandoId después de eliminar', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      listadeFavoritosService.eliminarDeFavoritos.and.returnValue(of(void 0).pipe(delay(100)));

      component.eliminar('1', 'París');
      tick(100);

      expect(component.eliminandoId()).toBe(null);
    }));

    it('debería limpiar el estado de eliminandoId en caso de error', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      listadeFavoritosService.eliminarDeFavoritos.and.returnValue(throwError(() => new Error('Error')));

      component.eliminar('1', 'París');
      tick(100);

      expect(component.eliminandoId()).toBe(null);
    }));
  });

  describe('Manejo de Señales', () => {
    it('debería actualizar la señal de favoritos correctamente', () => {
      component.favoritos.set(mockFavoritos);
      expect(component.favoritos().length).toBe(2);

      component.favoritos.update(lista => lista.filter(f => f.id !== '1'));
      expect(component.favoritos().length).toBe(1);
    });

    it('debería cambiar el estado de carga', () => {
      component.estaCargando.set(true);
      expect(component.estaCargando()).toBe(true);

      component.estaCargando.set(false);
      expect(component.estaCargando()).toBe(false);
    });

    it('debería actualizar eliminandoId al eliminar', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      listadeFavoritosService.eliminarDeFavoritos.and.returnValue(of(void 0).pipe(delay(100)));

      component.favoritos.set(mockFavoritos);
      component.eliminar('1', 'París');

      // Avanzar microtasks para asegurar que el valor fue seteado
      tick(0);
      expect(component.eliminandoId()).toBe('1');
    }));
  });

  describe('Integración HTML y lógica', () => {
    beforeEach(() => {
      // Configurar mock para que no lance errores en ngOnInit
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of([]));
    });

    it('debería renderizar el contador de favoritos correcto', () => {
      // Asegurar que ngOnInit cargue los mismos favoritos que esperamos mostrar
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of(mockFavoritos));
      fixture.detectChanges();

      const contador = fixture.nativeElement.textContent;
      expect(contador).toContain('2');
      expect(contador).toContain('destinos guardados');
    });

    it('debería mostrar loading spinner cuando estaCargando es true', () => {
      // Evitar que la llamada a obtenerFavoritos complete para mantener el estado de carga
      listadeFavoritosService.obtenerFavoritos.and.returnValue(new Observable(() => {}));
      fixture.detectChanges();

      const spinner = fixture.nativeElement.querySelector('.animate-spin');
      expect(spinner).toBeTruthy();
    });

    it('debería mostrar empty state cuando no hay favoritos', () => {
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of([]));
      fixture.detectChanges();

      const emptyState = fixture.nativeElement.textContent;
      expect(emptyState).toContain('Sin destinos guardados');
    });

    it('debería renderizar la lista de favoritos cuando hay datos (estado)', () => {
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of(mockFavoritos));
      fixture.detectChanges();

      // La plantilla usa `@for` en tiempo de build; en tests comprobamos el estado
      expect(component.favoritos().length).toBeGreaterThan(0);
    });

    it('debería mostrar la cantidad correcta de elementos en la lista (estado)', () => {
      listadeFavoritosService.obtenerFavoritos.and.returnValue(of(mockFavoritos));
      fixture.detectChanges();

      // La plantilla usa `@for`; comprobamos el estado de la señal en su lugar
      expect(component.favoritos().length).toBe(mockFavoritos.length);
    });
  });

   describe('Eventos Ticketmaster', () => {
    let eventosExternosService: jasmine.SpyObj<EventosExternosService>;
    const mockEventos: EventoTicketmasterDto[] = [
      { id: 'ev1', nombre: 'Concierto París', urlTicket: 'https://ticket.com/ev1' },
      { id: 'ev2', nombre: 'Festival Barcelona', urlTicket: 'https://ticket.com/ev2' }
    ];

    beforeEach(() => {
      eventosExternosService = TestBed.inject(EventosExternosService) as jasmine.SpyObj<EventosExternosService>;
    });

    it('debería abrir el modal y cargar eventos exitosamente', fakeAsync(() => {
      eventosExternosService.obtenerEventosPorUbicacion.and.returnValue(of(mockEventos));
      
      component.abrirModalEventos('48.8566', '2.3522');
      tick();

      expect(component.mostrarModalEventos).toBeTrue();
      expect(component.cargandoEventos).toBeFalse();
      expect(component.eventos.length).toBe(2);
      expect(component.eventos[0].nombre).toBe('Concierto París');
    }));

    it('debería mostrar estado de carga al buscar eventos', () => {
      eventosExternosService.obtenerEventosPorUbicacion.and.returnValue(new Observable(() => {}));
      
      component.abrirModalEventos('48.8566', '2.3522');
      
      expect(component.cargandoEventos).toBeTrue();
      expect(component.eventos.length).toBe(0);
    });

    it('debería manejar error al buscar eventos', fakeAsync(() => {
      eventosExternosService.obtenerEventosPorUbicacion.and.returnValue(throwError(() => new Error('Error Ticketmaster')));
      
      component.abrirModalEventos('48.8566', '2.3522');
      tick();
      
      expect(component.cargandoEventos).toBeFalse();
      expect(component.eventos.length).toBe(0);
    }));

    it('debería cerrar el modal de eventos', () => {
      component.mostrarModalEventos = true;
      
      component.cerrarModalEventos();
      
      expect(component.mostrarModalEventos).toBeFalse();
    });

    it('debería limpiar eventos al abrir un nuevo modal', fakeAsync(() => {
      component.eventos = mockEventos;
      eventosExternosService.obtenerEventosPorUbicacion.and.returnValue(of([]));
      
      component.abrirModalEventos('41.3874', '2.1686');
      tick();

      expect(component.eventos.length).toBe(0);
    }));

    it('debería manejar error al buscar eventos', fakeAsync(() => {
      spyOn(console, 'error');
      eventosExternosService.obtenerEventosPorUbicacion.and.returnValue(
        throwError(() => new Error('Error Ticketmaster'))
      );

      component.abrirModalEventos('48.8566', '2.3522');
      tick();

      expect(component.cargandoEventos).toBeFalse();
      expect(component.eventos.length).toBe(0);
    }));
   });
});