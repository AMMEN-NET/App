import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { RestService } from '@abp/ng.core';
import { EventosExternosService } from './eventos-externos.service';

describe('EventosExternosService', () => {
  let service: EventosExternosService;
  let restService: jasmine.SpyObj<RestService>;

  beforeEach(() => {
    const restSpy = jasmine.createSpyObj('RestService', ['request']);
    restSpy.request.and.returnValue(of([]));

    TestBed.configureTestingModule({
      providers: [
        EventosExternosService,
        { provide: RestService, useValue: restSpy }
      ]
    });

    service = TestBed.inject(EventosExternosService);
    restService = TestBed.inject(RestService) as jasmine.SpyObj<RestService>;
  });

  it('debería llamar al endpoint con latitud y longitud', () => {
    service.obtenerEventosPorUbicacion('48.8566', '2.3522').subscribe();

    expect(restService.request).toHaveBeenCalledWith(
      {
        method: 'POST',
        url: '/api/app/eventos-externos/obtener-eventos-por-ubicacion',
        params: { latitud: '48.8566', longitud: '2.3522' }
      },
      { apiName: 'Default' }
    );
  });

  it('debería mezclar config adicional', () => {
    service
      .obtenerEventosPorUbicacion('41.3874', '2.1686', { skipHandleError: true })
      .subscribe();

    expect(restService.request).toHaveBeenCalledWith(
      jasmine.objectContaining({
        method: 'POST',
        url: '/api/app/eventos-externos/obtener-eventos-por-ubicacion',
        params: { latitud: '41.3874', longitud: '2.1686' }
      }),
      jasmine.objectContaining({
        apiName: 'Default',
        skipHandleError: true
      })
    );
  });
});