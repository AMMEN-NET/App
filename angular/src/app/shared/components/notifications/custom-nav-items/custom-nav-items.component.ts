import { Component, inject } from '@angular/core';
import { AuthService, ConfigStateService, CurrentUserDto } from '@abp/ng.core';
import { Observable } from 'rxjs';
import { FotoPerfilService } from '../../../../services/foto-perfil.service';

@Component({
  selector: 'app-custom-nav-items',
  standalone: false,
  templateUrl: './custom-nav-items.component.html'
})
export class CustomNavItemsComponent {
  private authService = inject(AuthService);
  private configState = inject(ConfigStateService);
  private fotoService = inject(FotoPerfilService);

  // Observable del usuario
  currentUser$: Observable<CurrentUserDto> = this.configState.getOne$('currentUser');
  
  isUserMenuOpen = false;

  toggleUserMenu() {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  closeUserMenu() {
    this.isUserMenuOpen = false;
  }

  logout() {
    this.authService.logout().subscribe();
  }

  // --- NUEVO: Método para ir al Login ---
  login() {
    this.authService.navigateToLogin();
  }

  getFotoUrl(userId: string): string {
    return this.fotoService.obtenerUrlFoto(userId);
  }
}