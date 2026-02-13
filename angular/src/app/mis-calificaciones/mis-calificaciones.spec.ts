import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { of, throwError, Observable } from 'rxjs';
import { delay } from 'rxjs/operators';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterTestingModule } from '@angular/router/testing';
import { MisCalificacionesComponent } from './mis-calificaciones';
import { OpinionService } from '../proxy/opiniones/opinion.service';
import { ToasterService } from '@abp/ng.theme.shared';
import { ConfigStateService } from '@abp/ng.core';
import { OpinionDto, createUpdateOpinionDto } from '../proxy/opiniones/opiniones-dto/models';
import { ValorPuntuacion } from '../proxy/opiniones/valor-puntuacion.enum';

describe('MisCalificacionesComponent', () => {
  let component: MisCalificacionesComponent;
  let fixture: ComponentFixture<MisCalificacionesComponent>;
  let opinionService: jasmine.SpyObj<OpinionService>;
  let toasterService: jasmine.SpyObj<ToasterService>;
  let configStateService: jasmine.SpyObj<ConfigStateService>;

  // Mock data
  const mockOpiniones: OpinionDto[] = [
    {
      id: '1',
      nombreDestino: 'París',
      destinoTuristicoId: 'dest-1',
      puntuacion: 5 as ValorPuntuacion,
      comentario: 'Hermosa ciudad, recomiendomucho visitarla.',
      esFavorito: true,
      creationTime: '2024-01-15T00:00:00Z',
      isDeleted: false,
      userId: 'user-123'
    },
    {
      id: '2',
      nombreDestino: 'Barcelona',
      destinoTuristicoId: 'dest-2',
      puntuacion: 4 as ValorPuntuacion,
      comentario: 'Playas lindas y arquitectura única.',
      esFavorito: false,
      creationTime: '2024-02-10T00:00:00Z',
      isDeleted: false,
      userId: 'user-123'
    }
  ];

  const mockCurrentUser = { id: 'user-123', username: 'testuser' };

  beforeEach(async () => {
    // Crear mocks con jasmine.SpyObj para mejor type safety
    const opinionServiceSpy = jasmine.createSpyObj('OpinionService', [
      'obtenerPorUsuario',
      'eliminarOpinion',
      'actualizarOpinion'
    ]);

    const toasterSpy = jasmine.createSpyObj('ToasterService', [
      'success',
      'error',
      'info',
      'warn'
    ]);

    const configStateServiceSpy = jasmine.createSpyObj('ConfigStateService', ['getOne']);
    configStateServiceSpy.getOne.and.returnValue(mockCurrentUser);

    await TestBed.configureTestingModule({
      imports: [MisCalificacionesComponent, CommonModule, FormsModule, RouterTestingModule],
      providers: [
        { provide: OpinionService, useValue: opinionServiceSpy },
        { provide: ToasterService, useValue: toasterSpy },
        { provide: ConfigStateService, useValue: configStateServiceSpy }
      ]
    }).compileComponents();

    opinionService = TestBed.inject(OpinionService) as jasmine.SpyObj<OpinionService>;
    toasterService = TestBed.inject(ToasterService) as jasmine.SpyObj<ToasterService>;
    configStateService = TestBed.inject(ConfigStateService) as jasmine.SpyObj<ConfigStateService>;

    fixture = TestBed.createComponent(MisCalificacionesComponent);
    component = fixture.componentInstance;
  });

  describe('Creación e Inicialización', () => {
    it('debería crear el componente', () => {
      expect(component).toBeTruthy();
    });

    it('debería inicializar las señales correctamente', () => {
      expect(component.misOpiniones()).toEqual([]);
      expect(component.estaCargando()).toBe(true);
      expect(component.opinionEditando()).toBe(null);
      expect(component.eliminandoId()).toBe(null);
      expect(component.ratingEditando()).toBe(0);
      expect(component.comentarioEditando()).toBe('');
      expect(component.guardandoEdicion()).toBe(false);
    });

    it('debería llamar a cargarOpiniones en ngOnInit', () => {
      spyOn(component, 'cargarOpiniones');
      component.ngOnInit();
      expect(component.cargarOpiniones).toHaveBeenCalled();
    });

    it('debería obtener currentUserId desde ConfigStateService', () => {
      expect(component.currentUserId).toBe('user-123');
      expect(configStateService.getOne).toHaveBeenCalledWith('currentUser');
    });
  });

  describe('Carga de Opiniones', () => {
    it('debería cargar opiniones exitosamente', fakeAsync(() => {
      opinionService.obtenerPorUsuario.and.returnValue(of(mockOpiniones));

      component.cargarOpiniones();
      tick(100);

      expect(component.misOpiniones()).toEqual(mockOpiniones);
      expect(component.estaCargando()).toBe(false);
    }));

    it('debería filtrar opiniones eliminadas al cargar', fakeAsync(() => {
      const opinionesConEliminada = [
        ...mockOpiniones,
        {
          id: '3',
          nombreDestino: 'Madrid',
          destinoTuristicoId: 'dest-3',
          puntuacion: 3 as ValorPuntuacion,
          comentario: 'Ciudad interesante.',
          esFavorito: false,
          creationTime: '2024-03-01T00:00:00Z',
          isDeleted: true,
          userId: 'user-123'
        }
      ];

      opinionService.obtenerPorUsuario.and.returnValue(of(opinionesConEliminada));

      component.cargarOpiniones();
      tick(100);

      // Debe tener solo 2 opiniones (sin la eliminada)
      expect(component.misOpiniones().length).toBe(2);
      expect(component.misOpiniones().every(o => !o.isDeleted)).toBe(true);
    }));

    it('debería mostrar estado de carga mientras obtiene opiniones', fakeAsync(() => {
      opinionService.obtenerPorUsuario.and.returnValue(of(mockOpiniones));

      expect(component.estaCargando()).toBe(true);

      component.cargarOpiniones();
      tick(100);

      expect(component.estaCargando()).toBe(false);
    }));

    it('debería manejar errores al cargar opiniones', fakeAsync(() => {
      const error = new Error('Error de conexión');
      opinionService.obtenerPorUsuario.and.returnValue(throwError(() => error));

      component.cargarOpiniones();
      tick(100);

      expect(component.estaCargando()).toBe(false);
      expect(toasterService.error).toHaveBeenCalledWith(
        'Error al cargar las reseñas',
        'Error'
      );
      expect(component.misOpiniones().length).toBe(0);
    }));

    it('debería mostrar lista vacía cuando no hay opiniones', fakeAsync(() => {
      opinionService.obtenerPorUsuario.and.returnValue(of([]));

      component.cargarOpiniones();
      tick(100);

      expect(component.misOpiniones().length).toBe(0);
      expect(component.estaCargando()).toBe(false);
    }));

    it('no debería cargar opiniones si no hay currentUserId', fakeAsync(() => {
      configStateService.getOne.and.returnValue(null);

      component.cargarOpiniones();
      tick(100);

      expect(opinionService.obtenerPorUsuario).not.toHaveBeenCalled();
      expect(component.estaCargando()).toBe(false);
    }));
  });

  describe('Eliminación de Opiniones', () => {
    beforeEach(() => {
      component.misOpiniones.set([...mockOpiniones]);
    });

    it('debería eliminar una opinión exitosamente', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      opinionService.eliminarOpinion.and.returnValue(of(void 0).pipe(delay(100)));

      const opinionAEliminar = mockOpiniones[0];
      component.eliminar(opinionAEliminar);

      tick(0);
      expect(component.eliminandoId()).toBe('1');

      tick(100);

      expect(opinionService.eliminarOpinion).toHaveBeenCalledWith('1');
      expect(toasterService.success).toHaveBeenCalledWith(
        'Reseña eliminada correctamente',
        'Éxito'
      );
      expect(component.misOpiniones().length).toBe(1);
      expect(component.misOpiniones()[0].id).toBe('2');
      expect(component.eliminandoId()).toBe(null);
    }));

    it('debería mostrar estado de eliminación mientras se procesa', () => {
      spyOn(window, 'confirm').and.returnValue(true);
      opinionService.eliminarOpinion.and.returnValue(new Observable(() => {}));

      const opinionAEliminar = mockOpiniones[0];
      component.eliminar(opinionAEliminar);

      expect(component.eliminandoId()).toBe('1');
    });

    it('no debería eliminar si el usuario cancela la confirmación', () => {
      spyOn(window, 'confirm').and.returnValue(false);

      const opinionAEliminar = mockOpiniones[0];
      component.eliminar(opinionAEliminar);

      expect(opinionService.eliminarOpinion).not.toHaveBeenCalled();
    });

    it('debería manejar errores al eliminar una opinión', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      const error = new Error('Error del servidor');
      opinionService.eliminarOpinion.and.returnValue(throwError(() => error));

      const opinionesAntes = component.misOpiniones().length;
      const opinionAEliminar = mockOpiniones[0];
      component.eliminar(opinionAEliminar);
      tick(100);

      expect(toasterService.error).toHaveBeenCalledWith(
        'Ocurrió un error al intentar eliminar',
        'Error'
      );
      expect(component.misOpiniones().length).toBe(opinionesAntes);
      expect(component.eliminandoId()).toBe(null);
    }));

    it('debería limpiar el estado de eliminandoId después de eliminar exitosamente', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      opinionService.eliminarOpinion.and.returnValue(of(void 0).pipe(delay(100)));

      const opinionAEliminar = mockOpiniones[0];
      component.eliminar(opinionAEliminar);
      tick(100);

      expect(component.eliminandoId()).toBe(null);
    }));

    it('debería limpiar el estado de eliminandoId en caso de error', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      opinionService.eliminarOpinion.and.returnValue(
        throwError(() => new Error('Error'))
      );

      const opinionAEliminar = mockOpiniones[0];
      component.eliminar(opinionAEliminar);
      tick(100);

      expect(component.eliminandoId()).toBe(null);
    }));
  });

  describe('Edición de Opiniones', () => {
    beforeEach(() => {
      component.misOpiniones.set([...mockOpiniones]);
    });

    it('debería abrir modal de edición con datos correctos', () => {
      const opinionAEditar = mockOpiniones[0];
      component.abrirModalEdicion(opinionAEditar);

      expect(component.opinionEditando()).toEqual(opinionAEditar);
      expect(component.ratingEditando()).toBe(5);
      expect(component.comentarioEditando()).toBe(
        'Hermosa ciudad, recomiendomucho visitarla.'
      );
    });

    it('debería cerrar modal de edición y limpiar señales', () => {
      component.opinionEditando.set(mockOpiniones[0]);
      component.ratingEditando.set(5);
      component.comentarioEditando.set('Comentario de prueba');
      component.guardandoEdicion.set(true);

      component.cerrarModalEdicion();

      expect(component.opinionEditando()).toBe(null);
      expect(component.ratingEditando()).toBe(0);
      expect(component.comentarioEditando()).toBe('');
      expect(component.guardandoEdicion()).toBe(false);
    });

    it('debería guardar cambios en opinión exitosamente', fakeAsync(() => {
      const opinionAEditar = mockOpiniones[0];
      component.opinionEditando.set(opinionAEditar);
      component.ratingEditando.set(4);
      component.comentarioEditando.set('Comentario actualizado');

      const inputEsperado: createUpdateOpinionDto = {
        destinoTuristicoId: opinionAEditar.destinoTuristicoId,
        puntuacion: 4 as ValorPuntuacion,
        comentario: 'Comentario actualizado'
      };

      opinionService.actualizarOpinion.and.returnValue(
        of({} as OpinionDto).pipe(delay(100))
      );

      component.guardarEdicion();

      tick(0);
      expect(component.guardandoEdicion()).toBe(true);

      tick(100);

      expect(opinionService.actualizarOpinion).toHaveBeenCalledWith(
        '1',
        inputEsperado
      );
      expect(toasterService.success).toHaveBeenCalledWith(
        'Reseña actualizada con éxito',
        'Guardado'
      );

      // Verificar que la opinión fue actualizada en la lista
      const opinionActualizada = component.misOpiniones().find(o => o.id === '1');
      expect(opinionActualizada?.puntuacion).toBe(4);
      expect(opinionActualizada?.comentario).toBe('Comentario actualizado');

      // Verificar que el modal se cerró
      expect(component.opinionEditando()).toBe(null);
    }));

    it('debería manejar errores al guardar edición', fakeAsync(() => {
      const opinionAEditar = mockOpiniones[0];
      component.opinionEditando.set(opinionAEditar);
      component.ratingEditando.set(3);
      component.comentarioEditando.set('Nuevo comentario');

      const error = new Error('Error al actualizar');
      opinionService.actualizarOpinion.and.returnValue(throwError(() => error));

      component.guardarEdicion();
      tick(100);

      expect(toasterService.error).toHaveBeenCalledWith(
        'No se pudieron guardar los cambios',
        'Error'
      );
      expect(component.guardandoEdicion()).toBe(false);

      // Modal debe seguir abierto en caso de error
      expect(component.opinionEditando()).not.toBe(null);
    }));

    it('no debería guardar si rating es 0', fakeAsync(() => {
      const opinionAEditar = mockOpiniones[0];
      component.opinionEditando.set(opinionAEditar);
      component.ratingEditando.set(0);
      component.comentarioEditando.set('Test comment');

      // Aún así, configurar el mock para evitar errores si el componente llama al servicio
      opinionService.actualizarOpinion.and.returnValue(of({} as OpinionDto).pipe(delay(100)));

      component.guardarEdicion();
      tick(100);

      expect(opinionService.actualizarOpinion).not.toHaveBeenCalled();
    }));

    it('no debería procesar guardarEdicion si no hay opinionEditando', () => {
      component.opinionEditando.set(null);
      component.ratingEditando.set(5);
      component.comentarioEditando.set('Test comment');

      component.guardarEdicion();

      expect(opinionService.actualizarOpinion).not.toHaveBeenCalled();
    });
  });

  describe('Manejo de Señales', () => {
    it('debería actualizar la señal de misOpiniones correctamente', () => {
      component.misOpiniones.set(mockOpiniones);
      expect(component.misOpiniones().length).toBe(2);

      component.misOpiniones.update(lista => lista.filter(o => o.id !== '1'));
      expect(component.misOpiniones().length).toBe(1);
    });

    it('debería cambiar el estado de carga', () => {
      component.estaCargando.set(true);
      expect(component.estaCargando()).toBe(true);

      component.estaCargando.set(false);
      expect(component.estaCargando()).toBe(false);
    });

    it('debería actualizar ratingEditando cuando se selecciona una estrella', () => {
      component.ratingEditando.set(0);
      expect(component.ratingEditando()).toBe(0);

      component.ratingEditando.set(3);
      expect(component.ratingEditando()).toBe(3);

      component.ratingEditando.set(5);
      expect(component.ratingEditando()).toBe(5);
    });

    it('debería actualizar comentarioEditando cuando se escribe en el textarea', () => {
      component.comentarioEditando.set('');
      expect(component.comentarioEditando()).toBe('');

      component.comentarioEditando.set('Nuevo comentario');
      expect(component.comentarioEditando()).toBe('Nuevo comentario');
    });

    it('debería actualizar eliminandoId al eliminar', fakeAsync(() => {
      spyOn(window, 'confirm').and.returnValue(true);
      opinionService.eliminarOpinion.and.returnValue(of(void 0).pipe(delay(100)));

      component.misOpiniones.set(mockOpiniones);
      const opinionAEliminar = mockOpiniones[0];
      component.eliminar(opinionAEliminar);

      tick(0);
      expect(component.eliminandoId()).toBe('1');

      tick(100);
      expect(component.eliminandoId()).toBe(null);
    }));
  });

  describe('Integración HTML y lógica', () => {
    beforeEach(() => {
      opinionService.obtenerPorUsuario.and.returnValue(of([]));
    });

    it('debería renderizar el contador de opiniones correcto', () => {
      opinionService.obtenerPorUsuario.and.returnValue(of(mockOpiniones));
      fixture.detectChanges();

      const contador = fixture.nativeElement.textContent;
      expect(contador).toContain('2');
      expect(contador).toContain('opiniones publicadas');
    });

    it('debería mostrar loading spinner cuando estaCargando es true', () => {
      opinionService.obtenerPorUsuario.and.returnValue(new Observable(() => {}));
      fixture.detectChanges();

      const spinner = fixture.nativeElement.querySelector('.animate-spin');
      expect(spinner).toBeTruthy();
    });

    it('debería mostrar empty state cuando no hay opiniones', () => {
      opinionService.obtenerPorUsuario.and.returnValue(of([]));
      fixture.detectChanges();

      const emptyState = fixture.nativeElement.textContent;
      expect(emptyState).toContain('Aún no has opinado');
    });

    it('debería renderizar la lista de opiniones cuando hay datos (estado)', () => {
      opinionService.obtenerPorUsuario.and.returnValue(of(mockOpiniones));
      fixture.detectChanges();

      expect(component.misOpiniones().length).toBeGreaterThan(0);
    });

    it('debería mostrar la cantidad correcta de elementos en la lista (estado)', () => {
      opinionService.obtenerPorUsuario.and.returnValue(of(mockOpiniones));
      fixture.detectChanges();

      expect(component.misOpiniones().length).toBe(mockOpiniones.length);
    });
  });
});
