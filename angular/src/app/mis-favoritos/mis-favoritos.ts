import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router'; // Para el link de "volver"
import { ToasterService } from '@abp/ng.theme.shared';

// PROXIES
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos';
import { FavoritoDto } from '../proxy/favoritos/favoritos-dto'; // Asegúrate de que la ruta sea la que generó el comando abp

@Component({
  selector: 'app-mis-favoritos',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './mis-favoritos.html', // Asumo que se llama así tu archivo HTML
  styleUrls: ['./mis-favoritos.scss'] // Si tienes scss
})
export class MisFavoritosComponent implements OnInit {
  
  // INYECCIONES
  private readonly listaService = inject(ListaDeFavoritosService);
  private readonly toaster = inject(ToasterService);

  // SEÑALES (ESTADO)
  public favoritos = signal<FavoritoDto[]>([]);
  public estaCargando = signal<boolean>(true);
  public eliminandoId = signal<string | null>(null); // Para mostrar spinner en el botón de borrar específico

  ngOnInit(): void {
    this.cargarFavoritos();
  }

  // Carga la lista desde el Backend
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
      }
    });
  }

  // Elimina un favorito
  eliminar(id: string, nombreCiudad: string) {
    if (!confirm(`¿Seguro que quieres eliminar a ${nombreCiudad} de tus favoritos?`)) {
      return;
    }

    this.eliminandoId.set(id);

    this.listaService.eliminarDeFavoritos(id).subscribe({
      next: () => {
        this.toaster.info('Destino eliminado de la lista.', 'Eliminado');
        // Actualizamos la lista localmente filtrando el que borramos (para no recargar toda la pag)
        this.favoritos.update(lista => lista.filter(f => f.id !== id));
        this.eliminandoId.set(null);
      },
      error: (err) => {
        this.toaster.error('Ocurrió un error al eliminar.', 'Error');
        this.eliminandoId.set(null);
      }
    });
  }

  // Truco para generar una imagen "random" bonita basada en el nombre (para demo)
  obtenerImagenUrl(nombre: string): string {
    // Usamos un servicio de placeholders de paisajes
    return `https://source.unsplash.com/500x300/?city,${nombre},landmark`;
    // Nota: Si unsplash falla, puedes usar una imagen estática en assets
  }
}