import { Component, Inject, AfterViewInit, inject, Renderer2 } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { ConfigStateService, CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { FotoPerfilService } from './services/foto-perfil.service'; 

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CoreModule,
    ThemeSharedModule
  ],
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
  `,
})
export class AppComponent implements AfterViewInit {
  private config = inject(ConfigStateService);
  private fotoService = inject(FotoPerfilService);
  private renderer = inject(Renderer2);
  
  constructor(@Inject(DOCUMENT) private document: Document) {}

  ngAfterViewInit() {
    this.config.getOne$('currentUser').subscribe(user => {
      if (user?.id) {
        this.colocarFotoEnNavbar(user.id);
      }
    });
  }

  colocarFotoEnNavbar(userId: string) {
    const urlFoto = `${this.fotoService.obtenerUrlFoto(userId)}?t=${new Date().getTime()}`;

    const intervalo = setInterval(() => {
      const avatarElement = this.document.querySelector('.lpx-avatar');

      if (avatarElement) {
        clearInterval(intervalo);

        // 1. Preparamos el contenedor (la "caja")
        avatarElement.innerHTML = '';
        
        // --- ESTILOS CLAVE PARA EL CONTENEDOR ---
        // Forzamos un tamaño fijo pequeño (puedes ajustar los 40px si lo quieres más grande o más chico)
        this.renderer.setStyle(avatarElement, 'width', '40px'); 
        this.renderer.setStyle(avatarElement, 'height', '40px');
        // Aseguramos que sea redondo y recorte lo que sobra
        this.renderer.setStyle(avatarElement, 'border-radius', '50%');
        this.renderer.setStyle(avatarElement, 'overflow', 'hidden');
        // Centramos la imagen dentro
        this.renderer.setStyle(avatarElement, 'display', 'flex');
        this.renderer.setStyle(avatarElement, 'align-items', 'center');
        this.renderer.setStyle(avatarElement, 'justify-content', 'center');
        // ----------------------------------------

        // 2. Creamos y estilizamos la imagen
        const img = this.renderer.createElement('img');
        this.renderer.setAttribute(img, 'src', urlFoto);
        
        // Estilos de la imagen para que llene el contenedor sin deformarse
        this.renderer.setStyle(img, 'width', '100%');
        this.renderer.setStyle(img, 'height', '100%');
        this.renderer.setStyle(img, 'object-fit', 'cover'); 
        this.renderer.setStyle(img, 'display', 'block');

        // Error handler: si falla la carga, volvemos a poner el ícono
        this.renderer.listen(img, 'error', () => {
           // Reseteamos estilos para que el ícono se vea bien
           this.renderer.removeStyle(avatarElement, 'width');
           this.renderer.removeStyle(avatarElement, 'height');
           this.renderer.removeStyle(avatarElement, 'overflow');
           avatarElement.innerHTML = '<i class="fa fa-user"></i>'; 
        });

        this.renderer.appendChild(avatarElement, img);
      }
    }, 500);

    setTimeout(() => clearInterval(intervalo), 10000);
  }
}