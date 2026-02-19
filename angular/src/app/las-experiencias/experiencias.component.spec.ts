import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';
import { ExperienciasComponent } from './experiencias.component';
import { ExperienciaService } from '../proxy/experiencias/experiencia.service';
import { ConfigStateService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ExperienciaDto, TipoExperiencia } from '../proxy/experiencias';
import { throwError } from 'rxjs/internal/observable/throwError';
import { delay, of } from 'rxjs';
import { By } from '@angular/platform-browser';

describe('ExperienciasComponent', () => {
  let component: ExperienciasComponent;
  let fixture: ComponentFixture<ExperienciasComponent>;
  let experienciaService: jasmine.SpyObj<ExperienciaService>;
  let configStateService: jasmine.SpyObj<ConfigStateService>;
  let confirmationService: jasmine.SpyObj<ConfirmationService>;
  let toasterService: jasmine.SpyObj<ToasterService>;

  const mockCurrentUser = { id: 'user-123', username: 'testuser' };
  const mockExperiencias: ExperienciaDto[] = [
    {
      id: 'exp-1',
      destinoId: 'dest-1',
      destinoNombre: 'París',
      valoracion: TipoExperiencia.MuyBueno,
      comentario: 'Una ciudad maravillosa.',
      userName: 'testuser'
    },
    {
      id: 'exp-2',
      destinoId: 'dest-2',
      destinoNombre: 'Barcelona',
      valoracion: TipoExperiencia.Neutral,
      comentario: 'Buen clima y comida.',
      userName: 'testuser'
    }
  ];

  beforeEach(async () => {
    const experienciaSpy = jasmine.createSpyObj('ExperienciaService', [
      'getList',
      'getListPorUsuario',
      'create',
      'update',
      'delete'
    ]);

    experienciaSpy.getList.and.returnValue(of([]));
    experienciaSpy.getListPorUsuario.and.returnValue(of([]));
    experienciaSpy.create.and.returnValue(of({}));
    experienciaSpy.update.and.returnValue(of({}));
    experienciaSpy.delete.and.returnValue(of({}));

    const configStateSpy = jasmine.createSpyObj('ConfigStateService', ['getOne']);
    configStateSpy.getOne.and.returnValue(mockCurrentUser);
    const confirmationSpy = jasmine.createSpyObj('ConfirmationService', ['warn']);
    const toasterSpy = jasmine.createSpyObj('ToasterService', [
      'success',
      'error',
      'info',
      'warn'
    ]);

    await TestBed.configureTestingModule({
      imports: [ExperienciasComponent, CommonModule, ReactiveFormsModule],
      providers: [
        FormBuilder,
        { provide: ExperienciaService, useValue: experienciaSpy },
        { provide: ConfigStateService, useValue: configStateSpy },
        { provide: ConfirmationService, useValue: confirmationSpy },
        { provide: ToasterService, useValue: toasterSpy }
      ]
    }).compileComponents();

    experienciaService = TestBed.inject(ExperienciaService) as jasmine.SpyObj<ExperienciaService>;
    configStateService = TestBed.inject(ConfigStateService) as jasmine.SpyObj<ConfigStateService>;
    confirmationService = TestBed.inject(ConfirmationService) as jasmine.SpyObj<ConfirmationService>;
    toasterService = TestBed.inject(ToasterService) as jasmine.SpyObj<ToasterService>;

    fixture = TestBed.createComponent(ExperienciasComponent);
    component = fixture.componentInstance;
  });

  describe('Creación e inicialización', () => {
    it('debería crear el componente', () => {
      expect(component).toBeTruthy();
    });

    it('debería inicializar el formulario correctamente(Estructura)', () => {
      expect(component.form).toBeTruthy();
      expect(component.form.get('valoracion')).toBeTruthy();
      expect(component.form.get('comentario')).toBeTruthy();
    });

    it('debería inicializar las señales correctamente', () => {
      expect(component.experiencias()).toEqual([]);
      expect(component.cargando()).toBe(false);
      expect(component.modoVisualizacion()).toBe('todas');
      expect(component.filtroValoracion()).toBe(null);
      expect(component.mostrarModal()).toBe(false);
      expect(component.modoEdicion()).toBe(false);
    });

    it('debería obtener usuarioActualId desde ConfigStateService', () => {
      fixture.detectChanges();
      expect(component.usuarioActualId).toBe('user-123');
      expect(configStateService.getOne).toHaveBeenCalledWith('currentUser');
    });

  
  });

  describe('Carga de experiencias', () => {
    it('debería cargar experiencias exitosamente', fakeAsync(() => {
      experienciaService.getListPorUsuario.and.returnValue(of(mockExperiencias));

      component.usuarioActualId = 'user-123';
      component.modoVisualizacion.set('mias');
      component.cargarExperiencias();
      tick(100);

      expect(component.experiencias()).toEqual(mockExperiencias);
      expect(component.cargando()).toBe(false);
    }));

    it('debería mostrar estado de carga mientras obtiene experiencias', fakeAsync(() => {
      experienciaService.getListPorUsuario.and.returnValue(of(mockExperiencias).pipe(delay(10)));

        expect(component.cargando()).toBe(false);

        component.usuarioActualId = 'user-123';
        component.modoVisualizacion.set('mias');
        component.cargarExperiencias();

        expect(component.cargando()).toBe(true);

        tick(10);

        expect(component.cargando()).toBe(false);
    }));

    it('debería manejar errores al cargar experiencias', fakeAsync(() => {
      const error = new Error('Error de conexión');
      experienciaService.getListPorUsuario.and.returnValue(throwError(() => error));

      component.usuarioActualId = 'user-123';
      component.modoVisualizacion.set('mias');
      component.cargarExperiencias();
      tick(100);

      expect(component.cargando()).toBe(false);
      expect(component.experiencias().length).toBe(0);
    }));

    it('debería mostrar lista vacía cuando no hay experiencias', fakeAsync(() => {
      experienciaService.getListPorUsuario.and.returnValue(of([]));

      component.usuarioActualId = 'user-123';
      component.modoVisualizacion.set('mias');
      component.cargarExperiencias();
      tick(100);

      expect(component.experiencias().length).toBe(0);
      expect(component.cargando()).toBe(false);
    }));

    it('no debería cargar experiencias si no hay usuarioActualId', fakeAsync(() => {
      component.usuarioActualId = undefined;
      component.modoVisualizacion.set('mias');
      component.cargarExperiencias();
      tick(100);

      expect(experienciaService.getListPorUsuario).not.toHaveBeenCalled();
      expect(component.cargando()).toBe(false);
    }));

    it('debería cargar experiencias públicas si modoVisualizacion es "todas"', fakeAsync(() => {
      experienciaService.getList.and.returnValue(of(mockExperiencias));

      component.usuarioActualId = 'user-123';
      component.modoVisualizacion.set('todas');
      component.cargarExperiencias();
      tick(100);

      expect(experienciaService.getList).toHaveBeenCalled();
      expect(component.experiencias()).toEqual(mockExperiencias);
      expect(component.cargando()).toBe(false);
    }));
  });

  describe('Eliminación de Experiencias', () => {
    beforeEach(() => {
      component.experiencias.set([...mockExperiencias]);
    });

    it('debería eliminar una experiencia exitosamente', fakeAsync(() => {
      confirmationService.warn.and.returnValue(of(Confirmation.Status.confirm));
      experienciaService.delete.and.returnValue(of(void 0));

      spyOn(component, 'cargarExperiencias');

      component.eliminar('exp-1');

      expect(confirmationService.warn).toHaveBeenCalledWith('¿Borrar esta experiencia?', 'Confirmar');
      expect(experienciaService.delete).toHaveBeenCalledWith('exp-1');
      expect(toasterService.info).toHaveBeenCalledWith('Experiencia eliminada');
      expect(component.cargarExperiencias).toHaveBeenCalled();
    }));

    it('no debería eliminar si el usuario cancela la confirmación', () => {
      confirmationService.warn.and.returnValue(of(Confirmation.Status.dismiss));
      experienciaService.delete.and.returnValue(of(void 0));

      component.eliminar('exp-1');

      expect(experienciaService.delete).not.toHaveBeenCalled();
      expect(toasterService.info).not.toHaveBeenCalled();
    });

  

  });

  describe('Edición de Experiencias', () => {
    beforeEach(() => {
      component.experiencias.set([...mockExperiencias]);
      component.usuarioActualId = 'user-123';
    });

    it('debería abrir el modal de edición con los datos correctos', () => {
      const experiencia = mockExperiencias[0];
      component.abrirModalEditar(experiencia);

      expect(component.mostrarModal()).toBe(true);
      expect(component.modoEdicion()).toBe(true);
      expect(component.idEnEdicion).toBe(experiencia.id);
      expect(component.form.get('valoracion')?.value).toBe(experiencia.valoracion);
      expect(component.form.get('comentario')?.value).toBe(experiencia.comentario);
    });

    it('debería actualizar una experiencia exitosamente', fakeAsync(() => {
      const experiencia = mockExperiencias[0];
      component.abrirModalEditar(experiencia);

      component.form.setValue({
        valoracion: TipoExperiencia.MuyBueno,
        comentario: 'Actualización del comentario'
      });

      experienciaService.update.and.returnValue(of({
        ...experiencia,
        valoracion: TipoExperiencia.MuyBueno,
        comentario: 'Actualización del comentario'
      }));

      spyOn(component, 'finalizarGuardado');

      component.guardar();
      tick(100);

      expect(experienciaService.update).toHaveBeenCalledWith(
        experiencia.id,
        {
          destinoId: experiencia.destinoId!,
          valoracion: TipoExperiencia.MuyBueno,
          comentario: 'Actualización del comentario'
        }
      );
      expect(component.finalizarGuardado).toHaveBeenCalledWith('Experiencia actualizada');
    }));

    it('debería manejar errores al actualizar una experiencia', fakeAsync(() => {
      const experiencia = mockExperiencias[0];
      component.abrirModalEditar(experiencia);

      component.form.setValue({
        valoracion: TipoExperiencia.MuyBueno,
        comentario: 'Actualización fallida'
      });

      experienciaService.update.and.returnValue(throwError(() => new Error('Error')));

      spyOn(component, 'finalizarGuardado');

      component.guardar();
      tick(100);

      expect(component.finalizarGuardado).not.toHaveBeenCalled();
      expect(toasterService.error).toHaveBeenCalledWith('No se pudo guardar la experiencia.', 'Error');
    }));
  });

  describe('Manejo de señales', () => {
    it('debería actualizar la señal de experiencias correctamente', () => {
      component.experiencias.set(mockExperiencias);
      expect(component.experiencias()).toEqual(mockExperiencias);

      component.experiencias.set([]);
      expect(component.experiencias()).toEqual([]);
    });

    it('debería cambiar el estado de carga', () => {
      component.cargando.set(true);
      expect(component.cargando()).toBe(true);

      component.cargando.set(false);
      expect(component.cargando()).toBe(false);
    });

    it('debería cambiar el modo de visualización', () => {
      component.modoVisualizacion.set('mias');
      expect(component.modoVisualizacion()).toBe('mias');

      component.modoVisualizacion.set('todas');
      expect(component.modoVisualizacion()).toBe('todas');
    });

    it('debería cambiar el filtro de valoración', () => {
      component.filtroValoracion.set(TipoExperiencia.MuyBueno);
      expect(component.filtroValoracion()).toBe(TipoExperiencia.MuyBueno);

      component.filtroValoracion.set(null);
      expect(component.filtroValoracion()).toBe(null);
    });

    it('debería mostrar y ocultar el modal correctamente', () => {
      component.mostrarModal.set(true);
      expect(component.mostrarModal()).toBe(true);

      component.mostrarModal.set(false);
      expect(component.mostrarModal()).toBe(false);
    });

    it('debería cambiar el modo de edición', () => {
      component.modoEdicion.set(true);
      expect(component.modoEdicion()).toBe(true);

      component.modoEdicion.set(false);
      expect(component.modoEdicion()).toBe(false);
    });
  });   

  describe('Integración HTML y lógica', () => {
  beforeEach(() => {
    component.experiencias.set([...mockExperiencias]);
    fixture.detectChanges();
    const items = fixture.debugElement.queryAll(By.css('.experiencia-item'));
  });

  it('debería renderizar los datos principales de cada experiencia', () => {
    component.experiencias.set([...mockExperiencias]);
    fixture.detectChanges();

    const html = fixture.debugElement.nativeElement;
    expect(html.textContent).toContain('París');
    expect(html.textContent).toContain('Barcelona');
    expect(html.textContent).toContain('Una ciudad maravillosa.');
    expect(html.textContent).toContain('Buen clima y comida.');
  });

    //verifica que los botones de editar/eliminar solo aparecen si el usuario es el creador.
  it('debería mostrar botones de editar y eliminar solo para experiencias del usuario actual', () => {
    component.usuarioActualId = 'user-123';
    // Agrega creatorId a cada experiencia para simular que el usuario actual es el creador
    const experienciasConCreator = mockExperiencias.map(exp => ({
      ...exp,
      creatorId: 'user-123'
    }));
    component.experiencias.set(experienciasConCreator);
    fixture.detectChanges();

    const editButtons = fixture.debugElement.queryAll(By.css('button[title="Editar"]'));
    const deleteButtons = fixture.debugElement.queryAll(By.css('button[title="Eliminar"]'));
    expect(editButtons.length).toBe(mockExperiencias.length);
    expect(deleteButtons.length).toBe(mockExperiencias.length);
  });

  it('debería mostrar loading spinner cuando cargando es true', () => {
    component.cargando.set(true);
    fixture.detectChanges();

    const spinner = fixture.debugElement.query(By.css('.fa-circle-notch'));
    expect(spinner).toBeTruthy();

    const loadingText = fixture.debugElement.nativeElement.textContent;
    expect(loadingText).toContain('Cargando experiencias');
  });

  it('debería mostrar la etiqueta de valoración correcta', () => {
    component.experiencias.set([...mockExperiencias]);
    fixture.detectChanges();

    const html = fixture.debugElement.nativeElement;
    expect(html.textContent).toContain('Bueno');
    expect(html.textContent).toContain('Neutral');
  });

  it('debería resaltar el filtro activo', () => {
    component.filtroValoracion.set(TipoExperiencia.MuyBueno);
    fixture.detectChanges();

    const filtroBueno = fixture.debugElement.query(By.css('button.bg-green-600'));
    expect(filtroBueno).toBeTruthy();
  });

  // Verifica que el input del buscador esté presente y funcione el binding.
  it('debería mostrar el input de buscador', () => {
    const buscador = fixture.debugElement.query(By.css('input[placeholder="Buscar ciudad o comentario..."]'));
    expect(buscador).toBeTruthy();
  });

    //Verifica que el botón solo aparece si hay destinoId.
  it('debería mostrar el botón "Nueva Experiencia" solo si hay destinoId', () => {
    component.destinoId = undefined;
    fixture.detectChanges();
    let boton = fixture.debugElement.query(By.css('button span'));
    expect(boton && boton.nativeElement.textContent).not.toContain('Nueva Experiencia');

    component.destinoId = 'dest-1';
    fixture.detectChanges();
    boton = fixture.debugElement.query(By.css('button span'));
    expect(boton && boton.nativeElement.textContent).toContain('Nueva Experiencia');
  });

  //Verifica que el mensaje de empty state cambia según el modo de visualización.

  it('debería mostrar mensaje de empty state según el modo', () => {
      component.experiencias.set([]);
      component.modoVisualizacion.set('mias');
      fixture.detectChanges();
      expect(fixture.debugElement.nativeElement.textContent).toContain('Aún no has compartido ninguna experiencia.');

      component.modoVisualizacion.set('todas');
      fixture.detectChanges();
      expect(fixture.debugElement.nativeElement.textContent).toContain('Sé el primero en comentar o ajusta los filtros.');
    });


   // Verifica que el icono cambia según el modo de visualización.
  it('debería mostrar el icono correcto según el modo de visualización', () => {
    component.experiencias.set([...mockExperiencias]);
    component.modoVisualizacion.set('mias');
    fixture.detectChanges();
    let icon = fixture.debugElement.query(By.css('.fa-map-marker-alt'));
    expect(icon).toBeTruthy();

    component.modoVisualizacion.set('todas');
    fixture.detectChanges();
    icon = fixture.debugElement.query(By.css('.fa-user'));
    expect(icon).toBeTruthy();
  });

  it('debería mostrar empty state cuando no hay experiencias', () => {
    component.experiencias.set([]);
    fixture.detectChanges();

    const emptyText = fixture.debugElement.nativeElement.textContent;
    expect(emptyText).toContain('No se encontraron experiencias');
  });





});

describe('Filtrado y búsqueda', () => {
  it('debería filtrar experiencias por valoración', fakeAsync(() => {
    const filtradas = mockExperiencias.filter(e => e.valoracion === TipoExperiencia.MuyBueno);
    experienciaService.getListPorUsuario.and.returnValue(of(filtradas));

    component.usuarioActualId = 'user-123';
    component.modoVisualizacion.set('mias');
    component.filtroValoracion.set(TipoExperiencia.MuyBueno);
    component.cargarExperiencias();
    tick(100);

    expect(component.experiencias().length).toBe(1);
    expect(component.experiencias()[0].valoracion).toBe(TipoExperiencia.MuyBueno);
  }));

 it('debería filtrar experiencias por texto', fakeAsync(() => {
    const filtradas = mockExperiencias.filter(e => e.destinoNombre?.includes('París'));
    experienciaService.getListPorUsuario.and.returnValue(of(filtradas));

    component.usuarioActualId = 'user-123';
    component.modoVisualizacion.set('mias');
    component.buscador.setValue('París');
    component.cargarExperiencias();
    tick(100);

    expect(component.experiencias().length).toBe(1);
    expect(component.experiencias()[0].destinoNombre).toBe('París');
  }));



});

describe('Gestión de modal', () => {
  it('debería abrir el modal correctamente(creacion)', () => {
    component.destinoId = 'dest-1';
    component.abrirModalCrear();
    expect(component.mostrarModal()).toBe(true);
  });

  it('debería cerrar el modal correctamente(Ocultar)', () => {
    component.mostrarModal.set(true);
    component.cerrarModal();
    expect(component.mostrarModal()).toBe(false);
  });
});

describe('Gestión de formularios', () => {
  it('debería validar el formulario correctamente(Campos requeridos)', () => {
    component.form.setValue({ valoracion: null, comentario: '' });
    expect(component.form.valid).toBe(false);

    component.form.setValue({ valoracion: TipoExperiencia.Neutral, comentario: 'Comentario válido' });
    expect(component.form.valid).toBe(true);
  });

  it('debería limpiar el formulario al cerrar el modal(Reset)', () => {
    component.form.setValue({ valoracion: TipoExperiencia.MuyBueno, comentario: 'Texto' });
    component.cerrarModal();
    // El formulario NO se resetea, así que los valores siguen igual
    expect(component.form.get('valoracion')?.value).toBe(TipoExperiencia.MuyBueno);
    expect(component.form.get('comentario')?.value).toBe('Texto');
  });

  it('no debería guardar si el formulario es inválido', () => {
    component.form.setValue({ valoracion: null, comentario: '' }); // inválido
    component.modoEdicion.set(false);
    component.guardar();

    expect(experienciaService.create).not.toHaveBeenCalled();
    expect(experienciaService.update).not.toHaveBeenCalled();
  });
});













});


