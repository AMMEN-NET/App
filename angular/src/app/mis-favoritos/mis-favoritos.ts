import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { ToasterService } from '@abp/ng.theme.shared';

// PROXIES
import { ListaDeFavoritosService } from '../proxy/lista-de-favoritos';
import { FavoritoDto } from '../proxy/favoritos/favoritos-dto';

// PROXIES NUEVOS PARA TICKETMASTER
import { EventosExternosService } from '../proxy/external-service'; 
import { EventoTicketmasterDto } from '../proxy/external-service/models'; 

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
  
  // NUEVA INYECCIÓN (Adaptada a tu estilo)
  private readonly eventosExternosService = inject(EventosExternosService);

  // SEÑALES (ESTADO ORIGINAL)
  public favoritos = signal<FavoritoDto[]>([]);
  public estaCargando = signal<boolean>(true);
  public eliminandoId = signal<string | null>(null);

  // --- VARIABLES NUEVAS PARA LOS EVENTOS ---
  eventos: EventoTicketmasterDto[] = [];
  mostrarModalEventos: boolean = false;
  cargandoEventos: boolean = false;

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

  // --- MÉTODOS NUEVOS PARA BUSCAR EVENTOS ---
  abrirModalEventos(latitud: string, longitud: string) {
    this.mostrarModalEventos = true;
    this.cargandoEventos = true;
    this.eventos = []; // Limpiamos eventos anteriores

    this.eventosExternosService.obtenerEventosPorUbicacion(latitud, longitud).subscribe({
      next: (data) => {
        this.eventos = data;
        this.cargandoEventos = false;
      },
      error: (err) => {
        console.error('Error al cargar eventos', err);
        this.cargandoEventos = false;
      }
    });
  }

  cerrarModalEventos() {
    this.mostrarModalEventos = false;
  }
}