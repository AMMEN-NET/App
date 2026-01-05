import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
// Asegúrate de que estas rutas coincidan con dónde creaste realmente los archivos
import { ViajerosService } from '../proxy/viajeros/viajeros.service'; 
import { ViajeroDto } from '../proxy/viajeros'; // O donde tengas el DTO
import { FotoPerfilService } from '../services/foto-perfil.service';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';

@Component({
  selector: 'app-viajeros',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './viajeros.component.html'
})
export class ViajerosComponent implements OnInit {
  private viajerosService = inject(ViajerosService);
  private fotoService = inject(FotoPerfilService); 

  viajeros = signal<ViajeroDto[]>([]);
  cargando = signal(false);
  buscador = new FormControl('');

  ngOnInit() {
    this.cargarViajeros();

    this.buscador.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(valor => {
      this.cargarViajeros(valor || '');
    });
  }

  cargarViajeros(filtro: string = '') {
    this.cargando.set(true);
    // NOTA: Si tu servicio "proxy" fue generado automáticamente, es posible que getList espere un objeto 
    // en lugar de un string (ej: { filter: filtro }). 
    // Si te da error de compilación aquí, cámbialo por: .getList({ filter: filtro })
    this.viajerosService.getList(filtro).subscribe({
      next: (data) => {
        this.viajeros.set(data);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false)
    });
  }

  obtenerFoto(id: string) {
    return this.fotoService.obtenerUrlFoto(id);
  }

  // --- NUEVO MÉTODO PARA CORREGIR EL ERROR HTML ---
  manejarErrorFoto(event: any) {
    // Cuando la imagen falla (404), reemplazamos el contenido del contenedor
    const imgElement = event.target;
    const parent = imgElement.parentElement;
    
    // Ocultamos la imagen rota
    imgElement.style.display = 'none';
    
    // Inyectamos el ícono gris
    parent.innerHTML = `
      <div class="w-full h-full flex items-center justify-center bg-gray-100 text-gray-300">
        <i class="fa fa-user fa-2x"></i>
      </div>
    `;
  }
}