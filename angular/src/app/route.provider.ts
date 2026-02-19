import { RoutesService, eLayoutType } from '@abp/ng.core';
import { eThemeSharedRouteNames } from '@abp/ng.theme.shared'; 
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
      path: '/favoritos',
      name: 'Mis Favoritos',
      iconClass: 'fas fa-heart',
      order: 2,
      layout: eLayoutType.application,
    },
    {
      path: '/calificaciones',
      name: 'Mis Calificaciones',
      iconClass: 'fas fa-star',
      order: 3,
      layout: eLayoutType.application,
    },
    {
      path: '/viajeros',
      name: 'Comunidad de Viajeros',
      iconClass: 'fas fa-users',
      order: 4,
      layout: eLayoutType.application,
    },
    {
      path: '/experiencias',
      name: 'Experiencias',
      iconClass: 'fas fa-compass',
      order: 5,
      layout: eLayoutType.application,
    },
    // --- PANEL DE ADMIN ---
    {
      path: '/admin/dashboard',
      name: 'Panel de Control',
      parentName: eThemeSharedRouteNames.Administration, 
      layout: eLayoutType.application,
      iconClass: 'fa fa-chart-line',
      order: 1, 
      // 👇 AQUI ESTÁ EL CAMBIO (Opción A)
      // Usamos el permiso de "Ver Usuarios" que solo tienen los Admins
      requiredPolicy: 'AbpIdentity.Users', 
    },
  ]);
}