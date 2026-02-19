# Ejemplos de Unit Tests para Servicios - Tu Proyecto

## 📝 Tabla de Contenidos
1. [ListaDeFavoritosService](#listadefavoritosservice)
2. [DashboardService](#dashboardservice)
3. [ExperienciaService](#experienciaservice)
4. [FotoPerfilService](#fotoperfilservice)

---

## ListaDeFavoritosService

Este servicio es usado por MisFavoritosComponent y necesita tests para todas sus operaciones.

### Archivo: `src/app/proxy/lista-de-favoritos/lista-de-favoritos.service.spec.ts`

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RestService } from '@abp/ng.core';
import { ListaDeFavoritosService } from './lista-de-favoritos.service';
import { FavoritoDto } from '../favoritos/favoritos-dto';

describe('ListaDeFavoritosService', () => {
  let service: ListaDeFavoritosService;
  let httpMock: HttpTestingController;
  let restService: jasmine.SpyObj<RestService>;

  const mockFavoritos: FavoritoDto[] = [
    {
      id: '1',
      nombre: 'París',
      pais: 'Francia',
      poblacion: 2161000,
      latitud: 48.8566,
      longitud: 2.3522,
      promedioPuntuacion: 4.5,
      cantidadOpiniones: 120
    }
  ];

  beforeEach(() => {
    const restServiceSpy = jasmine.createSpyObj('RestService', ['request']);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        ListaDeFavoritosService,
        { provide: RestService, useValue: restServiceSpy }
      ]
    });

    service = TestBed.inject(ListaDeFavoritosService);
    restService = TestBed.inject(RestService) as jasmine.SpyObj<RestService>;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('obtenerFavoritos', () => {
    it('debería obtener la lista de favoritos', () => {
      restService.request.and.returnValue(Promise.resolve(mockFavoritos));

      service.obtenerFavoritos().subscribe(data => {
        expect(data).toEqual(mockFavoritos);
        expect(data.length).toBe(1);
      });

      expect(restService.request).toHaveBeenCalledWith(
        jasmine.objectContaining({
          method: 'POST',
          url: '/api/app/lista-de-favoritos/obtener-favoritos'
        }),
        jasmine.any(Object)
      );
    });

    it('debería retornar array vacío si no hay favoritos', () => {
      restService.request.and.returnValue(Promise.resolve([]));

      service.obtenerFavoritos().subscribe(data => {
        expect(data).toEqual([]);
        expect(data.length).toBe(0);
      });
    });
  });

  describe('agregarAFavoritos', () => {
    it('debería agregar un destino a favoritos', () => {
      const destinoId = '123';
      restService.request.and.returnValue(Promise.resolve(void 0));

      service.agregarAFavoritos(destinoId).subscribe(() => {
        expect(restService.request).toHaveBeenCalled();
      });

      expect(restService.request).toHaveBeenCalledWith(
        jasmine.objectContaining({
          method: 'POST',
          url: `/api/app/lista-de-favoritos/agregar-aFavoritos/${destinoId}`
        }),
        jasmine.any(Object)
      );
    });
  });

  describe('eliminarDeFavoritos', () => {
    it('debería eliminar un destino de favoritos', () => {
      const destinoId = '1';
      restService.request.and.returnValue(Promise.resolve(void 0));

      service.eliminarDeFavoritos(destinoId).subscribe(() => {
        expect(restService.request).toHaveBeenCalled();
      });

      expect(restService.request).toHaveBeenCalledWith(
        jasmine.objectContaining({
          method: 'POST',
          url: `/api/app/lista-de-favoritos/eliminar-de-favoritos/${destinoId}`
        }),
        jasmine.any(Object)
      );
    });
  });

  describe('esFavorito', () => {
    it('debería verificar si un destino es favorito', () => {
      const destinoId = '1';
      restService.request.and.returnValue(Promise.resolve(true));

      service.esFavorito(destinoId).subscribe(resultado => {
        expect(resultado).toBe(true);
      });
    });

    it('debería retornar false si no es favorito', () => {
      const destinoId = '999';
      restService.request.and.returnValue(Promise.resolve(false));

      service.esFavorito(destinoId).subscribe(resultado => {
        expect(resultado).toBe(false);
      });
    });
  });

  describe('contarFavoritos', () => {
    it('debería contar la cantidad de favoritos', () => {
      restService.request.and.returnValue(Promise.resolve(5));

      service.contarFavoritos().subscribe(count => {
        expect(count).toBe(5);
        expect(typeof count).toBe('number');
      });
    });
  });

  describe('vaciarFavoritos', () => {
    it('debería vaciar todos los favoritos', () => {
      restService.request.and.returnValue(Promise.resolve(void 0));

      service.vaciarFavoritos().subscribe(() => {
        expect(restService.request).toHaveBeenCalled();
      });

      expect(restService.request).toHaveBeenCalledWith(
        jasmine.objectContaining({
          method: 'POST',
          url: '/api/app/lista-de-favoritos/vaciar-favoritos'
        }),
        jasmine.any(Object)
      );
    });
  });

  describe('agregarFavoritoDesdeBusqueda', () => {
    it('debería agregar un favorito desde búsqueda externa', () => {
      const ciudadExterna = {
        id: 'geo-123',
        nombre: 'Barcelona',
        pais: 'España',
        poblacion: 1620000,
        latitud: 41.3874,
        longitud: 2.1686
      };

      restService.request.and.returnValue(Promise.resolve(void 0));

      service.agregarFavoritoDesdeBusqueda(ciudadExterna).subscribe(() => {
        expect(restService.request).toHaveBeenCalled();
      });

      expect(restService.request).toHaveBeenCalledWith(
        jasmine.objectContaining({
          method: 'POST',
          url: '/api/app/lista-de-favoritos/agregar-favorito-desde-busqueda',
          body: ciudadExterna
        }),
        jasmine.any(Object)
      );
    });
  });
});
```

---

## DashboardService

Para el panel del administrador que muestra métricas.

### Archivo: `src/app/proxy/estadisticas/dashboard.service.spec.ts`

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { DashboardService } from './dashboard.service';
import { DashboardMetricsDto } from './models';

describe('DashboardService', () => {
  let service: DashboardService;
  let httpMock: HttpTestingController;

  const mockMetricas: DashboardMetricsDto = {
    totalSearches: 1250,
    topCities: ['París', 'Barcelona', 'Madrid'],
    avgResponseTime: 245,
    errorRate: 0.5
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [DashboardService]
    });

    service = TestBed.inject(DashboardService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('obtenerMetricas', () => {
    it('debería obtener las métricas del dashboard', (done) => {
      service.obtenerMetricas().subscribe(data => {
        expect(data.totalSearches).toBe(1250);
        expect(data.topCities.length).toBe(3);
        expect(data.avgResponseTime).toBe(245);
        done();
      });

      const req = httpMock.expectOne('/api/app/dashboard/metricas');
      expect(req.request.method).toBe('GET');
      req.flush(mockMetricas);
    });

    it('debería manejar errores al obtener métricas', (done) => {
      service.obtenerMetricas().subscribe(
        () => fail('debería haber fallado'),
        error => {
          expect(error.status).toBe(500);
          done();
        }
      );

      const req = httpMock.expectOne('/api/app/dashboard/metricas');
      req.flush('Error interno del servidor', { status: 500, statusText: 'Internal Server Error' });
    });
  });

  describe('obtenerCiudadesMasConsultadas', () => {
    it('debería obtener las ciudades más consultadas', (done) => {
      const ciudadesMasConsultadas = ['París', 'Barcelona', 'Madrid', 'Roma', 'Berlín'];

      service.obtenerCiudadesMasConsultadas().subscribe(data => {
        expect(data).toEqual(ciudadesMasConsultadas);
        expect(data.length).toBe(5);
        done();
      });

      const req = httpMock.expectOne('/api/app/dashboard/ciudades-mas-consultadas');
      req.flush(ciudadesMasConsultadas);
    });
  });

  describe('obtenerTiemposPromedio', () => {
    it('debería obtener los tiempos promedio de respuesta', (done) => {
      const tiempos = {
        min: 150,
        max: 500,
        average: 245
      };

      service.obtenerTiemposPromedio().subscribe(data => {
        expect(data.average).toBe(245);
        expect(data.min).toBeLessThan(data.max);
        done();
      });

      const req = httpMock.expectOne('/api/app/dashboard/tiempos-promedio');
      req.flush(tiempos);
    });
  });

  describe('obtenerLogs', () => {
    it('debería obtener los logs del sistema', (done) => {
      const logs = [
        { timestamp: new Date(), level: 'ERROR', message: 'Conexión perdida' },
        { timestamp: new Date(), level: 'INFO', message: 'Usuario conectado' }
      ];

      service.obtenerLogs().subscribe(data => {
        expect(data.length).toBe(2);
        expect(data[0].level).toBe('ERROR');
        done();
      });

      const req = httpMock.expectOne('/api/app/dashboard/logs');
      req.flush(logs);
    });

    it('debería filtrar logs por nivel', (done) => {
      const logsError = [
        { timestamp: new Date(), level: 'ERROR', message: 'Error 1' }
      ];

      service.obtenerLogs('ERROR').subscribe(data => {
        expect(data.every(log => log.level === 'ERROR')).toBe(true);
        done();
      });

      const req = httpMock.expectOne('/api/app/dashboard/logs?level=ERROR');
      req.flush(logsError);
    });
  });
});
```

---

## ExperienciaService

Para buscar y gestionar experiencias turísticas.

### Archivo: `src/app/proxy/experiencias/experiencia.service.spec.ts`

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ExperienciaService } from './experiencia.service';
import { ExperienciaDto } from './models';

describe('ExperienciaService', () => {
  let service: ExperienciaService;
  let httpMock: HttpTestingController;

  const mockExperiencias: ExperienciaDto[] = [
    {
      id: '1',
      nombre: 'Tour Eiffel',
      ciudad: 'París',
      descripcion: 'Visita a la icónica Torre Eiffel',
      precio: 15.50,
      duracion: '2 horas',
      puntuacion: 4.8,
      imagenUrl: 'https://...'
    },
    {
      id: '2',
      nombre: 'Museo del Louvre',
      ciudad: 'París',
      descripcion: 'Recorrido por el museo más visitado del mundo',
      precio: 20.00,
      duracion: '3 horas',
      puntuacion: 4.7,
      imagenUrl: 'https://...'
    }
  ];

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [ExperienciaService]
    });

    service = TestBed.inject(ExperienciaService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('buscarPorCiudad', () => {
    it('debería buscar experiencias por ciudad', (done) => {
      const ciudad = 'París';

      service.buscarPorCiudad(ciudad).subscribe(data => {
        expect(data.length).toBe(2);
        expect(data.every(exp => exp.ciudad === ciudad)).toBe(true);
        done();
      });

      const req = httpMock.expectOne(`/api/app/experiencias?ciudad=${ciudad}`);
      expect(req.request.method).toBe('GET');
      req.flush(mockExperiencias);
    });

    it('debería retornar lista vacía si no hay experiencias', (done) => {
      service.buscarPorCiudad('NoExiste').subscribe(data => {
        expect(data).toEqual([]);
        done();
      });

      const req = httpMock.expectOne(`/api/app/experiencias?ciudad=NoExiste`);
      req.flush([]);
    });
  });

  describe('obtenerPorId', () => {
    it('debería obtener una experiencia por ID', (done) => {
      const id = '1';
      const mockExperiencia = mockExperiencias[0];

      service.obtenerPorId(id).subscribe(data => {
        expect(data.id).toBe(id);
        expect(data.nombre).toBe('Tour Eiffel');
        done();
      });

      const req = httpMock.expectOne(`/api/app/experiencias/${id}`);
      req.flush(mockExperiencia);
    });

    it('debería manejar error 404 cuando no existe', (done) => {
      service.obtenerPorId('999').subscribe(
        () => fail('debería haber fallado'),
        error => {
          expect(error.status).toBe(404);
          done();
        }
      );

      const req = httpMock.expectOne('/api/app/experiencias/999');
      req.flush('No encontrado', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('crearExperiencia', () => {
    it('debería crear una nueva experiencia', (done) => {
      const nuevaExperiencia = {
        nombre: 'Paseo en barco',
        ciudad: 'Barcelona',
        descripcion: 'Recorrido por la costa',
        precio: 25,
        duracion: '1.5 horas'
      };

      service.crear(nuevaExperiencia).subscribe(data => {
        expect(data.id).toBeDefined();
        expect(data.nombre).toBe(nuevaExperiencia.nombre);
        done();
      });

      const req = httpMock.expectOne('/api/app/experiencias');
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(nuevaExperiencia);
      req.flush({ ...nuevaExperiencia, id: '3' });
    });
  });

  describe('actualizarExperiencia', () => {
    it('debería actualizar una experiencia existente', (done) => {
      const id = '1';
      const actualizado = { ...mockExperiencias[0], precio: 18.50 };

      service.actualizar(id, actualizado).subscribe(data => {
        expect(data.precio).toBe(18.50);
        done();
      });

      const req = httpMock.expectOne(`/api/app/experiencias/${id}`);
      expect(req.request.method).toBe('PUT');
      req.flush(actualizado);
    });
  });

  describe('eliminarExperiencia', () => {
    it('debería eliminar una experiencia', (done) => {
      const id = '1';

      service.eliminar(id).subscribe(() => {
        expect(true).toBe(true); // Operación exitosa
        done();
      });

      const req = httpMock.expectOne(`/api/app/experiencias/${id}`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('buscarPorNombre', () => {
    it('debería buscar experiencias por nombre', (done) => {
      const termino = 'Eiffel';

      service.buscar(termino).subscribe(data => {
        expect(data.length).toBeGreaterThan(0);
        expect(data[0].nombre).toContain('Eiffel');
        done();
      });

      const req = httpMock.expectOne(`/api/app/experiencias/buscar?q=${termino}`);
      req.flush([mockExperiencias[0]]);
    });
  });
});
```

---

## FotoPerfilService

Para gestionar fotos de perfil de usuarios.

### Archivo: `src/app/proxy/controllers/foto-perfil.service.spec.ts`

```typescript
import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { FotoPerfilService } from './foto-perfil.service';

describe('FotoPerfilService', () => {
  let service: FotoPerfilService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [FotoPerfilService]
    });

    service = TestBed.inject(FotoPerfilService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('obtenerFotoPerfil', () => {
    it('debería obtener la foto de perfil del usuario', (done) => {
      const mockFoto = {
        id: '1',
        url: 'https://...',
        nombreArchivo: 'perfil.jpg',
        tamaño: 102400,
        fechaSubida: new Date()
      };

      service.obtenerFotoPerfil().subscribe(data => {
        expect(data.url).toBeDefined();
        expect(data.nombreArchivo).toBe('perfil.jpg');
        done();
      });

      const req = httpMock.expectOne('/api/app/foto-perfil');
      expect(req.request.method).toBe('GET');
      req.flush(mockFoto);
    });

    it('debería manejar error cuando no hay foto', (done) => {
      service.obtenerFotoPerfil().subscribe(
        () => fail('debería haber fallado'),
        error => {
          expect(error.status).toBe(404);
          done();
        }
      );

      const req = httpMock.expectOne('/api/app/foto-perfil');
      req.flush('No hay foto de perfil', { status: 404, statusText: 'Not Found' });
    });
  });

  describe('subirFotoPerfil', () => {
    it('debería subir una nueva foto de perfil', (done) => {
      const mockFile = new File(['contenido'], 'perfil.jpg', { type: 'image/jpeg' });
      const formData = new FormData();
      formData.append('file', mockFile);

      service.subirFotoPerfil(mockFile).subscribe(data => {
        expect(data.url).toBeDefined();
        done();
      });

      const req = httpMock.expectOne('/api/app/foto-perfil/upload');
      expect(req.request.method).toBe('POST');
      req.flush({ url: 'https://...', nombreArchivo: 'perfil.jpg' });
    });

    it('debería rechazar archivos que no sean imagen', (done) => {
      const mockFile = new File(['contenido'], 'documento.pdf', { type: 'application/pdf' });

      service.subirFotoPerfil(mockFile).subscribe(
        () => fail('debería rechazar archivos no imagen'),
        error => {
          expect(error).toBeDefined();
          done();
        }
      );

      const req = httpMock.expectOne('/api/app/foto-perfil/upload');
      req.flush('Solo se aceptan imágenes', { status: 400, statusText: 'Bad Request' });
    });

    it('debería rechazar archivos muy grandes', (done) => {
      // Crear un archivo simulado muy grande (>5MB)
      const largeData = new ArrayBuffer(6 * 1024 * 1024);
      const mockFile = new File([largeData], 'large.jpg', { type: 'image/jpeg' });

      service.subirFotoPerfil(mockFile).subscribe(
        () => fail('debería rechazar archivos muy grandes'),
        error => {
          expect(error.status).toBe(413);
          done();
        }
      );

      const req = httpMock.expectOne('/api/app/foto-perfil/upload');
      req.flush('Archivo demasiado grande', { status: 413, statusText: 'Payload Too Large' });
    });
  });

  describe('eliminarFotoPerfil', () => {
    it('debería eliminar la foto de perfil', (done) => {
      service.eliminarFotoPerfil().subscribe(() => {
        expect(true).toBe(true);
        done();
      });

      const req = httpMock.expectOne('/api/app/foto-perfil');
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('actualizarFotoPerfil', () => {
    it('debería actualizar la foto de perfil con una nueva', (done) => {
      const mockFile = new File(['contenido'], 'nueva-perfil.jpg', { type: 'image/jpeg' });

      service.actualizarFotoPerfil(mockFile).subscribe(data => {
        expect(data.nombreArchivo).toBe('nueva-perfil.jpg');
        done();
      });

      const req = httpMock.expectOne('/api/app/foto-perfil/update');
      expect(req.request.method).toBe('PUT');
      req.flush({ url: 'https://...', nombreArchivo: 'nueva-perfil.jpg' });
    });
  });
});
```

---

## ✅ Checklist para Implementar Estos Tests

1. **Crear archivos .spec.ts** en la misma carpeta que cada servicio
2. **Importar TestBed y HttpClientTestingModule** en cada test
3. **Crear mock data** que represente datos reales
4. **Configurar beforeEach** con TestBed
5. **Hacer afterEach().verify()** para HttpTestingController
6. **Probar casos exitosos** (happy path)
7. **Probar casos de error** (error handling)
8. **Probar validaciones** (si aplica)
9. **Ejecutar**: `ng test`
10. **Verificar cobertura**: `ng test --code-coverage`

---

## 🔍 Patrones Importantes

### Para servicios HTTP:
```typescript
// SIEMPRE hacer esto
afterEach(() => {
  httpMock.verify(); // Verificar sin requests pendientes
});

// Usar expectOne para una petición
const req = httpMock.expectOne('/api/endpoint');

// Usar flush para enviar datos mock
req.flush(mockData);

// Para errores:
req.flush('Error', { status: 500, statusText: 'Server Error' });
```

---

## 📊 Ejecución de Todos los Tests

```bash
# Todos los tests
ng test --watch=false

# Solo servicios
ng test --include='**/*.service.spec.ts' --watch=false

# Con cobertura
ng test --no-watch --code-coverage
```

El coverage debería estar >80% en líneas y funciones.
