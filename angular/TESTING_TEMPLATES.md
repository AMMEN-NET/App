# Templates para Crear Tests Rápidamente

Copia y pega estos templates para crear tests nuevos rápidamente. Solo necesitas reemplazar los nombres.

---

## 🎯 Template 1: Test Básico de Componente

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MyComponent } from './my.component';
import { MyService } from '../services/my.service';

describe('MyComponent', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;
  let myService: jasmine.SpyObj<MyService>;

  beforeEach(async () => {
    const myServiceSpy = jasmine.createSpyObj('MyService', [
      'getData',
      'saveData',
      'deleteData'
    ]);

    await TestBed.configureTestingModule({
      imports: [MyComponent],
      providers: [
        { provide: MyService, useValue: myServiceSpy }
      ]
    }).compileComponents();

    myService = TestBed.inject(MyService) as jasmine.SpyObj<MyService>;
    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
  });

  describe('Inicialización', () => {
    it('debería crear el componente', () => {
      expect(component).toBeTruthy();
    });

    it('debería inicializar con valores por defecto', () => {
      expect(component.data).toBeDefined();
    });
  });

  describe('Funcionalidad Principal', () => {
    it('debería hacer algo cuando se ejecuta', (done) => {
      myService.getData.and.returnValue(Promise.resolve([]));

      component.ngOnInit();

      setTimeout(() => {
        expect(myService.getData).toHaveBeenCalled();
        done();
      }, 100);
    });
  });

  describe('Manejo de Errores', () => {
    it('debería manejar errores correctamente', (done) => {
      myService.getData.and.returnValue(Promise.reject(new Error('Error')));

      component.ngOnInit();

      setTimeout(() => {
        // Verificar que se manejó el error
        done();
      }, 100);
    });
  });
});
```

---

## 🎯 Template 2: Test de Servicio con HTTP

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { MyService } from './my.service';

describe('MyService', () => {
  let service: MyService;
  let httpMock: HttpTestingController;

  const mockData = {
    id: '1',
    nombre: 'Test'
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [MyService]
    });

    service = TestBed.inject(MyService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('GET Requests', () => {
    it('debería obtener datos', (done) => {
      service.getData().subscribe(data => {
        expect(data).toEqual(mockData);
        done();
      });

      const req = httpMock.expectOne('/api/endpoint');
      expect(req.request.method).toBe('GET');
      req.flush(mockData);
    });
  });

  describe('POST Requests', () => {
    it('debería guardar datos', (done) => {
      service.saveData(mockData).subscribe(data => {
        expect(data.id).toBe('1');
        done();
      });

      const req = httpMock.expectOne('/api/endpoint');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(mockData);
      req.flush(mockData);
    });
  });

  describe('DELETE Requests', () => {
    it('debería eliminar datos', (done) => {
      service.deleteData('1').subscribe(() => {
        expect(true).toBe(true);
        done();
      });

      const req = httpMock.expectOne('/api/endpoint/1');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('Manejo de Errores HTTP', () => {
    it('debería manejar error 404', (done) => {
      service.getData().subscribe(
        () => fail('debería fallar'),
        error => {
          expect(error.status).toBe(404);
          done();
        }
      );

      const req = httpMock.expectOne('/api/endpoint');
      req.flush('Not found', { status: 404, statusText: 'Not Found' });
    });

    it('debería manejar error 500', (done) => {
      service.getData().subscribe(
        () => fail('debería fallar'),
        error => {
          expect(error.status).toBe(500);
          done();
        }
      );

      const req = httpMock.expectOne('/api/endpoint');
      req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
    });
  });
});
```

---

## 🎯 Template 3: Test de Señales (Signals)

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { MyComponent } from './my.component';

describe('MyComponent - Signals', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
  });

  describe('Señales', () => {
    it('debería inicializar señales', () => {
      expect(component.isLoading()).toBe(false);
      expect(component.items()).toEqual([]);
    });

    it('debería actualizar una señal', () => {
      component.isLoading.set(true);
      expect(component.isLoading()).toBe(true);
    });

    it('debería actualizar señal con update()', () => {
      component.items.set([{ id: 1 }, { id: 2 }]);
      
      component.items.update(list => [...list, { id: 3 }]);
      
      expect(component.items().length).toBe(3);
    });

    it('debería filtrar items con update()', () => {
      component.items.set([{ id: 1, name: 'A' }, { id: 2, name: 'B' }]);
      
      component.items.update(list => list.filter(item => item.id !== 1));
      
      expect(component.items().length).toBe(1);
      expect(component.items()[0].id).toBe(2);
    });
  });

  describe('Computed Signals', () => {
    it('debería calcular valor computado', () => {
      component.items.set([1, 2, 3]);
      
      // Si tienes una señal computada de cantidad:
      // const count = computed(() => this.items().length);
      
      // expect(component.count()).toBe(3);
    });
  });

  describe('Efectos con Signals', () => {
    it('debería ejecutar efecto cuando cambia señal', (done) => {
      let efectoEjecutado = false;

      component.isLoading.set(true);

      setTimeout(() => {
        // Verificar si el efecto se ejecutó
        done();
      }, 100);
    });
  });
});
```

---

## 🎯 Template 4: Test de Componente con Formulario

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MyFormComponent } from './my-form.component';
import { MyService } from '../services/my.service';

describe('MyFormComponent', () => {
  let component: MyFormComponent;
  let fixture: ComponentFixture<MyFormComponent>;
  let myService: jasmine.SpyObj<MyService>;

  beforeEach(async () => {
    const myServiceSpy = jasmine.createSpyObj('MyService', ['submitForm']);

    await TestBed.configureTestingModule({
      imports: [MyFormComponent, ReactiveFormsModule, FormsModule],
      providers: [
        { provide: MyService, useValue: myServiceSpy }
      ]
    }).compileComponents();

    myService = TestBed.inject(MyService) as jasmine.SpyObj<MyService>;
    fixture = TestBed.createComponent(MyFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Validación de Formulario', () => {
    it('el formulario debería ser inválido sin datos', () => {
      const form = component.myForm;
      expect(form.valid).toBe(false);
    });

    it('el formulario debería ser válido con datos correctos', () => {
      const form = component.myForm;
      form.controls['email'].setValue('test@example.com');
      form.controls['password'].setValue('password123');
      
      expect(form.valid).toBe(true);
    });

    it('debería validar email correctamente', () => {
      const emailControl = component.myForm.controls['email'];
      
      emailControl.setValue('invalid-email');
      expect(emailControl.hasError('email')).toBe(true);
      
      emailControl.setValue('valid@example.com');
      expect(emailControl.hasError('email')).toBe(false);
    });

    it('debería validar campo requerido', () => {
      const nameControl = component.myForm.controls['name'];
      
      nameControl.setValue('');
      expect(nameControl.hasError('required')).toBe(true);
      
      nameControl.setValue('John');
      expect(nameControl.hasError('required')).toBe(false);
    });
  });

  describe('Envío de Formulario', () => {
    it('debería enviar el formulario cuando es válido', () => {
      myService.submitForm.and.returnValue(Promise.resolve({}));
      
      const form = component.myForm;
      form.controls['email'].setValue('test@example.com');
      form.controls['password'].setValue('password123');

      component.onSubmit();

      expect(myService.submitForm).toHaveBeenCalled();
    });

    it('no debería enviar el formulario cuando es inválido', () => {
      component.onSubmit();
      expect(myService.submitForm).not.toHaveBeenCalled();
    });
  });
});
```

---

## 🎯 Template 5: Test de Interacción DOM

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MyComponent } from './my.component';

describe('MyComponent - DOM Interaction', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent]
    }).compileComponents();

    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('Click Events', () => {
    it('debería manejar click en botón', () => {
      spyOn(component, 'onButtonClick');

      const button = fixture.nativeElement.querySelector('button');
      button.click();

      expect(component.onButtonClick).toHaveBeenCalled();
    });

    it('debería cambiar estado al hacer click', () => {
      component.isActive = false;
      
      const button = fixture.nativeElement.querySelector('button');
      button.click();

      expect(component.isActive).toBe(true);
    });
  });

  describe('Input Events', () => {
    it('debería actualizar modelo con input', () => {
      const input = fixture.nativeElement.querySelector('input');
      input.value = 'test value';
      input.dispatchEvent(new Event('input'));

      expect(component.text).toBe('test value');
    });
  });

  describe('Renderización Condicional', () => {
    it('debería mostrar elemento cuando condición es true', () => {
      component.showElement = true;
      fixture.detectChanges();

      const element = fixture.nativeElement.querySelector('.hidden-element');
      expect(element).toBeTruthy();
    });

    it('debería ocultar elemento cuando condición es false', () => {
      component.showElement = false;
      fixture.detectChanges();

      const element = fixture.nativeElement.querySelector('.hidden-element');
      expect(element).toBeFalsy();
    });
  });

  describe('Lista (*ngFor o @for)', () => {
    it('debería renderizar lista con items', () => {
      component.items = [1, 2, 3];
      fixture.detectChanges();

      const items = fixture.nativeElement.querySelectorAll('.list-item');
      expect(items.length).toBe(3);
    });

    it('debería actualizar lista dinámicamente', () => {
      component.items = [1, 2];
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelectorAll('.list-item').length).toBe(2);

      component.items.push(3);
      fixture.detectChanges();

      expect(fixture.nativeElement.querySelectorAll('.list-item').length).toBe(3);
    });
  });

  describe('Clases CSS', () => {
    it('debería aplicar clase cuando condición es true', () => {
      component.hasError = true;
      fixture.detectChanges();

      const element = fixture.nativeElement.querySelector('.input-field');
      expect(element.classList.contains('error')).toBe(true);
    });

    it('debería remover clase cuando condición es false', () => {
      component.hasError = false;
      fixture.detectChanges();

      const element = fixture.nativeElement.querySelector('.input-field');
      expect(element.classList.contains('error')).toBe(false);
    });
  });
});
```

---

## 🎯 Template 6: Test de Rutas (Routing)

```typescript
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { Location } from '@angular/common';
import { MyComponent } from './my.component';

describe('MyComponent - Routing', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;
  let router: Router;
  let location: Location;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent],
      // Aquí va tu configuración de rutas
      // providers: [provideRouter(routes)]
    }).compileComponents();

    router = TestBed.inject(Router);
    location = TestBed.inject(Location);
    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
  });

  describe('Navegación', () => {
    it('debería navegar a ruta', (done) => {
      router.navigate(['/home']).then(() => {
        expect(location.path()).toBe('/home');
        done();
      });
    });

    it('debería navegar con parámetros', (done) => {
      router.navigate(['/profile', '123']).then(() => {
        expect(location.path()).toBe('/profile/123');
        done();
      });
    });
  });

  describe('RouterLink', () => {
    it('debería tener routerLink configurado', () => {
      const link = fixture.nativeElement.querySelector('[routerLink]');
      expect(link).toBeTruthy();
      expect(link.getAttribute('routerLink')).toBe('/home');
    });
  });

  describe('Parámetros de Ruta', () => {
    it('debería leer parámetros de ruta', (done) => {
      router.navigate(['/profile', '123']).then(() => {
        // Verificar que el componente leyó el parámetro
        done();
      });
    });
  });
});
```

---

## 🚀 Quick Reference

### Importaciones Comunes
```typescript
// Testing
import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';

// HTTP Testing
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

// RxJS
import { of, throwError } from 'rxjs';

// Angular
import { signal, computed } from '@angular/core';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
```

### Matchers Comunes
```typescript
// Igualdad
expect(value).toBe(expectedValue);           // Igualdad estricta
expect(value).toEqual(expectedValue);        // Igualdad profunda
expect(value).toContain(item);               // Array contiene item
expect(value).toMatch(/pattern/);            // Regex match

// Booleans
expect(value).toBeTruthy();
expect(value).toBeFalsy();

// Null / Undefined
expect(value).toBeNull();
expect(value).toBeUndefined();
expect(value).toBeDefined();

// Números
expect(value).toBeGreaterThan(5);
expect(value).toBeLessThan(10);
expect(value).toBeCloseTo(3.14, 2);

// Excepciones
expect(() => myFunction()).toThrow();
expect(() => myFunction()).toThrowError('message');

// Funciones
expect(spy).toHaveBeenCalled();
expect(spy).toHaveBeenCalledWith(arg1, arg2);
expect(spy).toHaveBeenCalledTimes(2);
```

---

## 📋 Checklist Básico

- ✅ Importar `TestBed` y `ComponentFixture`
- ✅ Crear mocks de dependencias
- ✅ Configurar `beforeEach(async)`
- ✅ Crear fixture y componente
- ✅ Dividir en `describe()` temáticos
- ✅ Usar `it()` con nombres descriptivos
- ✅ Seguir patrón AAA (Arrange-Act-Assert)
- ✅ Probar casos exitosos Y errores
- ✅ Usar `done()` para async
- ✅ Ejecutar: `ng test`
