import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminDashboardComponent } from './admin-dashboard.component';
import { DashboardService } from '../../proxy/estadisticas/dashboard.service';
import { of, throwError } from 'rxjs';

describe('AdminDashboardComponent', () => {
  let component: AdminDashboardComponent;
  let fixture: ComponentFixture<AdminDashboardComponent>;
  let dashboardServiceMock: jasmine.SpyObj<DashboardService>;

  const resumenMock = {
    totalUsuarios: 10,
    totalExperiencias: 20,
    totalOpiniones: 5,
    totalBusquedas: 100,
    totalDestinosGuardados: 7,
    totalLlamadasApi: 50,
    totalErroresApi: 2,
    tiempoPromedioRespuestaMs: 120,
    topBusquedas: [
      { termino: 'París', cantidad: 30 },
      { termino: 'Roma', cantidad: 25 },
      { termino: 'Madrid', cantidad: 20 },
      { termino: 'Londres', cantidad: 15 },
      { termino: 'Barcelona', cantidad: 10 }
    ]
  };

  beforeEach(async () => {
    dashboardServiceMock = jasmine.createSpyObj('DashboardService', ['getResumen']);
    await TestBed.configureTestingModule({
      imports: [AdminDashboardComponent],
      providers: [
        { provide: DashboardService, useValue: dashboardServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboardComponent);
    component = fixture.componentInstance;
  });

  it('debería crearse', () => {
    expect(component).toBeTruthy();
  });

  it('debería cargar el resumen correctamente', () => {
    dashboardServiceMock.getResumen.and.returnValue(of(resumenMock));
    fixture.detectChanges();

    expect(component.resumen).toEqual(resumenMock);
    expect(component.loading).toBeFalse();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Total Usuarios');
    expect(html.textContent).toContain('10');
    expect(html.textContent).toContain('Experiencias');
    expect(html.textContent).toContain('20');
    expect(html.textContent).toContain('Opiniones');
    expect(html.textContent).toContain('5');
    expect(html.textContent).toContain('París');
    expect(html.textContent).toContain('30');
  });

  it('debería manejar errores al cargar el resumen', () => {
    dashboardServiceMock.getResumen.and.returnValue(throwError(() => new Error('Error')));
    fixture.detectChanges();

    expect(component.resumen).toBeNull();
    expect(component.loading).toBeFalse();
  });

  it('debería mostrar mensaje si no hay búsquedas registradas', () => {
    dashboardServiceMock.getResumen.and.returnValue(of({ ...resumenMock, topBusquedas: [] }));
    fixture.detectChanges();

    const html = fixture.nativeElement as HTMLElement;
    expect(html.textContent).toContain('Aún no hay búsquedas registradas.');
  });
});