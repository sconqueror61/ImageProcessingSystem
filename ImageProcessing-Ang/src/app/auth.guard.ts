import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = (route) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  const requiredRole = route.data?.['role'];
  if (requiredRole) {
    const userRole = authService.getUserRole();

    if (requiredRole === 'admin' && userRole?.toLowerCase() !== 'admin') {
      router.navigate(['/upload']);
      return false;
    }
  }

  return true;
};