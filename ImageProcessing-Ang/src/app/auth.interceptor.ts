import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core'; // PLATFORM_ID ekledik
import { isPlatformBrowser } from '@angular/common'; // isPlatformBrowser ekledik
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const platformId = inject(PLATFORM_ID); // Kodun nerede çalıştığını (Server mı Browser mı) öğreniyoruz

  let authReq = req;

  // KONTROL: Sadece tarayıcı ortamındaysak localStorage'a bak
  if (isPlatformBrowser(platformId)) {
    const token = localStorage.getItem('token');

    if (token) {
      authReq = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
  }

  return next(authReq).pipe(
    catchError((err: any) => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        
        // Yönlendirmeyi de sadece tarayıcıda yapmalıyız
        if (isPlatformBrowser(platformId)) {
            console.warn('Token geçersiz, çıkış yapılıyor...');
            localStorage.removeItem('token');
            router.navigate(['/login']);
        }
      }
      return throwError(() => err);
    })
  );
};