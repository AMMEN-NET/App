import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HomeComponent } from './home.component';
import { DestinoService } from '../proxy/destinos';
import { ToasterService } from '@abp/ng.theme.shared';
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos';
import { OpinionService } from '../proxy/opiniones/opinion.service';
import { ExperienciaService } from '../proxy/experiencias/experiencia.service';
import { ConfigStateService } from '@abp/ng.core';
import { FormBuilder } from '@angular/forms';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        { provide: DestinoService, useValue: {} },
        { provide: ToasterService, useValue: {} },
        { provide: ListaDeFavoritosService, useValue: { obtenerFavoritos: () => ({ subscribe: () => {} }) } },
        { provide: OpinionService, useValue: {} },
        { provide: ExperienciaService, useValue: {} },
        { provide: ConfigStateService, useValue: { getOne: () => ({ name: 'TestUser' }) } },
        { provide: DestinoService, useValue: { buscarCiudades: () => ({ subscribe: () => {} }) } },
        FormBuilder
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('debería crear el componente', () => {
    expect(component).toBeTruthy();
  });

  it('debería renderizar el título', () => {
    const title = fixture.nativeElement.querySelector('h1');
    expect(title).toBeTruthy();
  });

  it('debería renderizar el saludo de bienvenida', () => {
    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('¡Bienvenido');
  });

  it('debería mostrar el saludo con el nombre del usuario', () => {
    // Crea el spy antes de crear el componente
    const configStateMock = TestBed.inject(ConfigStateService) as any;
    spyOn(configStateMock, 'getOne').and.returnValue({ name: 'Test1' });


    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('¡Bienvenido Test1!');
  });

  it('debería mostrar los inputs de búsqueda', () => {
    const html = fixture.nativeElement as HTMLElement;
    expect(html.querySelector('input[placeholder="Ej. Paris"]')).toBeTruthy();
    expect(html.querySelector('input[placeholder="Ej. France"]')).toBeTruthy();
    expect(html.querySelector('input[placeholder="0"]')).toBeTruthy();
  });

  it('debería llamar a buscarCiudades al hacer click en "Buscar"', () => {
    fixture.detectChanges();
    spyOn(component, 'buscarCiudades');
    const button = fixture.debugElement.nativeElement.querySelector('button');
    button.click();
    expect(component.buscarCiudades).toHaveBeenCalled();
  });

  it('debería mostrar resultados de ciudades si existen', () => {
    component.ciudades.set({
      ciudades: [
        { id: '1', nombre: 'París', pais: 'Francia', geoDBId: 'paris', poblacion: 2000000, latitud: 48.85, longitud: 2.35 }
      ]
    } as any);
    fixture.detectChanges();
    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('París');
    expect(html.textContent).toContain('Francia');
  });

  it('debería mostrar mensaje de error si hay error', () => {
    component.error.set('No se pudieron cargar los resultados.');
    fixture.detectChanges();
    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Error de conexión');
    expect(html.textContent).toContain('No se pudieron cargar los resultados.');
  });

  it('debería mostrar spinner cuando está cargando', () => {
    component.estaCargando.set(true);
    fixture.detectChanges();
    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('◷');
  });

  it('no debería buscar si todos los filtros están vacíos', () => {
    spyOn(component['destinoService'], 'buscarCiudades');
    component.nombre.set('');
    component.pais.set('');
    component.poblacion.set(null);
    component.buscarCiudades();
    expect(component['destinoService'].buscarCiudades).not.toHaveBeenCalled();
  });

});