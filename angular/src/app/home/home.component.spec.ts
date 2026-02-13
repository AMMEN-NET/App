/*import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HomeComponent } from './home.component';

describe('HomeComponent', () => {       //Agrupa todos los tests del componente HomeComponent
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  beforeEach(async () => {          //Se ejecuta antes de cada test (si tengo 10 test, se ejecuta 10 veces)
    await TestBed.configureTestingModule({
      imports: [HomeComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);       //Crea una instancia del componente
    component = fixture.componentInstance;                  //Obtiene la instancia del componente desde el fixture
    fixture.detectChanges();                                //Detecta los cambios y actualiza la vista
  });

  it('debería crear el componente', () => {                 //Primer test: Verifica que el componente se crea correctamente
    expect(component).toBeTruthy();
  });

  // Agregar más tests aquí
  it('debería renderizar el título', () => {                //Segundo test: Verifica que el título se renderiza correctamente   
    const title = fixture.nativeElement.querySelector('h1');
    expect(title).toBeTruthy();
  });
});*/