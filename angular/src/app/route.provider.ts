import { RoutesService, eLayoutType } from '@abp/ng.core';
import { inject, provideAppInitializer } from '@angular/core';

export const APP_ROUTE_PROVIDER = [
  provideAppInitializer(() => {
    configureRoutes();
  }),
];

function configureRoutes() {
  const routes = inject(RoutesService);
  routes.add([
      {
        path: '/',
        name: '::Menu:Home',
        iconClass: 'fas fa-home',
        order: 1,
        layout: eLayoutType.application,
      },
      {
        path: '/favoritos',       // La URL a la que llevará
        name: 'Mis Favoritos',    // Lo que se lee en la sidebar
        iconClass: 'fas fa-heart', // El icono (FontAwesome)
        order: 2,                 // El orden (para que salga debajo del Home)
        layout: eLayoutType.application,
      },
  ]);
}
