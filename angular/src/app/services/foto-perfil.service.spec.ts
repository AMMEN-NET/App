import { TestBed } from '@angular/core/testing';
import { FotoPerfilService } from './foto-perfil.service';

describe('FotoPerfilService', () => {
  let service: FotoPerfilService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [FotoPerfilService]
    });
    service = TestBed.inject(FotoPerfilService);
  });

  describe('obtenerUrlFoto', () => {
    it('debería retornar URL del backend con usuarioId válido', () => {
      const usuarioId = 'user-123';
      const url = service.obtenerUrlFoto(usuarioId);

      expect(url).toContain('/api/app/foto-perfil/obtener/');
      expect(url).toContain(usuarioId);
    });

    it('debería retornar placeholder cuando no hay usuarioId', () => {
      const url = service.obtenerUrlFoto();

      expect(url).toContain('data:image/svg+xml');
      expect(url).toContain('👤');
    });

    it('debería retornar placeholder cuando se pasa undefined', () => {
      const url = service.obtenerUrlFoto(undefined);

      expect(url).toContain('data:image/svg+xml');
    });
  });

  describe('getPlaceholder', () => {
    it('debería retornar SVG placeholder válido', () => {
      const placeholder = service.getPlaceholder();

      expect(placeholder).toContain('data:image/svg+xml');
      expect(placeholder).toContain('👤');
      expect(placeholder).toContain('%3Csvg');
    });

    it('debería retornar el mismo placeholder siempre', () => {
      const placeholder1 = service.getPlaceholder();
      const placeholder2 = service.getPlaceholder();

      expect(placeholder1).toBe(placeholder2);
    });
  });
});
