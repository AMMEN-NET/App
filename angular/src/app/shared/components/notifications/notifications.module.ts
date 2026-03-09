import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { RouterModule } from '@angular/router';

// Importamos tus componentes
import { NotificationsDropdownComponent } from './notifications-dropdown/notifications-dropdown.component';
import { CustomNavItemsComponent } from './custom-nav-items/custom-nav-items.component';

@NgModule({
  declarations: [
    NotificationsDropdownComponent,
    CustomNavItemsComponent
  ],
  imports: [
    CommonModule,
    CoreModule,
    ThemeSharedModule,
    RouterModule
  ],
  exports: [
    CustomNavItemsComponent,
    NotificationsDropdownComponent
  ]
})
export class NotificationsModule { }