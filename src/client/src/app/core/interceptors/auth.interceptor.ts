import { HttpHandlerFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';

export function authInterceptor(
  request: HttpRequest<unknown>, 
  next: HttpHandlerFn
) {
  const token = localStorage.getItem('token');

  if (token) {
    const authRequest = request.clone({
      headers: request.headers.set('Authorization', `Bearer ${token}`)
    });
    return next(authRequest);
  }

  return next(request);
}