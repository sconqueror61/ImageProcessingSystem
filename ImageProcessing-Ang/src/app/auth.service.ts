import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  public apiUrl = 'https://localhost:7189/api';

  constructor(
    private http: HttpClient, 
    private router: Router,
    @Inject(PLATFORM_ID) private platformId: Object 
  ) {}

  getUserRole(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      const token = localStorage.getItem('token');
      if (!token) return null;

      try {
        const payloadBase64 = token.split('.')[1];
        const decodedJson = atob(payloadBase64);
        const payload = JSON.parse(decodedJson);

        return payload.role || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'User';
      } catch (e) {
        return null;
      }
    }
    return null;
  }

  redirectBasedOnRole() {
    const role = this.getUserRole();

    if (role) {
      const normalizedRole = role.toLowerCase();

      if (normalizedRole === 'admin') {
        this.router.navigate(['/admin-dashboard']);
      } 
      else if (normalizedRole === 'company') {
        this.router.navigate(['/company-dashboard']);
      } 
      else {
        this.router.navigate(['/upload']); 
      }
    } else {
      this.router.navigate(['/login']);
    }
  }
isLoggedIn(): boolean {
  if (typeof localStorage !== 'undefined') {
    return !!localStorage.getItem('token'); 
  }
  return false;
}

  logout() {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem('token');
      localStorage.removeItem('tenantId');
    }
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('token');
    }
    return null;
  }

  getTenantId(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem('tenantId');
    }
    return null;
  }

  login(credentials: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/login`, credentials).pipe(
      tap((res: any) => {
        if (res.token && isPlatformBrowser(this.platformId)) {
          localStorage.setItem('token', res.token);
          const tenantId = this.getTenantIdFromToken(res.token);
          if (tenantId) {
            localStorage.setItem('tenantId', tenantId);
          }
        }
      })
    );
  }

  private getTenantIdFromToken(token: string): string | null {
    try {
      const payloadBase64 = token.split('.')[1];
      const decodedJson = atob(payloadBase64);
      const payload = JSON.parse(decodedJson);
      return payload.tenant_id || payload.TanetId || null;
    } catch (e) {
      return null;
    }
  }

  register(userData: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/Auth/register`, userData);
  }

createTenant(tenantData: { name: string, address: string }): Observable<any> {
  return this.http.post(`${this.apiUrl}/Tenant/register`, tenantData);
}

  getTenants(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/Tenant/list`);
  }
}