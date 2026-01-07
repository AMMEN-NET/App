import { provideAbpCore, withOptions, ReplaceableComponentsService } from '@abp/ng.core'; // <-- Importar ReplaceableComponentsService
import { provideAbpOAuth } from '@abp/ng.oauth';
import { provideSettingManagementConfig } from '@abp/ng.setting-management/config';
import { provideFeatureManagementConfig } from '@abp/ng.feature-management';
import { provideAbpThemeShared } from '@abp/ng.theme.shared';
import { provideIdentityConfig } from '@abp/ng.identity/config';
import { provideAccountConfig } from '@abp/ng.account/config';
import { eAccountComponents } from '@abp/ng.account'; // <-- Importar Enum de componentes
import { registerLocale } from '@abp/ng.core/locale';
import { provideThemeLeptonX } from '@abp/ng.theme.lepton-x';
import { provideSideMenuLayout } from '@abp/ng.theme.lepton-x/layouts';
import { provideLogo, withEnvironmentOptions } from "@volo/ngx-lepton-x.core";
import { ApplicationConfig, APP_INITIALIZER, inject } from '@angular/core'; // <-- Importar APP_INITIALIZER e inject
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { environment } from '../environments/environment';
import { APP_ROUTES } from './app.routes';
import { APP_ROUTE_PROVIDER } from './route.provider';
import { MiPerfilPersonalizadoComponent } from './mi-perfil-personalizado/mi-perfil-personalizado.component'; // <-- Importar tu componente

// Función para inicializar el reemplazo
function configureReplaceableComponents() {
  const replaceableComponents = inject(ReplaceableComponentsService);
  
  replaceableComponents.add({
    component: MiPerfilPersonalizadoComponent,
    key: eAccountComponents.PersonalSettings // Reemplaza la pestaña "Información Personal"
  });
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(APP_ROUTES),
    APP_ROUTE_PROVIDER,
    provideAnimations(),
    
    // Proveedor para ejecutar el reemplazo al inicio
    {
      provide: APP_INITIALIZER,
      useFactory: () => configureReplaceableComponents,
      multi: true,
    },

    provideAbpCore(
      withOptions({
        environment,
        registerLocaleFn: registerLocale(),
      }),
    ),
    provideAbpOAuth(),
    provideIdentityConfig(),
    provideSettingManagementConfig(),
    provideFeatureManagementConfig(),
    provideThemeLeptonX(),
    provideSideMenuLayout(),
    provideLogo(withEnvironmentOptions(environment)),
    provideAccountConfig(),
    provideAbpThemeShared(),
  ]
};