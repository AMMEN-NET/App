import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44326/',
  redirectUri: baseUrl,
  clientId: 'AmmenTravel_App',
  responseType: 'code',
  scope: 'offline_access AmmenTravel',
  requireHttps: true,
};

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'AmmenTravel',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44326',
      rootNamespace: 'AmmenTravel',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
  remoteEnv: {
    url: '/getEnvConfig',
    mergeStrategy: 'deepmerge'
  }
} as Environment;
