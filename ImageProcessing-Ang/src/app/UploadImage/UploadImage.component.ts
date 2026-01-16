import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpEventType, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-upload-image',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './UploadImage.component.html',
  styleUrl: './UploadImage.component.css'
})
export class UploadImageComponent {
  selectedFile: File | null = null;
  imagePreviewUrl: string | ArrayBuffer | null = null;
  uploadProgress: number | null = null;
  uploadMessage: string | null = null;
  
  // Analiz sonucunu ve Dosya ID'sini tutacak değişkenler
  analysisResult: any = null; 
  uploadedFileId: string | null = null; // Backend'den gelen ID'yi burada saklayacağız
  showAnalysis: boolean = false;
  isSaving: boolean = false;

  constructor(
    private http: HttpClient, 
    private router: Router, 
    public authService: AuthService 
  ) {}

  onFileSelected(event: Event) {
    const element = event.target as HTMLInputElement;
    if (element.files && element.files.length > 0) {
      this.selectedFile = element.files[0];
      // Resetleme işlemleri
      this.uploadMessage = null;
      this.uploadProgress = null;
      this.analysisResult = null; 
      this.uploadedFileId = null;
      this.showAnalysis = false;

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviewUrl = reader.result;
      };
      reader.readAsDataURL(this.selectedFile);
    }
  }

  // MAVİ BUTON: SADECE ANALİZ ET
  onUpload() {
    if (!this.selectedFile) {
      this.uploadMessage = "Lütfen dosya seçin.";
      return;
    }

    const token = this.authService.getToken();
    const tenantId = this.authService.getTenantId();

    if (!token || !tenantId) return;

    this.uploadMessage = "Analiz ediliyor...";
    this.uploadProgress = 0;
    this.showAnalysis = false;

    const formData = new FormData();
    formData.append('File', this.selectedFile, this.selectedFile.name);
    formData.append('TanetId', tenantId);

    const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });

    // 1. Endpoint: AnalyzeFile (Sadece Analiz)
    this.http.post(`${this.authService.apiUrl}/File/AnalyzeFile`, formData, {
      headers: headers,
      reportProgress: true,
      observe: 'events'
    }).subscribe({
      next: (event: any) => {
        if (event.type === HttpEventType.UploadProgress && event.total) {
            this.uploadProgress = Math.round((100 * event.loaded) / event.total);
        } 
        else if (event.type === HttpEventType.Response) {
          const response = event.body;
          if (response.success) {
             this.uploadProgress = 100;
             this.uploadMessage = 'Analiz Tamamlandı. Lütfen kontrol edip KAYDET butonuna basın.';
             
             // ID ve JSON'u sakla
             this.analysisResult = response; 
             // C# modelinde File -> Id yolundan ID'yi alıyoruz
             this.uploadedFileId = response.file?.id || response.File?.Id; 
             this.showAnalysis = true;
          } else {
             this.uploadMessage = 'Hata: ' + response.message;
          }
        }
      },
      error: (err) => {
        this.uploadMessage = 'Sunucu Hatası: ' + err.message;
      }
    });
  }

  // SARI BUTON: VERİTABANINA KAYDET (Details Tablosuna)
  saveToDatabase() {
    if (!this.uploadedFileId) {
      alert("Önce analiz yapmalısınız!");
      return;
    }

    const token = this.authService.getToken();
    const tenantId = this.authService.getTenantId();
    this.isSaving = true;

    // Gönderilecek Veri: FileID ve JSON Sonucu
    const saveRequest = {
        FileId: this.uploadedFileId,
        TanetId: tenantId,
        // Getter'dan temizlenmiş JSON stringi alıyoruz
        AnalysisResult: this.formattedAnalysisResult 
    };

    const headers = new HttpHeaders({ 'Authorization': `Bearer ${token}` });

    // 2. Endpoint: SaveDetails (Kaydetme)
    this.http.post(`${this.authService.apiUrl}/File/SaveDetails`, saveRequest, {
      headers: headers
    }).subscribe({
      next: (response: any) => {
        this.isSaving = false;
        if (response.success || response.Success) {
          alert("Başarıyla veritabanına kaydedildi!");
          // İstersen burada Dashboard'a yönlendirebilirsin
          // this.router.navigate(['/dashboard']);
        } else {
          alert("Kayıt başarısız oldu.");
        }
      },
      error: (err) => {
        this.isSaving = false;
        alert("Kayıt sırasında hata: " + err.message);
      }
    });
  }

  // Yardımcı Getter: Ekranda ve Save işleminde kullanılacak temiz JSON
  get formattedAnalysisResult(): string {
    const rawData = this.analysisResult?.file?.analysisResult || this.analysisResult?.File?.AnalysisResult;
    if (!rawData) return '';
    try {
      let jsonString = rawData;
      if (typeof jsonString === 'string') {
        jsonString = jsonString.replace(/```json/g, '').replace(/```/g, '').trim();
      }
      // Doğrulamak için parse edip tekrar string yapıyoruz
      const jsonObj = JSON.parse(jsonString);
      return JSON.stringify(jsonObj, null, 2); 
    } catch (e) {
      return rawData;
    }
  }
  get safeFileName(): string {
    const f = this.analysisResult?.file || this.analysisResult?.File;
    return f?.fileName || f?.FileName || this.selectedFile?.name || '-';
  }

  get safeCreatedDate(): any {
    const f = this.analysisResult?.file || this.analysisResult?.File;

    return f?.createdDate || f?.CreatedDate || new Date();
  }
}