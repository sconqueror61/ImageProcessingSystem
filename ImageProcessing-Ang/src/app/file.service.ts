import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class FileService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  // Fetch paginated dashboard details
  getDashboardDetails(page: number): Observable<any> {
    const tanetId = this.authService.getTenantId();
    // Assuming page size is 10 based on your HTML logic
    return this.http.get(`${this.authService.apiUrl}/File/dashboard-details?tanetId=${tanetId}&page=${page}&pageSize=10`);
  }

  // Download a specific file
  downloadFile(fileId: string): Observable<Blob> {
    const tanetId = this.authService.getTenantId();
    return this.http.get(`${this.authService.apiUrl}/File/${fileId}/download?tanetId=${tanetId}`, {
      responseType: 'blob'
    });
  }

  // Delete a specific file
  deleteFile(fileId: string): Observable<any> {
    const tanetId = this.authService.getTenantId();
    return this.http.delete(`${this.authService.apiUrl}/File/${fileId}?tanetId=${tanetId}`);
  }
}