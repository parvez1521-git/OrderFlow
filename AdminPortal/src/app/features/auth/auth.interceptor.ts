/** HTTP interceptor that adds the correct development auth header to API requests. */
import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  if (req.url.startsWith('http://localhost:6200/api')) {
    const gatewayCredentials = btoa('orderflow-admin:orderflow-dev-password');
    return next(req.clone({ setHeaders: { Authorization: `Basic ${gatewayCredentials}` } }));
  }

  const token = localStorage.getItem('orderflow.admin.token');
  return next(token ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req);
};
