import { Component, Inject, AfterViewInit, OnInit, inject, Renderer2 } from '@angular/core';
import { DOCUMENT } from '@angular/common';
import { ConfigStateService, CoreModule, ReplaceableComponentsService } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { eThemeLeptonXComponents } from '@abp/ng.theme.lepton-x';
import { FotoPerfilService } from './services/foto-perfil.service';

import { NotificationsModule } from './shared/components/notifications/notifications.module';
import { CustomNavItemsComponent } from './shared/components/notifications/custom-nav-items/custom-nav-items.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CoreModule,
    ThemeSharedModule,
    NotificationsModule
  ],
  // --- BORRAMOS LA LÍNEA DE INTERNET STATUS AQUÍ ---
  template: `
    <abp-loader-bar></abp-loader-bar>
    <abp-dynamic-layout></abp-dynamic-layout>
  `,
})
export class AppComponent implements OnInit, AfterViewInit {
    // ... (El resto de tu código déjalo IGUAL, no lo toques)
    private config = inject(ConfigStateService);
    private fotoService = inject(FotoPerfilService);
    private renderer = inject(Renderer2);
    private replaceableComponents = inject(ReplaceableComponentsService);

    constructor(@Inject(DOCUMENT) private document: Document) {}

    ngOnInit() {
        this.replaceableComponents.add({
        component: CustomNavItemsComponent, 
        key: eThemeLeptonXComponents.NavItems,
        });
    }

    ngAfterViewInit() {
        this.config.getOne$('currentUser').subscribe(user => {
        if (user?.id) {
            this.colocarFotoEnNavbar(user.id);
        }
        });
    }

    colocarFotoEnNavbar(userId: string) {
        // ... (Tu lógica de fotos igual)
        const urlFoto = `${this.fotoService.obtenerUrlFoto(userId)}?t=${new Date().getTime()}`;
        const intervalo = setInterval(() => {
        const avatarElement = this.document.querySelector('.lpx-avatar');
        if (avatarElement) {
            clearInterval(intervalo);
            avatarElement.innerHTML = '';
            
            this.renderer.setStyle(avatarElement, 'width', '40px'); 
            this.renderer.setStyle(avatarElement, 'height', '40px');
            this.renderer.setStyle(avatarElement, 'border-radius', '50%');
            this.renderer.setStyle(avatarElement, 'overflow', 'hidden');
            this.renderer.setStyle(avatarElement, 'display', 'flex');
            this.renderer.setStyle(avatarElement, 'align-items', 'center');
            this.renderer.setStyle(avatarElement, 'justify-content', 'center');

            const img = this.renderer.createElement('img');
            this.renderer.setAttribute(img, 'src', urlFoto);
            this.renderer.setStyle(img, 'width', '100%');
            this.renderer.setStyle(img, 'height', '100%');
            this.renderer.setStyle(img, 'object-fit', 'cover'); 
            this.renderer.setStyle(img, 'display', 'block');
            
            this.renderer.listen(img, 'error', () => {
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