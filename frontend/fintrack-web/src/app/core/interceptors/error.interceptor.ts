import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Não force logout em endpoints de auth — credencial inválida retorna 401
      // e o componente de login precisa tratar o erro localmente.
      const isAuthEndpoint = req.url.includes('/auth/');

      if (error.status === 401 && !isAuthEndpoint) {
        // Token expirado ou inválido em rota protegida → redireciona para login.
        auth.logout();
      }

      return throwError(() => error);
    })
  );
};
