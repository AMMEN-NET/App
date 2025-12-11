import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';

// PROXIES
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos';
import { FavoritoDto } from '../proxy/favoritos/favoritos-dto';

@Component({
  selector: 'app-mis-favoritos',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './mis-favoritos.html',
  // styleUrls: ['./mis-favoritos.scss'] // Ya no es necesario si usamos solo Tailwind en el HTML
})
export class MisFavoritosComponent implements OnInit {
  
  // INYECCIONES
  private readonly listaService = inject(ListaDeFavoritosService);
  private readonly toaster = inject(ToasterService);

  // SEÑALES (ESTADO)
  public favoritos = signal<FavoritoDto[]>([]);
  public estaCargando = signal<boolean>(true);
  public eliminandoId = signal<string | null>(null);

  ngOnInit(): void {
    this.cargarFavoritos();
  }

  cargarFavoritos() {
    this.estaCargando.set(true);
    this.listaService.obtenerFavoritos().subscribe({
      next: (lista) => {
        this.favoritos.set(lista);
        this.estaCargando.set(false);
      },
      error: (err) => {
        console.error(err);
        this.estaCargando.set(false);
        this.toaster.error('Error al cargar la lista.', 'Error');
      }
    });
  }

  eliminar(id: string, nombreCiudad: string) {
    // Usamos un confirm nativo por simplicidad, se puede mejorar con un modal después
    if (!confirm(`¿Deseas quitar a ${nombreCiudad} de tus favoritos?`)) {
      return;
    }

    this.eliminandoId.set(id);

    this.listaService.eliminarDeFavoritos(id).subscribe({
      next: () => {
        this.toaster.info('Destino eliminado.', 'Adiós');
        this.favoritos.update(lista => lista.filter(f => f.id !== id));
        this.eliminandoId.set(null);
      },
      error: (err) => {
        this.toaster.error('No se pudo eliminar.', 'Error');
        this.eliminandoId.set(null);
      }
    });
  }
}