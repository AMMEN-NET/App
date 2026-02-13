# Guía de Unit Testing con Jasmine en Angular

## 📋 Índice
1. [Estructura Básica](#estructura-básica)
2. [Cómo Funcionan los Tests](#cómo-funcionan-los-tests)
3. [Ejemplo Práctico: MisFavoritosComponent](#ejemplo-práctico-misfavoritoscomponent)
4. [Mocks y Spies](#mocks-y-spies)
5. [Señales en Angular 14+](#señales-en-angular-14)
6. [Patrones Comunes](#patrones-comunes)
7. [Comandos Útiles](#comandos-útiles)

---

## Estructura Básica

### Archivo .spec.ts
El archivo de test debe estar al lado del archivo que quieres probar:

```
mi-favoritos/
├── mis-favoritos.ts        ← Componente
├── mis-favoritos.html      ← Template
├── mis-favoritos.scss      ← Estilos
└── mis-favoritos.spec.ts   ← TESTS ✓
```

### Estructura de un Test

```typescript
describe('Nombre de la Suite', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;
  let servicio: MyService;

  // 1️⃣ BEFOREEACH: Configuración antes de CADA test
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyComponent],
      providers: [MyService]
    }).compileComponents();
  });

  // 2️⃣ TEST INDIVIDUAL
  it('descripción del comportamiento esperado', () => {
    // ARRANGE: Preparar datos
    const expectedValue = 'test';
    
    // ACT: Ejecutar acción
    component.myMethod();
    
    // ASSERT: Verificar resultado
    expect(component.myProperty).toBe(expectedValue);
  });
});
```

---

## Cómo Funcionan los Tests

### 🔄 Ciclo de vida de un test

```
1. describe()      → Agrupar tests relacionados
2. beforeEach()    → Ejecutar ANTES de cada test
3. it()            → Definir test individual
4. expect()        → Hacer aserciones
5. afterEach()     → Ejecutar DESPUÉS de cada test
```

### 📊 Estructura AAA (Arrange - Act - Assert)

```typescript
it('debería actualizar el carrito', () => {
  // ARRANGE: Preparar estado inicial
  const producto = { id: 1, nombre: 'Laptop', precio: 999 };
  component.carrito = [];
  
  // ACT: Ejecutar la acción
  component.agregarAlCarrito(producto);
  
  // ASSERT: Verificar el resultado
  expect(component.carrito.length).toBe(1);
  expect(component.carrito[0].id).toBe(1);
});
```

---

## Ejemplo Práctico: MisFavoritosComponent

### 📁 Estructura del Test

El archivo `mis-favoritos.spec.ts` está organizado en 4 secciones principales:

#### 1. **Creación y Inicialización**
```typescript
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
```

**¿Por qué?**
- Verifica que el componente se instancia correctamente
- Valida el estado inicial de las variables
- Confirma que los hooks de vida (ngOnInit) funcionan

---

#### 2. **Carga de Favoritos**
```typescript
describe('Carga de Favoritos', () => {
  it('debería cargar favoritos exitosamente', (done) => {
    // Mock del servicio
    listadeFavoritosService.obtenerFavoritos.and.returnValue(of(mockFavoritos));

    // Ejecutar
    component.cargarFavoritos();

    // Esperar respuesta asincrónica
    setTimeout(() => {
      expect(component.favoritos()).toEqual(mockFavoritos);
      expect(component.estaCargando()).toBe(false);
      done(); // ✓ Notificar que el test terminó
    }, 100);
  });

  it('debería manejar errores al cargar', (done) => {
    const error = new Error('Error de conexión');
    listadeFavoritosService.obtenerFavoritos.and.returnValue(
      throwError(() => error)
    );

    component.cargarFavoritos();

    setTimeout(() => {
      expect(component.estaCargando()).toBe(false);
      expect(toasterService.error).toHaveBeenCalledWith('Error al cargar la lista.', 'Error');
      done();
    }, 100);
  });
});
```

**Conceptos clave:**
- `done()`: Notifica a Jasmine que una prueba asincrónica terminó
- `throwError()`: Simula un error en una Observable
- `toHaveBeenCalledWith()`: Verifica que se llamó con argumentos específicos

---

#### 3. **Eliminación de Favoritos**
```typescript
describe('Eliminación de Favoritos', () => {
  beforeEach(() => {
    component.favoritos.set([...mockFavoritos]);
  });

  it('debería eliminar un favorito exitosamente', (done) => {
    spyOn(window, 'confirm').and.returnValue(true);
    listadeFavoritosService.eliminarDeFavoritos.and.returnValue(of(void 0));

    component.eliminar('1', 'París');

    setTimeout(() => {
      expect(component.favoritos().length).toBe(1);
      expect(component.favoritos()[0].id).toBe('2');
      expect(component.eliminandoId()).toBe(null);
      done();
    }, 100);
  });

  it('no debería eliminar si el usuario cancela', () => {
    spyOn(window, 'confirm').and.returnValue(false);
    component.eliminar('1', 'París');
    
    expect(listadeFavoritosService.eliminarDeFavoritos).not.toHaveBeenCalled();
  });
});
```

**Patrones importantes:**
- `spyOn(window, 'confirm')`: Simula interacción del usuario
- `expect(...).not.toHaveBeenCalled()`: Verifica que algo NO se llamó
- `update()`: Modificar señales de manera reactiva

---

#### 4. **Integración HTML y Lógica**
```typescript
describe('Integración HTML y lógica', () => {
  beforeEach(() => {
    listadeFavoritosService.obtenerFavoritos.and.returnValue(of([]));
  });

  it('debería renderizar el contador de favoritos', () => {
    component.favoritos.set(mockFavoritos);
    fixture.detectChanges(); // ✓ Ejecutar detección de cambios

    const contador = fixture.nativeElement.textContent;
    expect(contador).toContain('2');
  });

  it('debería mostrar empty state cuando no hay favoritos', () => {
    component.estaCargando.set(false);
    component.favoritos.set([]);
    fixture.detectChanges();

    const emptyState = fixture.nativeElement.textContent;
    expect(emptyState).toContain('Sin destinos guardados');
  });
});
```

**¿Por qué fixture.detectChanges()?**
Angular no detecta cambios automáticamente en tests. Necesitas llamar a `detectChanges()` para:
- Procesar bindings
- Ejecutar detectores de cambios
- Renderizar el template

---

## Mocks y Spies

### 🎭 Crear un Mock con jasmine.createSpyObj

```typescript
beforeEach(() => {
  const listadeFavoritosSpy = jasmine.createSpyObj('ListaDeFavoritosService', [
    'obtenerFavoritos',
    'eliminarDeFavoritos',
    'agregarAFavoritos'
  ]);

  TestBed.configureTestingModule({
    providers: [
      { provide: ListaDeFavoritosService, useValue: listadeFavoritosSpy }
    ]
  });

  service = TestBed.inject(ListaDeFavoritosService);
});
```

### 📍 Configurar el Comportamiento del Mock

```typescript
// Retornar un Observable exitoso
service.obtenerFavoritos.and.returnValue(of(mockData));

// Retornar un error
service.obtenerFavoritos.and.returnValue(throwError(() => new Error('Error')));

// Retornar un Promise
service.obtenerFavoritos.and.returnValue(Promise.resolve(mockData));

// Ejecutar código personalizado
service.obtenerFavoritos.and.callFake(() => {
  return of(mockData);
});
```

### 🔍 Verificar que se Llamó el Mock

```typescript
// Verificar que se llamó
expect(service.obtenerFavoritos).toHaveBeenCalled();

// Verificar que se llamó con argumentos específicos
expect(service.obtenerFavoritos).toHaveBeenCalledWith('123');

// Verificar que se llamó una cantidad específica de veces
expect(service.obtenerFavoritos).toHaveBeenCalledTimes(2);

// Verificar que NO se llamó
expect(service.obtenerFavoritos).not.toHaveBeenCalled();
```

---

## Señales en Angular 14+

### 📡 Testing Señales

```typescript
it('debería actualizar una señal', () => {
  // Crear una señal
  component.favoritos.set(mockFavoritos);
  
  // Leerla
  expect(component.favoritos()).toEqual(mockFavoritos);
  
  // Actualizarla
  component.favoritos.update(lista => 
    lista.filter(f => f.id !== '1')
  );
  
  expect(component.favoritos().length).toBe(1);
});
```

### 🔗 Señales Computadas (computed())

Si tienes una señal computada:
```typescript
// En el componente
favoriteCount = computed(() => this.favoritos().length);

// En el test
it('debería calcular el contador', () => {
  component.favoritos.set(mockFavoritos);
  expect(component.favoriteCount()).toBe(2);
});
```

---

## Patrones Comunes

### ✅ Patrón de Tests Asincronos

```typescript
// OPCIÓN 1: Con done()
it('debería cargar datos', (done) => {
  service.obtenerDatos().subscribe(() => {
    expect(component.datos).toBeDefined();
    done();
  });
});

// OPCIÓN 2: Con async/await (Angular 16+)
it('debería cargar datos', async () => {
  await component.cargarDatos();
  expect(component.datos).toBeDefined();
});

// OPCIÓN 3: Con fakeAsync
import { fakeAsync, tick } from '@angular/core/testing';

it('debería cargar datos', fakeAsync(() => {
  component.cargarDatos();
  tick(1000); // Avanzar 1000ms en el tiempo
  expect(component.datos).toBeDefined();
}));
```

---

### 🎨 Patrón de Tests de Componentes con Servicios

```typescript
describe('MyComponent', () => {
  let component: MyComponent;
  let fixture: ComponentFixture<MyComponent>;
  let myService: jasmine.SpyObj<MyService>;

  beforeEach(async () => {
    const serviceSpy = jasmine.createSpyObj('MyService', ['getData']);

    await TestBed.configureTestingModule({
      imports: [MyComponent],
      providers: [
        { provide: MyService, useValue: serviceSpy }
      ]
    }).compileComponents();

    myService = TestBed.inject(MyService) as jasmine.SpyObj<MyService>;
    fixture = TestBed.createComponent(MyComponent);
    component = fixture.componentInstance;
  });

  it('debería llamar al servicio en ngOnInit', () => {
    myService.getData.and.returnValue(of([]));
    component.ngOnInit();
    expect(myService.getData).toHaveBeenCalled();
  });
});
```

---

### 🧪 Patrón de Tests de Métodos con Efectos Secundarios

```typescript
it('debería actualizar estado y llamar al servicio', (done) => {
  // ARRANGE
  const id = '123';
  myService.deleteItem.and.returnValue(of(null));

  // ACT
  component.deleteItem(id);

  // ASSERT
  setTimeout(() => {
    expect(component.items()).not.toContainEqual(jasmine.objectContaining({ id }));
    expect(myService.deleteItem).toHaveBeenCalledWith(id);
    expect(toasterService.success).toHaveBeenCalled();
    done();
  }, 100);
});
```

---

## Comandos Útiles

### 🚀 Ejecutar Tests

```bash
# Ejecutar todos los tests en modo watch (desarrollo)
ng test

# Ejecutar todos los tests una sola vez (CI/CD)
ng test --watch=false

# Ejecutar un archivo específico
ng test --include='**/mis-favoritos.spec.ts'

# Ejecutar con cobertura de código
ng test --no-watch --code-coverage

# Ejecutar en headless (sin interfaz gráfica)
ng test --watch=false --browsers=ChromeHeadless
```

### 📊 Ver Cobertura de Código

```bash
ng test --no-watch --code-coverage
```

Esto genera un reporte en `coverage/` con el porcentaje de líneas testeadas.

---

## 📝 Checklist para Escribir Buenos Tests

- ✅ Un test = una cosa a probar
- ✅ Nombres descriptivos en `it('debería...')`
- ✅ Usar AAA pattern (Arrange-Act-Assert)
- ✅ Mocks de dependencias externas
- ✅ No usar valores hardcodeados (usar variables de setup)
- ✅ Cleanup en `afterEach()` si es necesario
- ✅ Evitar `.toBeTruthy()` / `.toBeFalsy()`, ser específico
- ✅ Probar errores además de casos exitosos
- ✅ Tests independientes (no depender del orden)

---

## Ejemplo Completo: Servicio

```typescript
// favoritos.service.spec.ts
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FavoritosService } from './favoritos.service';

describe('FavoritosService', () => {
  let service: FavoritosService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [FavoritosService]
    });

    service = TestBed.inject(FavoritosService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify(); // ✓ Verificar que no haya peticiones pendientes
  });

  it('debería obtener favoritos', () => {
    const mockFavoritos = [
      { id: '1', nombre: 'París' }
    ];

    service.obtenerFavoritos().subscribe(data => {
      expect(data.length).toBe(1);
      expect(data[0].nombre).toBe('París');
    });

    const req = httpMock.expectOne('/api/favoritos');
    expect(req.request.method).toBe('GET');
    req.flush(mockFavoritos);
  });

  it('debería manejar errores', () => {
    service.obtenerFavoritos().subscribe(
      () => fail('debería haber fallado'),
      error => {
        expect(error.status).toBe(404);
      }
    );

    const req = httpMock.expectOne('/api/favoritos');
    req.flush('Not found', { status: 404, statusText: 'Not Found' });
  });
});
```

---

## 🎯 Próximos Pasos

1. **Ejecuta los tests**: `ng test`
2. **Observa qué falla**: Lee los mensajes de error
3. **Arregla el código**: No los tests
4. **Verifica cobertura**: `ng test --code-coverage`
5. **Repite**: Hasta tener >80% de cobertura

---

## 📚 Referencias Útiles

- [Jasmine Documentation](https://jasmine.github.io/)
- [Angular Testing Guide](https://angular.io/guide/testing)
- [Karma Runner](https://karma-runner.github.io/)
