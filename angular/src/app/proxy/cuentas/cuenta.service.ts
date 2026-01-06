import { Injectable, inject } from '@angular/core';
import { RestService } from '@abp/ng.core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CuentaService {
  private rest = inject(RestService);

  eliminarMiCuenta(): Observable<void> {
    return this.rest.request<any, void>({
      method: 'POST',
      url: '/api/app/cuenta/eliminar-mi-cuenta', 
    });
  }
}