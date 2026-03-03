import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { NotificacionService } from '../../../../proxy/notificaciones/notificacion.service';
import { PreferenciasNotificacionService } from '../../../../proxy/notificaciones/preferencias-notificacion.service';
import { NotificacionDto, FrecuenciaNotificacion } from '../../../../proxy/notificaciones/models';

@Component({
  selector: 'app-notifications-dropdown',
  standalone: false,
  templateUrl: './notifications-dropdown.component.html',
})
export class NotificationsDropdownComponent implements OnInit, OnDestroy {
  isOpen = false;
  unreadCount = 0;
  notificaciones: NotificacionDto[] = []; 
  pollingInterval: any;
  esFrecuenciaSemanal = false;
  
  // Variable para el temporizador del mouse
  closeTimer: any;

  constructor(
    private notificacionService: NotificacionService,
    private preferenciasService: PreferenciasNotificacionService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.cargarDatos();
    this.cargarPreferencias();
    this.pollingInterval = setInterval(() => {
      this.cargarDatos();
    }, 60000);
  }

  ngOnDestroy(): void {
    if (this.pollingInterval) clearInterval(this.pollingInterval);
    if (this.closeTimer) clearTimeout(this.closeTimer);
  }

  cargarDatos() {
    this.notificacionService.getCantidadNoLeidas().subscribe(count => {
      this.unreadCount = count;
    });
    this.notificacionService.getMisNotificaciones().subscribe(list => {
      this.notificaciones = list;
    });
  }

  cargarPreferencias() {
    this.preferenciasService.getMiPreferencia().subscribe(prefs => {
      this.esFrecuenciaSemanal = prefs.frecuencia === FrecuenciaNotificacion.ResumenSemanal;
    });
  }

  // --- NUEVA LÓGICA DE HOVER ---

onMouseEnter() {
    // Si había una orden de cerrar, la cancelamos porque el usuario volvió
    if (this.closeTimer) {
        clearTimeout(this.closeTimer);
    }
}

  onMouseLeave() {
    // Esto da tiempo al usuario de mover el mouse del icono al panel sin que se cierre.
    this.closeTimer = setTimeout(() => {
      this.isOpen = false;
    }, 1200);
  }

  // Mantenemos el toggle manual por si está en celular (donde no hay hover)
  toggleDropdown() {
    this.isOpen = !this.isOpen;
    if (this.isOpen) this.cargarDatos();
  }

  // --- ACCIONES ---

  clickNotificacion(notif: NotificacionDto) {
    if (!notif.leida && notif.id) {
      this.notificacionService.marcarComoLeida(notif.id).subscribe(() => {
        notif.leida = true;
        this.unreadCount = Math.max(0, this.unreadCount - 1);
      });
    }

    // Cerramos inmediatamente al hacer click
    this.isOpen = false;

    if (notif.linkReferencia) {
      this.router.navigateByUrl(notif.linkReferencia);
    }
  }

  marcarTodas() {
    this.notificacionService.marcarTodasComoLeidas().subscribe(() => {
      this.unreadCount = 0;
      this.notificaciones.forEach(n => n.leida = true);
    });
  }
}