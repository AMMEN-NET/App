import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
// Asegúrate de que estas rutas coincidan con dónde creaste realmente los archivos
import { ViajerosService } from '../proxy/viajeros/viajeros.service'; 
import { ViajeroDto } from '../proxy/viajeros'; 
// IMPORTANTE: Asegúrate de importar el DTO del perfil público. 
// Si no lo tienes en un archivo separado, avísame y te paso la interfaz para pegarla aquí mismo.
import { PerfilPublicoDto } from '../proxy/viajeros'; 
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

  // Signals existentes
  viajeros = signal<ViajeroDto[]>([]);
  cargando = signal(false);
  buscador = new FormControl('');

  // --- NUEVOS SIGNALS PARA LA MODAL ---
  perfilSeleccionado = signal<PerfilPublicoDto | null>(null);
  cargandoPerfil = signal(false); // Para mostrar un spinner dentro de la modal si tarda

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
    // Asumiendo que tu proxy espera un objeto con la propiedad 'filtro'
    // Si tu backend espera 'filter', cambia { filtro: filtro } por { filter: filtro }
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

  // --- NUEVOS MÉTODOS PARA LA MODAL ---

  abrirPerfil(id: string) {
    this.cargandoPerfil.set(true);
    
    // Llamamos al servicio para obtener el detalle completo
    this.viajerosService.getPerfilPublico(id).subscribe({
        next: (perfil) => {
            this.perfilSeleccionado.set(perfil);
            this.cargandoPerfil.set(false);
        },
        error: (err) => {
            console.error('Error al cargar perfil', err);
            this.cargandoPerfil.set(false);
            // Aquí podrías mostrar un toaster de error si quisieras
        }
    });
  }

  cerrarPerfil() {
    this.perfilSeleccionado.set(null);
  }
}