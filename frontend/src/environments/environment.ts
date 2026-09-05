/**
 * Development environment configuration.
 * apiUrl is empty because Angular's dev proxy (proxy.conf.json) forwards
 * all /api/* requests to http://localhost:5150 automatically.
 */
export const environment = {
  production: false,
  apiUrl: '',
  assetBaseUrl: 'http://localhost:5150',
};
