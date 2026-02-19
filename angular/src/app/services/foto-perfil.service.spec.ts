import { TestBed } from '@angular/core/testing';
import { FotoPerfilService } from './foto-perfil.service';

describe('FotoPerfilService', () => {
  let service: FotoPerfilService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FotoPerfilService]
    });
    service = TestBed.inject(FotoPerfilService);
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
  });

  describe('subirFoto', () => {
    it('debería convertir archivo a base64 y guardar en localStorage', (done) => {
      const mockFile = new File(['contenido'], 'avatar.jpg', { type: 'image/jpeg' });

      service.subirFoto(mockFile).subscribe({
        next: (base64) => {
          expect(base64).toContain('data:image/jpeg');
          expect(localStorage.getItem('foto-perfil-usuario')).toContain('data:image/jpeg');
          done();
        },
        error: () => fail('No debería rechazar archivos válidos')
      });
    });

    it('debería rechazar archivos que no sean imágenes', (done) => {
      const mockFile = new File(['contenido'], 'documento.pdf', { type: 'application/pdf' });

      service.subirFoto(mockFile).subscribe({
        next: () => fail('Debería rechazar archivos no-imagen'),
        error: (err) => {
          expect(err.message).toContain('debe ser una imagen');
          done();
        }
      });
    });

    it('debería rechazar archivos mayores a 5MB', (done) => {
      const largeBuffer = new ArrayBuffer(6 * 1024 * 1024 + 1);
      const mockFile = new File([largeBuffer], 'huge.jpg', { type: 'image/jpeg' });

      service.subirFoto(mockFile).subscribe({
        next: () => fail('Debería rechazar archivos > 5MB'),
        error: (err) => {
          expect(err.message).toContain('no debe superar 5MB');
          done();
        }
      });
    });

    it('debería permitir archivo de 1MB', (done) => {
      const buffer = new ArrayBuffer(1 * 1024 * 1024);
      const mockFile = new File([buffer], 'large.jpg', { type: 'image/jpeg' });

      service.subirFoto(mockFile).subscribe({
        next: (base64) => {
          expect(base64).toBeTruthy();
          done();
        },
        error: () => fail('Debería permitir archivo de 1MB')
      });
    });

    it('debería sobrescribir foto anterior en localStorage', (done) => {
      const mockFile1 = new File(['contenido1'], 'avatar1.jpg', { type: 'image/jpeg' });
      const mockFile2 = new File(['contenido2'], 'avatar2.jpg', { type: 'image/jpeg' });

      service.subirFoto(mockFile1).subscribe({
        next: (base64_1) => {
          service.subirFoto(mockFile2).subscribe({
            next: (base64_2) => {
              expect(base64_1).not.toBe(base64_2);
              expect(localStorage.getItem('foto-perfil-usuario')).toBe(base64_2);
              done();
            }
          });
        }
      });
    });
  });

  describe('obtenerUrlFoto', () => {
    it('debería retornar foto guardada en localStorage', () => {
      const base64Mock = 'data:image/jpeg;base64,/9j/4AAQSkZJRg==';
      localStorage.setItem('foto-perfil-usuario', base64Mock);

      const url = service.obtenerUrlFoto();

      expect(url).toBe(base64Mock);
    });

    it('debería retornar placeholder si no hay foto guardada', () => {
      const url = service.obtenerUrlFoto();

      expect(url).toContain('data:image/svg+xml');
      expect(url).toContain('👤');
    });
  });

  describe('eliminarFoto', () => {
    it('debería eliminar foto de localStorage', () => {
      localStorage.setItem('foto-perfil-usuario', 'data:image/jpeg;base64,test123');

      service.eliminarFoto();

      expect(localStorage.getItem('foto-perfil-usuario')).toBeNull();
    });
  });

  describe('tieneFoto', () => {
    it('debería retornar true si hay foto guardada', () => {
      localStorage.setItem('foto-perfil-usuario', 'data:image/jpeg;base64,test123');

      expect(service.tieneFoto()).toBe(true);
    });

    it('debería retornar false si no hay foto guardada', () => {
      expect(service.tieneFoto()).toBe(false);
    });
  });

  describe('Integración', () => {
    it('debería completar flujo: subir → obtener → verificar', (done) => {
      const mockFile = new File(['contenido'], 'avatar.jpg', { type: 'image/jpeg' });

      service.subirFoto(mockFile).subscribe({
        next: (base64) => {
          expect(service.tieneFoto()).toBe(true);
          expect(service.obtenerUrlFoto()).toBe(base64);
          done();
        }
      });
    });

    it('debería manejar múltiples subidas secuenciales', (done) => {
      const file1 = new File(['contenido1'], 'avatar1.jpg', { type: 'image/jpeg' });
      const file2 = new File(['contenido2'], 'avatar2.jpg', { type: 'image/jpeg' });

      service.subirFoto(file1).subscribe({
        next: (base64_1) => {
          service.subirFoto(file2).subscribe({
            next: (base64_2) => {
              expect(service.obtenerUrlFoto()).toBe(base64_2);
              expect(base64_1).not.toBe(base64_2);
              done();
            }
          });
        }
      });
    });
  });
});
